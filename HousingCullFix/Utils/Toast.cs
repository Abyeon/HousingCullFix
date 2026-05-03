using System;
using Dalamud.Interface.ImGuiNotification;

namespace HousingCullFix.Utils;

public static class Toast
{
    public static void Warning(string msg)
    {
        var notification = new Notification
        {
            Content = msg,
            Type = NotificationType.Warning,
            InitialDuration = TimeSpan.FromSeconds(15)
        };
        
        Plugin.NotificationManager.AddNotification(notification);
    }
}
