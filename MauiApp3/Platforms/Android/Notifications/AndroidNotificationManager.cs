using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Application = Android.App.Application;
using Context = Android.Content.Context;
using Action = AndroidX.Core.App.NotificationCompat.Action;

namespace MauiApp3.Notifications;

  public class AndroidNotificationManager
    {
        const string ChannelId = "default";
        const string ChannelName = "Default";
        const string ChannelDescription = "The default channel for notifications.";

        public const string TitleKey = "title";
        public const string MessageKey = "message";

        bool _channelInitialized = false;
        int _messageId = 0;
        int _pendingIntentId = 0;

        NotificationManager _manager;

        public event EventHandler NotificationReceived;

        public static AndroidNotificationManager Instance { get; private set; }

        public AndroidNotificationManager() => Initialize();

        public void Initialize()
        {
            if (Instance != null) return;
            CreateNotificationChannel();
            Instance = this;
        }

        public int SendNotification(string title, string message, DateTime? notifyTime = null)
        {
            if (!_channelInitialized)
            {
                CreateNotificationChannel();
            }

            if (notifyTime != null)
            {
                var intent = new Intent(Application.Context, typeof(AlarmHandler));
                intent.PutExtra(TitleKey, title);
                intent.PutExtra(MessageKey, message);

                var pendingIntent = PendingIntent.GetBroadcast(Application.Context, _pendingIntentId++, intent, PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable);
                var triggerTime = GetNotifyTime(notifyTime.Value);
                var alarmManager = Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;
                alarmManager.SetAndAllowWhileIdle(AlarmType.RtcWakeup, triggerTime, pendingIntent);

                return _pendingIntentId;
            }
            else
            {
                Show(title, message);
            }
            return -1;
        }
        
        public void ReceiveNotification(string title, string message)
        {
            var args = new NotificationEventArgs()
            {
                Title = title,
                Message = message,
            };
            NotificationReceived?.Invoke(null, args);
        }

        public void Show(string title, string message)
        {
            var intent = new Intent(Application.Context, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);

            var pendingIntent = PendingIntent.GetActivity(Application.Context, _pendingIntentId++, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var builder = new NotificationCompat.Builder(Application.Context, ChannelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.maui_splash_image)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);

            var notification = builder.Build();
            _manager.Notify(_messageId++, notification);
        }

        void CreateNotificationChannel()
        {
            _manager = (NotificationManager)Application.Context.GetSystemService(Application.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(ChannelName);
                var channel = new NotificationChannel(ChannelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = ChannelDescription
                };
                _manager.CreateNotificationChannel(channel);
            }

            _channelInitialized = true;
        }

        long GetNotifyTime(DateTime notifyTime)
        {
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
            var epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
            var utcAlarmTime = utcTime.AddSeconds(-epochDiff).Ticks / 10000;
            return utcAlarmTime; // milliseconds
        }

        public static void CreateNotificationFromIntent(Intent intent)
        {
            if (intent?.Extras != null)
            {
                var title = intent.GetStringExtra(AndroidNotificationManager.TitleKey);
                var message = intent.GetStringExtra(AndroidNotificationManager.MessageKey);
            }
        }
    }

    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver")]
    public class AlarmHandler : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent?.Extras != null)
            {
                var title = intent.GetStringExtra(AndroidNotificationManager.TitleKey);
                var message = intent.GetStringExtra(AndroidNotificationManager.MessageKey);

                var manager = AndroidNotificationManager.Instance ?? new AndroidNotificationManager();
                manager.Show(title, message);
            }
        }
    }
    
public class NotificationEventArgs : EventArgs
{
    public string Title { get; set; }
    public string Message { get; set; }
}