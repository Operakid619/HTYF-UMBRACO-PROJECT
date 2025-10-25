using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeniorEventBooking.Data;
using SeniorEventBooking.Models;
using SeniorEventBooking.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace SeniorEventBooking.Controllers;

/// <summary>
/// Surface controller for handling event booking form submissions
/// </summary>
public class BookingController : SurfaceController
{
    private readonly EventBookingDbContext _dbContext;
    private readonly IMemberbaseService _memberbaseService;
    private readonly ILogger<BookingController> _logger;

    public BookingController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        EventBookingDbContext dbContext,
        IMemberbaseService memberbaseService,
        ILogger<BookingController> logger)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _dbContext = dbContext;
        _memberbaseService = memberbaseService;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitBooking(BookingFormViewModel model)
    {
        // Check if user is authenticated (member only feature)
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            TempData["BookingError"] = "You must be logged in to book an event.";
            return RedirectToCurrentUmbracoPage();
        }

        // Validate model
        if (!ModelState.IsValid)
        {
            TempData["BookingError"] = "Please correct the errors in the form.";
            return CurrentUmbracoPage();
        }

        try
        {
            _logger.LogInformation("Processing booking for event {EventKey} by {Email}", 
                model.EventKey, model.Email);

            // Check if booking already exists
            var existingBooking = await _dbContext.EventBookings
                .FirstOrDefaultAsync(b => b.EventKey == model.EventKey && b.BookerEmail == model.Email);

            if (existingBooking != null)
            {
                TempData["BookingError"] = "You have already booked this event.";
                return RedirectToCurrentUmbracoPage();
            }

            // Create booking entity
            var booking = new EventBooking
            {
                EventKey = model.EventKey,
                BookerName = model.Name,
                BookerEmail = model.Email,
                Note = model.Note,
                CreatedDate = DateTime.UtcNow
            };

            // Save booking to database first
            _dbContext.EventBookings.Add(booking);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Booking saved to database with ID: {BookingId}", booking.Id);

            // Attempt to create contact in Memberbase CRM
            var (success, contactId, message) = await _memberbaseService.CreateContactAsync(model.Name, model.Email);
            
            // Update booking with API response
            booking.ApiSuccess = success;
            booking.ApiContactId = contactId;
            booking.ApiResponse = message;
            await _dbContext.SaveChangesAsync();

            if (success)
            {
                _logger.LogInformation("Successfully created contact in Memberbase. Contact ID: {ContactId}", contactId);
                TempData["BookingSuccess"] = "Your booking has been confirmed! A confirmation will be sent to your email.";
            }
            else
            {
                _logger.LogWarning("Booking saved but Memberbase API call failed: {Message}", message);
                TempData["BookingSuccess"] = "Your booking has been confirmed! However, there was an issue syncing with our CRM system.";
            }

            return RedirectToCurrentUmbracoPage();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while saving booking");
            TempData["BookingError"] = "There was an error processing your booking. Please try again.";
            return CurrentUmbracoPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while processing booking");
            TempData["BookingError"] = "An unexpected error occurred. Please try again later.";
            return CurrentUmbracoPage();
        }
    }

    /// <summary>
    /// Gets all bookings for a specific event (used by Content App)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetEventBookings(Guid eventKey)
    {
        try
        {
            var bookings = await _dbContext.EventBookings
                .Where(b => b.EventKey == eventKey)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return Json(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for event {EventKey}", eventKey);
            return StatusCode(500, "Error retrieving bookings");
        }
    }
}
