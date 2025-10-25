using Microsoft.EntityFrameworkCore;
using SeniorEventBooking.Data;
using SeniorEventBooking.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Increase Kestrel limits to avoid 400 Bad Request due to large header sizes (e.g., backoffice cookies)
builder.WebHost.ConfigureKestrel(options =>
{
    // Allow larger total request header size (default ~32KB). Increase to 128KB.
    options.Limits.MaxRequestHeadersTotalSize = 524288; // 512 KB
    // Optionally, allow a few more headers if needed.
    options.Limits.MaxRequestHeaderCount = 200;
});

// Register EventBooking DbContext
builder.Services.AddDbContext<EventBookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EventBookingDb")));

// Register HttpClient for Memberbase API
builder.Services.AddHttpClient<IMemberbaseService, MemberbaseService>();

// MemberbaseService is registered as a typed HttpClient above; no additional scoped registration needed

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

// Ensure EventBookings table exists as a safety net (in case Umbraco migration didn't run)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<EventBookingDbContext>();
        // Ensure EF creates the schema for EventBookingDbContext if missing (dev safety)
        try
        {
            var created = await db.Database.EnsureCreatedAsync();
            if (created)
            {
                app.Logger.LogInformation("EF EnsureCreated created EventBookings schema.");
            }
        }
        catch (Exception ensureEx)
        {
            app.Logger.LogWarning(ensureEx, "EF EnsureCreated failed (continuing to raw SQL guard).");
        }
        var sql = @"
IF OBJECT_ID(N'[dbo].[EventBookings]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[EventBookings](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EventKey] UNIQUEIDENTIFIER NOT NULL,
        [BookerName] NVARCHAR(200) NOT NULL,
        [BookerEmail] NVARCHAR(255) NOT NULL,
        [Note] NVARCHAR(1000) NULL,
        [CreatedDate] DATETIME2 NOT NULL CONSTRAINT DF_EventBookings_CreatedDate DEFAULT (SYSUTCDATETIME()),
        [ApiContactId] NVARCHAR(200) NULL,
        [ApiResponse] NVARCHAR(MAX) NULL,
        [ApiSuccess] BIT NOT NULL CONSTRAINT DF_EventBookings_ApiSuccess DEFAULT (0)
    );
    CREATE INDEX IX_EventBookings_EventKey ON [dbo].[EventBookings]([EventKey]);
    CREATE INDEX IX_EventBookings_BookerEmail ON [dbo].[EventBookings]([BookerEmail]);
    CREATE INDEX IX_EventBookings_CreatedDate ON [dbo].[EventBookings]([CreatedDate]);
END
ELSE
BEGIN
    -- If the Id column is not an IDENTITY, recreate the table with the correct schema (dev safety net)
    IF ISNULL(COLUMNPROPERTY(OBJECT_ID('dbo.EventBookings'), 'Id', 'IsIdentity'), 0) = 0
    BEGIN
        -- Development safety net: drop and recreate with correct identity PK
        IF OBJECT_ID(N'[dbo].[EventBookings]', N'U') IS NOT NULL DROP TABLE [dbo].[EventBookings];

        CREATE TABLE [dbo].[EventBookings](
            [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            [EventKey] UNIQUEIDENTIFIER NOT NULL,
            [BookerName] NVARCHAR(200) NOT NULL,
            [BookerEmail] NVARCHAR(255) NOT NULL,
            [Note] NVARCHAR(1000) NULL,
            [CreatedDate] DATETIME2 NOT NULL CONSTRAINT DF_EventBookings_CreatedDate DEFAULT (SYSUTCDATETIME()),
            [ApiContactId] NVARCHAR(200) NULL,
            [ApiResponse] NVARCHAR(MAX) NULL,
            [ApiSuccess] BIT NOT NULL CONSTRAINT DF_EventBookings_ApiSuccess DEFAULT (0)
        );
        CREATE INDEX IX_EventBookings_EventKey ON [dbo].[EventBookings]([EventKey]);
        CREATE INDEX IX_EventBookings_BookerEmail ON [dbo].[EventBookings]([BookerEmail]);
        CREATE INDEX IX_EventBookings_CreatedDate ON [dbo].[EventBookings]([CreatedDate]);
    END
END
-- Widen columns defensively if table already exists
IF COL_LENGTH('dbo.EventBookings','ApiResponse') IS NOT NULL
BEGIN
    BEGIN TRY
        ALTER TABLE [dbo].[EventBookings] ALTER COLUMN [ApiResponse] NVARCHAR(MAX) NULL;
    END TRY BEGIN CATCH END CATCH
END
IF COL_LENGTH('dbo.EventBookings','ApiContactId') IS NOT NULL
BEGIN
    BEGIN TRY
        ALTER TABLE [dbo].[EventBookings] ALTER COLUMN [ApiContactId] NVARCHAR(200) NULL;
    END TRY BEGIN CATCH END CATCH
END
";
        await db.Database.ExecuteSqlRawAsync(sql);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Startup check failed when ensuring EventBookings table exists.");
    }
}

// Diagnostic middleware to trace booking POST requests and catch 400s
app.Use(async (context, next) =>
{
    var path = context.Request.Path.HasValue ? context.Request.Path.Value! : string.Empty;
    if (string.Equals(path, "/umbraco/surface/booking/submitbooking", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var headersSize = context.Request.Headers.Sum(h => h.Key.Length + h.Value.ToString().Length);
            app.Logger.LogInformation(
                "Booking POST incoming. Method={Method}, HeadersCount={Count}, HeadersSize={Size}",
                context.Request.Method,
                context.Request.Headers.Count,
                headersSize);

            await next();

            app.Logger.LogInformation(
                "Booking POST completed. StatusCode={StatusCode}",
                context.Response.StatusCode);
            return;
        }
        catch (BadHttpRequestException ex)
        {
            app.Logger.LogError(ex, "BadHttpRequestException during booking POST");
            throw;
        }
    }

    await next();
});

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
