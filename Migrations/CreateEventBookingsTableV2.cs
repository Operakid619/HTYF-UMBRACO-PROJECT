using Umbraco.Cms.Infrastructure.Migrations;

namespace SeniorEventBooking.Migrations;

/// <summary>
/// Follow-up migration to ensure the EventBookings table exists with the correct name
/// </summary>
public class CreateEventBookingsTableV2 : MigrationBase
{
    public CreateEventBookingsTableV2(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        // Ensure the intended table exists; create if missing
        if (!TableExists("EventBookings"))
        {
            Create.Table<EventBookings>().Do();
        }
    }

    // IMPORTANT: Name this class to match the intended table name so Create.Table<T>()
    // produces a table named "EventBookings".
    private class EventBookings
    {
        public int Id { get; set; }
        public Guid EventKey { get; set; }
        public string BookerName { get; set; } = string.Empty;
        public string BookerEmail { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ApiContactId { get; set; }
        public string? ApiResponse { get; set; }
        public bool ApiSuccess { get; set; }
    }
}
