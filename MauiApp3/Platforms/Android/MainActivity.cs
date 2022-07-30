using Android.App;
using Android.Content.PM;
using Android.OS;
using MauiApp3.Notifications;

namespace MauiApp3;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        new AndroidNotificationManager().SendNotification("Test notification", "This is a test message, tap me!");
    }
}