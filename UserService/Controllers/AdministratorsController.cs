using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Entities;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using UserService.Models;
using Microsoft.AspNetCore.Identity;
using UserService.Interfaces;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiVersion("2.0")]
    [Authorize(Roles = "ADMIN")]
    [Route("api/v{version:apiVersion}/Administrators")]
    [ApiController]
    public class AdministratorsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TelephoneDbContext _context;
        private readonly RedisService _redisService;

        public AdministratorsController(UserManager<IdentityUser> userManager, TelephoneDbContext context, RedisService redisService)
        {
            _userManager = userManager;
            _context = context;
            _redisService = redisService;
        }

        // GET: api/Administrators
        [HttpGet]
        public async Task<IActionResult> GetAdmins()
        {
            var admins = await _userManager.GetUsersInRoleAsync(UserRoles.ADMIN);
            return Ok(admins);
        }

        // GET: api/Administrators/5
        [HttpGet("subscribers")]
        public async Task<ActionResult> GetSubscribers()
        {
            var admins = await _userManager.GetUsersInRoleAsync(UserRoles.SUBSCRIBER);
            return Ok(admins);
        }

        // GET: api/Administrators/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IdentityUser>> GetSubscriber(string id)
        {
            // Check cache first
            var cachedUser = _redisService.GetValue<IdentityUser>(id);
            if (cachedUser != null)
            {
                return Ok(cachedUser);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(); // User not found
            }

            // Cache the user data
            _redisService.SetValue(id, user);
            Response.Headers.Add("Cache-Control", "public, max-age=3600"); // Cache for 1 hour
            Response.Headers.Add("ETag", GenerateETag(user)); // Generate ETag


            return Ok(user);
        }

        // GET: api/Administrators/unpaid-accounts
        [HttpGet("unpaid-accounts")]
        public async Task<ActionResult> GetUnpaidAccounts()
        {
            try
            {
                // Query the database for records with status false
                var records = await _context.Bills
                    .Where(r => r.Status == false)
                    .ToListAsync();

                return Ok(records);
            }
            catch (Exception ex)
            {
                return BadRequest("Error while get");
            }
        }

        // GET: api/Administrators/{id}/user-unpaid-accounts
        [HttpGet("{id}/user-unpaid-accounts")]
        public async Task<ActionResult<Bill>> GetUnpaidAccount(string id)
        {
            try
            {
                // Query the database for records with status false and matching the specified ID
                var records = await _context.Bills
                    .Where(r => r.Status == false && r.SubscriberId == id)
                    .ToListAsync();

                return Ok(records);
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return an error response
                return BadRequest("Error while search");
            }
        }

        // POST: api/Administrators/create-subscriber
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create-subscriber")]
        public async Task<ActionResult<string>> CreateSubscriber([FromBody] RegisterRequestModel registerRequestModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User { UserName = registerRequestModel.Name };
            var result = await _userManager.CreateAsync(user, registerRequestModel.Password);

            if (!result.Succeeded)
            {
                return BadRequest("Error while creating");
            }

            // Add user to the "SUBSCRIBER" role
            await _userManager.AddToRoleAsync(user, "SUBSCRIBER");

            // Create a corresponding entry in the "Subscribers" table
            var subscriber = new Subscriber { id = user.Id, Name = registerRequestModel.Name, Password = registerRequestModel.Password };
            _context.Subscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            // Cache the new subscriber
            _redisService.SetValue(user.Id, user);

            return Ok(new { message = "Registration successful" });
        }

        // DELETE: api/Administrators/{id}/delete-subscriber
        [HttpDelete("{id}/delete-subscriber")]
        public async Task<IActionResult> DeleteSubscriber(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(); // User not found
            }

            // Check if the user is a subscriber
            if (await _userManager.IsInRoleAsync(user, UserRoles.SUBSCRIBER))
            {
                // Proceed with deletion
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    var subscriber = await _context.Subscribers.FindAsync(id);
                    if (subscriber != null)
                    {
                        _context.Subscribers.Remove(subscriber);
                        await _context.SaveChangesAsync();

                        // Remove from cache
                        _redisService.SetValue<IdentityUser>(user.Id, null);
                    }
                    return NoContent(); // Successfully deleted
                }
                else
                {
                    // Failed to delete user, return error messages
                    return BadRequest(result.Errors);
                }
            }
            else
            {
                return Forbid(); // User is not a subscriber, forbid deletion
            }
        }

        private string GenerateETag(IdentityUser user)
        {
            // Generate ETag based on user properties
            return user.Id.GetHashCode().ToString();
        }
    }
}
