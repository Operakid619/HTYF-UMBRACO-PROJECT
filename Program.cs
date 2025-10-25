using Microsoft.EntityFrameworkCore;
using SeniorEventBooking.Data;
using SeniorEventBooking.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register EventBooking DbContext
builder.Services.AddDbContext<EventBookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EventBookingDb")));

// Register HttpClient for Memberbase API
builder.Services.AddHttpClient<IMemberbaseService, MemberbaseService>();

// Register Memberbase service
builder.Services.AddScoped<IMemberbaseService, MemberbaseService>();

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();


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
