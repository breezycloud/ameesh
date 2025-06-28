using Microsoft.AspNetCore.ResponseCompression;
using Server.Context;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Npgsql;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Server.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Server.Logging;
using QuestPDF.Infrastructure;
using Pharmacy.Server.BackgroundJob;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;
// QuestPDF.Settings.EnableDebugging = true;

NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
// Add services to the container.
var key = Encoding.ASCII.GetBytes(builder.Configuration["App:Key"]!);
builder.Services.AddResponseCaching();
builder.Services.AddControllers(options =>
{
    options.CacheProfiles.Add("Default60",
        new CacheProfile
        {
            Duration = 60,
        });
}).AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(30));

string ConnectionString = string.Empty;
#if DEBUG
    ConnectionString = builder.Configuration.GetConnectionString("Production")!;
#else
    ConnectionString = builder.Configuration.GetConnectionString("Production");
#endif

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddScoped<AppDbContext>();
builder.Services.AddDbContextFactory<LoggingContext>(options =>
{
    options.UseNpgsql(ConnectionString, o => { o.SetPostgresVersion(16, 3); o.EnableRetryOnFailure(); });
});

builder.Services.AddScoped<OrderService>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(ConnectionString, o => o.SetPostgresVersion(16, 3));
});


builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = true;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
    };    
});

builder.Services.AddSignalR(options => 
{    
    options.StatefulReconnectBufferSize = 1000;
});

builder.Services.AddHostedService<ServerPeriodicJob>();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ILoggerProvider, ApplicationLoggerProvider>();


var app = builder.Build();
//SeedData.EnsureSeeded(app.Services, true);


app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    //app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseResponseCompression();

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.MapHub<SignalRHubs>("/hubs", options =>
{
    options.AllowStatefulReconnects = true;
});
//app.UseMiddleware<AuditMiddleware>();
app.MapFallbackToFile("index.html");

#if DEBUG
    app.Run();
#else
    app.Run("http://localhost:5004");
#endif
