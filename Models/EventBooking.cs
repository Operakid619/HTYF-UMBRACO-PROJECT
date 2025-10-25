using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorEventBooking.Models;

/// <summary>
/// Represents a booking for an event made by a member
/// </summary>
[Table("EventBookings")]
public class EventBooking
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The unique identifier of the Event content node in Umbraco
    /// </summary>
    [Required]
    public Guid EventKey { get; set; }

    /// <summary>
    /// Full name of the person making the booking
    /// </summary>
    [Required]
    [StringLength(200)]
    public string BookerName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the booker
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string BookerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Optional notes or comments from the booker
    /// </summary>
    [StringLength(1000)]
    public string? Note { get; set; }

    /// <summary>
    /// When the booking was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The Contact ID returned from Memberbase API (nullable in case API call fails)
    /// </summary>
    [StringLength(100)]
    public string? ApiContactId { get; set; }

    /// <summary>
    /// Response message from the API call (for debugging and logging)
    /// </summary>
    [StringLength(500)]
    public string? ApiResponse { get; set; }

    /// <summary>
    /// Indicates whether the API call was successful
    /// </summary>
    public bool ApiSuccess { get; set; } = false;
}
