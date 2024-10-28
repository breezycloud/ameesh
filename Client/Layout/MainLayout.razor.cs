using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client.AppTheme;
using Client.Services.AppService;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Toolbelt.Blazor.PWA.Updater;
using Microsoft.AspNetCore.Components.Authorization;

namespace Client.Layout;

public partial class MainLayout : LayoutComponentBase, IDisposable
{
    [Inject] IJSRuntime jsRuntime { get; set; } = default!;
    [Inject] public LayoutService? LayoutService { get; set; }    
    public MudThemeProvider _mudThemeProvider = default!;

    [Inject] public IWebAssemblyHostEnvironment? HostEnv { get; set; }
    private PWAUpdater.States _InitialState = PWAUpdater.States.Hidden;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected override void OnInitialized()
    {
        LayoutService?.SetBaseTheme(Theme.LandingPageTheme());
        LayoutService!.MajorUpdateOccured += LayoutServiceOnMajorUpdateOccured!;
        //if (HostEnv!.IsDevelopment())
        //{
        //    _InitialState = PWAUpdater.States.Showing;
        //}
    }    

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            await ApplyUserPreferences();
            
            StateHasChanged();
        }
    }

    private async Task ApplyUserPreferences()
    {
        var defaultDarkMode = await _mudThemeProvider!.GetSystemPreference();
        await LayoutService?.ApplyUserPreferences(defaultDarkMode!)!;
    }

    public void Dispose()
    {
        LayoutService!.MajorUpdateOccured -= LayoutServiceOnMajorUpdateOccured!;
    }

    private void LayoutServiceOnMajorUpdateOccured(object sender, EventArgs e) => StateHasChanged();
}

