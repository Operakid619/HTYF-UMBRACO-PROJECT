using System.ComponentModel.DataAnnotations;

namespace SeniorEventBooking.Models;

/// <summary>
/// View model for the event booking form
/// </summary>
public class BookingFormViewModel
{
    [Required(ErrorMessage = "Event ID is required")]
    public Guid EventKey { get; set; }

    [Required(ErrorMessage = "Please enter your full name")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Note cannot exceed 1000 characters")]
    [Display(Name = "Additional Notes (Optional)")]
    public string? Note { get; set; }
}
