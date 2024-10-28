using Shared.Models.Notifications;
using Client.Services.AppService;
using Microsoft.AspNetCore.Components;
using Client.Pages.Users;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.Models.Orders;

namespace Client.Layout.AppUI;

public partial class AppBarButtons
{
    [Inject] private INotificationService NotificationService { get; set; } = default!;
    [Inject] private LayoutService LayoutService { get; set; } = default!;
    [Inject] private AutoLogoutService AutoLogout { get; set; } = default!;
    private NotificationMessage[] _messages = null!;
    private bool _newNotificationsAvailable = false;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await js.InvokeVoidAsync("initAutoLogoutListener", DotNetObjectReference.Create(this));
        }
    }    

    private async Task GetNotifications()
    {
        _newNotificationsAvailable = await NotificationService.AreNewNotificationsAvailable();
        _messages = await NotificationService.GetUnReadNotifications();
    }

    private async Task MarkNotificationAsRead()
    {
        await NotificationService.MarkNotificationsAsRead();
        _newNotificationsAvailable = false;
    }

    private async Task ChangePasswordDialog()
    {
        await Dialog.ShowAsync<ChangePassword>("");
    }

    
    [JSInvokable]
    public async Task LogOut()
    {
        await localStorage.RemoveItemAsync("token");
        await localStorage.RemoveItemAsync("uid");
        await localStorage.RemoveItemAsync("branch");
        await localStorage.RemoveItemAsync("access");
        var builder = new HubConnectionBuilder().WithUrl(nav.ToAbsoluteUri("/hubs")).WithAutomaticReconnect().WithStatefulReconnect();
        builder.Services.Configure<HubConnectionOptions>(o => o.StatefulReconnectBufferSize = 1000);
        var hub = builder.Build();
        var internetfound = await js.InvokeAsync<bool>("checkinternet");
        if (internetfound)
        {
            var items = await localStorage.GetItemAsync<List<SuspendBills>?>("cart");            
            if (items is not null)
            {
                await hub.StartAsync();
                await hub!.SendAsync("ReturnSuspendedBills", items);
                await localStorage.RemoveItemAsync("cart");                
            }            
        }            
        nav.NavigateTo("/", true);
    }

    public void Dispose()
    {        
    }
}