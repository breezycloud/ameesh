using Microsoft.AspNetCore.Components;
using MudBlazor;
using static QRCoder.PayloadGenerator;

namespace Client.Services.AppService;
public class LayoutService
{
    private readonly NavigationManager _navigation = default!;
    private readonly IUserPreferencesService _userPreferencesService;
    private UserPreferences _userPreferences;

    public bool IsRTL { get; private set; } = false;
    public bool IsDarkMode { get; private set; } = false;
    public MudTheme CurrentTheme { get; private set; } = default!;


    public LayoutService(IUserPreferencesService userPreferencesService, NavigationManager navigation)
    {
        _userPreferencesService = userPreferencesService;
        _navigation = navigation;
    }

    private string ActiveClass = "mud-chip-text mud-chip-color-primary mx-1 px-3";

    public bool ActiveUri(string url)
    {
        return _navigation.Uri.Contains(url);
    }
    public string GetActiveLinkClass(string url = "", string url2 = "")
    {
        if (_navigation.Uri.Contains(url))
            return ActiveClass;
        else if (string.IsNullOrEmpty(url) && _navigation.Uri.Contains(url2))
            return ActiveClass;
        else
            return "mx-1 px-3";
    }

    public void SetDarkMode(bool value)
    {
        IsDarkMode = value;
    }

    public async Task ApplyUserPreferences(bool isDarkModeDefaultTheme)
    {
        _userPreferences = await _userPreferencesService.LoadUserPreferences();
        if (_userPreferences != null)
        {
            IsDarkMode = _userPreferences.DarkTheme;
            IsRTL = _userPreferences.RightToLeft;
        }
        else
        {
            IsDarkMode = isDarkModeDefaultTheme ? true : false;
            _userPreferences = new UserPreferences { DarkTheme = IsDarkMode };
            await _userPreferencesService.SaveUserPreferences(_userPreferences);
        }
    }

    public event EventHandler MajorUpdateOccured;

    public void OnMajorUpdateOccured() => MajorUpdateOccured?.Invoke(this, EventArgs.Empty);

    public async Task ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        _userPreferences.DarkTheme = IsDarkMode;
        await _userPreferencesService.SaveUserPreferences(_userPreferences);
        OnMajorUpdateOccured();
    }

    public async Task ToggleRightToLeft()
    {
        IsRTL = !IsRTL;
        _userPreferences.RightToLeft = IsRTL;
        await _userPreferencesService.SaveUserPreferences(_userPreferences);
        OnMajorUpdateOccured();

    }

    public void SetBaseTheme(MudTheme theme)
    {
        CurrentTheme = theme;
        OnMajorUpdateOccured();
    }
}