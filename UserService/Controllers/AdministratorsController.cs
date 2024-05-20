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


        public AdministratorsController(UserManager<IdentityUser> userManager, TelephoneDbContext context)
        {
            _userManager = userManager;
            _context = context;
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


        /// <summary>
        /// Get a subscriber by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns subscriber's data"</returns>

        /// <response code="404">Returns no subscriber with this id</response>


        [HttpGet("{id}")]
        public async Task<ActionResult<IdentityUser>> GetSubscriber(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(); // User not found
            }
            return user;
        }




        // GET: api/Administrators/5
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





        [HttpGet("{id} /user-unpaid-accounts")]
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



        /// <summary>
        /// Add a subscriber
        /// </summary>
        /// <param name="registerRequestModel"></param>
        /// <returns>Returns message = "Registration successful"</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /create-subscriber
        ///     {
        ///      "name": "string",
        ///        "password": "string"
        ///       }
        ///
        /// </remarks>
        /// <response code="400">Returns error while creating</response>

        // POST: api/Administrators
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

            return Ok(new { message = "Registration successful" });
        }

        // DELETE: api/Administrators/5
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
    }
}
