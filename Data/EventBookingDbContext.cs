using Microsoft.EntityFrameworkCore;
using SeniorEventBooking.Models;

namespace SeniorEventBooking.Data;

/// <summary>
/// Database context for Event Booking feature
/// </summary>
public class EventBookingDbContext : DbContext
{
    public EventBookingDbContext(DbContextOptions<EventBookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<EventBooking> EventBookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EventBooking>(entity =>
        {
            entity.ToTable("EventBookings");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EventKey);
            entity.HasIndex(e => e.BookerEmail);
            entity.HasIndex(e => e.CreatedDate);
            
            entity.Property(e => e.BookerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BookerEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.ApiContactId).HasMaxLength(200);
            // ApiResponse left without max length to map to NVARCHAR(MAX)
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
