using Client.Services.AppService;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Layout.AppUI;

public partial class AppBar
{
    [Parameter] public EventCallback<MouseEventArgs> DrawerToggleCallback { get; set; }
    [Parameter] public bool DisplaySearchBar { get; set; }    
    [Inject] private NavigationManager? NavigationManager { get; set; }
    [Inject] private LayoutService? LayoutService { get; set; }
    private string _badgeTextSoon = "coming soon";
    private bool _searchDialogOpen;
    private MudMenu _menuCP;
    private MudMenu _menuCustomers;
    private MudMenu _menuLabs;
    private MudMenu _menuServices;
    private MudMenu _menuSales;
    private MudMenu _menuExpenses;

    private void Nav(string page, string menu)
    {
        if (menu == "items")
            _menuCP.CloseMenu();
        else if (menu == "labs")
            _menuLabs.CloseMenu();
        else if (menu == "customers")
            _menuCustomers.CloseMenu();
        else if (menu == "sales")
            _menuSales.CloseMenu();
        else if (menu == "expense")
            _menuExpenses.CloseMenu();
        else
            _menuServices.CloseMenu();
        NavigationManager!.NavigateTo(page);
    }

    private void CloseAppMenu(string menu)
    {
        if (menu == "employees")
            _menuCP.CloseMenu();
        else if (menu == "services")
            _menuServices.CloseMenu();
        else
            _menuSales.CloseMenu();
    }

    private void OpenSearchDialog() => _searchDialogOpen = true;
    private DialogOptions _dialogOptions = new() { Position = DialogPosition.TopCenter, NoHeader = true };

}
