using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SeniorEventBooking.Services;

/// <summary>
/// Service for integrating with Memberbase CRM API
/// </summary>
public class MemberbaseService : IMemberbaseService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MemberbaseService> _logger;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public MemberbaseService(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<MemberbaseService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Memberbase:ApiKey"] 
            ?? throw new ArgumentNullException("Memberbase:ApiKey", "Memberbase API key not configured");
        _baseUrl = configuration["Memberbase:BaseUrl"] 
            ?? "https://demo-log.memberbase-sandbox.com";

        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<(bool Success, string? ContactId, string Message)> CreateContactAsync(string name, string email)
    {
        try
        {
            _logger.LogInformation("Attempting to create contact in Memberbase: {Email}", email);

            // Prepare the contact data according to Memberbase API specification
            var contactData = new
            {
                name = name,
                email = email,
                source = "Event Booking System"
            };

            var jsonContent = JsonSerializer.Serialize(contactData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Send POST request to create contact
            var response = await _httpClient.PostAsync("/api/contacts", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                
                try
                {
                    using var jsonDoc = JsonDocument.Parse(responseContent);
                    var contactId = jsonDoc.RootElement.GetProperty("data").GetProperty("id").GetString();
                    
                    _logger.LogInformation("Successfully created contact in Memberbase. Contact ID: {ContactId}", contactId);
                    
                    return (true, contactId, "Contact created successfully in Memberbase CRM");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse contact ID from Memberbase response: {Response}", responseContent);
                    return (true, null, "Contact created but ID could not be retrieved");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Memberbase API returned error: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);
                
                return (false, null, $"API Error: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while calling Memberbase API");
            return (false, null, "Network error connecting to Memberbase CRM");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating contact in Memberbase");
            return (false, null, "Unexpected error occurred while creating contact");
        }
    }
}
