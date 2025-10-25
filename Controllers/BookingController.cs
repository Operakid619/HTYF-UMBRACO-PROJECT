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
        // Trace entry to diagnose 400s and confirm routing hits this action
        try
        {
            var referer = Request.Headers["Referer"].FirstOrDefault() ?? "(none)";
            var cookieHeaderLen = Request.Headers.ContainsKey("Cookie") ? Request.Headers["Cookie"].ToString().Length : 0;
            _logger.LogInformation(
                "SubmitBooking invoked. Path={Path}, Referer={Referer}, Authenticated={Auth}, CookieHeaderLength={CookieLength}",
                Request.Path,
                referer,
                User?.Identity?.IsAuthenticated ?? false,
                cookieHeaderLen
            );
        }
        catch
        {
            // no-op: logging must never break the flow
        }

        // Helper to safely redirect back to the originating page
        IActionResult RedirectBack()
        {
            var referer = Request.Headers["Referer"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(referer) && Url.IsLocalUrl(referer))
            {
                return Redirect(referer);
            }
            return Redirect("/");
        }

        // Check if user is authenticated (member only feature)
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            TempData["BookingError"] = "You must be logged in to book an event.";
            return RedirectBack();
        }

        // Validate model
        if (!ModelState.IsValid)
        {
            // Log validation issues for troubleshooting
            try
            {
                var allErrors = ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .Select(kvp => new
                    {
                        Field = kvp.Key,
                        Errors = kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    });
                foreach (var item in allErrors)
                {
                    _logger.LogWarning("ModelState error on {Field}: {Errors}", item.Field, string.Join(" | ", item.Errors));
                }
            }
            catch { }

            TempData["BookingError"] = "Please correct the errors in the form.";
            return RedirectBack();
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
                return RedirectBack();
            }

            // Create booking entity
            var booking = new EventBooking
            {
                EventKey = model.EventKey,
                BookerName = model.Name,
                BookerEmail = model.Email,
                Note = model.Note,
                CreatedDate = DateTime.UtcNow,
                // Defensive defaults in case DB columns are non-nullable
                ApiContactId = string.Empty,
                ApiResponse = string.Empty,
                ApiSuccess = false
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

            return RedirectBack();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while saving booking");
            TempData["BookingError"] = "There was an error processing your booking. Please try again.";
            return RedirectBack();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while processing booking");
            TempData["BookingError"] = "An unexpected error occurred. Please try again later.";
            return RedirectBack();
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
