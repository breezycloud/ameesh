using Blazored.LocalStorage;
using Client;
using Client.Handlers;
using Shared.Logging;
using Client.Services.AppService;
using Client.Services.Auth;
using Client.Services.Company;
using Client.Services.Customers;
using Client.Services.Dashboard;
using Client.Services.Expenses;
using Client.Services.Orders;
using Client.Services.Products;
using Client.Services.Returns;
using Client.Services.Users;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using QuestPDF.Infrastructure;
using Shared.Models;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Client.Services.Welfare;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddPWAUpdater();

builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<AutoLogoutService>();
builder.Services.AddScoped<LayoutService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddHttpClient("AppUrl", http =>
{
    http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
}).AddHttpMessageHandler<CustomAuthorizationHandler>();
builder.Services.AddHttpClient<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(options => options.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddTransient<CustomAuthorizationHandler>();


builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IDashboardService, DashboardService>();
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<IStoreService, StoreService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IReturnService, ReturnService>();
builder.Services.AddTransient<IExpenseService, ExpenseService>();
builder.Services.AddTransient<ISyncService, SyncService>();
builder.Services.AddTransient<ISalaryService, SalaryService>();
builder.Services.AddTransient<ISalaryBonusService, SalaryBonusService>();
builder.Services.AddTransient<ISalaryAdvanceService, SalaryAdvanceService>();
builder.Services.AddTransient<IPenaltyService, PenaltyService>();


// logging
builder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Error));
builder.Services.AddSingleton<LogQueue>();
builder.Services.AddSingleton<LogReader>();
builder.Services.AddSingleton<LogWriter>();
builder.Services.AddSingleton<ILoggerProvider, ApplicationLoggerProvider>();
builder.Services.AddHttpClient("LoggerJob", c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddSingleton<LoggerJob>();


QuestPDF.Settings.License = LicenseType.Community;
//QuestPDF.Settings.EnableDebugging = true;

//QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = true;

await builder.Build().RunAsync();
