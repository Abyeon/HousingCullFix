using System;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;

namespace HousingCullFix;

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
