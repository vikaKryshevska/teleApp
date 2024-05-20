using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Entities;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiVersion("2.0")]
    [Authorize(Roles = "SUBSCRIBER")]
    [Route("api/v{version:apiVersion}/Subscribers")]
    [ApiController]
    public class SubscribersController : ControllerBase
    {
        private readonly TelephoneDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SubscribersController(TelephoneDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/subscriber/{subscriberId}/bills
        [HttpGet("unpaid-bills")]
        public async Task<IActionResult> GetBills()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(); // User not authenticated or ID not found in claims
                }

                var records = await _context.Bills
                    .Where(r => r.SubscriberId == userId && r.Status == false)
                    .ToListAsync();
                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        /// <summary>
        /// Use the service
        /// </summary>
        /// <param name="serviceId"></param>
        /// <response code="400">Returns error while creating</response>
        ///  <response code="204">Returns service succesfuly added</response>


        [HttpPost("add/{serviceId}")]

        public async Task<IActionResult> AddServiceToSubscriber(string serviceId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(); // User not found or not authenticated
            }

            // Check if service with the given ID exists
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
            {
                return NotFound("Service not found");
            }

            var subscriber = await _context.Subscribers.FindAsync(userId);


            if (subscriber == null)
            {
                return NotFound();
            }

            // Check if the Services collection is null, if so, initialize it
            if (subscriber.Services == null)
            {
                subscriber.Services = new List<Service>();
            }
            // Add the new service to the list of services associated with the subscriber
            subscriber.Services.Add(service);

            // Calculate the price of the service
            double servicePrice = service.Prise; // Assuming the price is stored in the service object

            // Create a new bill object
            var bill = new Bill
            {
                Id = IdGenerator.CreateLetterName(7),
                Prise = servicePrice,
                Status = false, // Not paid
                DueDate = DateTime.Now.AddDays(30),
                SubscriberId = subscriber.id
            };

            // Add the new bill to the Bills table
            _context.Bills.Add(bill);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                return BadRequest(e.Message);
            }

            return NoContent();
        }



        [HttpPut("{id}/pay-bill")]
        public async Task<IActionResult> PayBill(string id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }

            bill.Status = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                return BadRequest(e.Message);
            }

            return NoContent();
        }

    }
}
