using Client.AppTheme;
using Client.Services.AppService;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Toolbelt.Blazor.PWA.Updater;

namespace Client.Layout.AppUI;

public partial class AppLayout : LayoutComponentBase
{
    [Inject] private LayoutService? LayoutService { get; set; }
    [Inject] private NavigationManager? NavigationManager { get; set; }
    [Inject] public IWebAssemblyHostEnvironment? HostEnv { get; set; }
    private PWAUpdater.States _InitialState = PWAUpdater.States.Hidden;
    private bool _drawerOpen = false;
    private bool _topMenuOpen = false;
    protected override async Task OnInitializedAsync()
    {        
        LayoutService?.SetBaseTheme(Theme.LandingPageTheme());
        AppState.Token = await localStorage.GetItemAsync<string>("token");
        AppState.Role = await localStorage.GetItemAsync<string>("access");
        if (AppState.Role != "Admin" || AppState.Role != "Master" || AppState.Role != "Manager")
        {
            var guid = await localStorage.GetItemAsync<Guid?>("branch");
            if (guid is not null && guid != Guid.Empty)
                AppState.StoreID = guid.Value;
        }
    }

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }

    private void OpenTopMenu()
    {
        _topMenuOpen = true;
    }

    private void OnDrawerOpenChanged(bool value)
    {
        _topMenuOpen = false;
        _drawerOpen = value;
        StateHasChanged();
    }
}
