using Umbraco.Cms.Infrastructure.Migrations;

namespace SeniorEventBooking.Migrations;

/// <summary>
/// Migration to create the EventBookings table
/// </summary>
public class CreateEventBookingsTable : MigrationBase
{
    public CreateEventBookingsTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        if (!TableExists("EventBookings"))
        {
            Create.Table<EventBookingSchema>().Do();
        }
    }

    private class EventBookingSchema
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
