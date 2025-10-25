namespace SeniorEventBooking.Services;

/// <summary>
/// Interface for Memberbase CRM API integration
/// </summary>
public interface IMemberbaseService
{
    /// <summary>
    /// Creates a new contact in Memberbase CRM
    /// </summary>
    /// <param name="name">Full name of the contact</param>
    /// <param name="email">Email address of the contact</param>
    /// <returns>Tuple containing success status, contact ID (if successful), and message</returns>
    Task<(bool Success, string? ContactId, string Message)> CreateContactAsync(string name, string email);
}
