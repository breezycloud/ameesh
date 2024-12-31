namespace Client.Services.AppService;

// public interface IAutoLogoutService
// {
//     void StartLogoutTimer();
//     void StopLogoutTimer();
// }

public class AutoLogoutService
{
    private  CancellationTokenSource? cancellationTokenSource;
    public event Action? OnLogout;
    // public event EventHandler? OnLogout;
    // public void LogOut() => OnLogout?.Invoke(this, EventArgs.Empty);
    public int DefaultTimer = 10;
    private DateTime startTime = DateTime.UtcNow.AddMinutes(5);
    System.Timers.Timer? myTimer;
    public void StartLogoutTimer()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();

        myTimer = new System.Timers.Timer(TimeSpan.FromMinutes(5));
        myTimer.Elapsed += OnTimedEvent!;
        myTimer.Start();
        
    }

    private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
    {
        TimeSpan remainingTime = TimeSpan.FromTicks(startTime.Ticks) - e.SignalTime.TimeOfDay;       
        Console.WriteLine(remainingTime);
        if (remainingTime <= TimeSpan.Zero)
        {            
            myTimer!.Stop();    
            OnLogout?.Invoke();        
        }        
    }
    public void StopLogoutTimer()
    {        
    }
}