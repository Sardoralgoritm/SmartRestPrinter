using SmartRestaurant.Desktop.Windows.BlurWindow;
using System.Windows;

namespace SmartRestaurant.Desktop.Windows.Extensions;

public static class MessageBoxManager
{
    public static bool Show(string message, MessageBoxWindow.MessageType type, MessageBoxWindow.MessageButtons buttons)
    {
        var messageBox = new MessageBoxWindow(message, type, buttons);
        return messageBox.ShowDialog() ?? false;
    }
    public static bool ShowWithOwner(Window window, string message, MessageBoxWindow.MessageType type, MessageBoxWindow.MessageButtons buttons)
    {
        var messageBox = new MessageBoxWindow(message, type, buttons);
        messageBox.Owner = window;
        messageBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        return messageBox.ShowDialog() ?? false;
    }

    public static bool ShowInfo(string message)
    {
        return Show(message, MessageBoxWindow.MessageType.Info, MessageBoxWindow.MessageButtons.Ok);
    }

    public static bool ShowSuccess(string message)
    {
        return Show(message, MessageBoxWindow.MessageType.Success, MessageBoxWindow.MessageButtons.Ok);
    }

    public static bool ShowWarning(string message)
    {
        return Show(message, MessageBoxWindow.MessageType.Warning, MessageBoxWindow.MessageButtons.Ok);
    }

    public static bool ShowError(string message)
    {
        return Show(message, MessageBoxWindow.MessageType.Error, MessageBoxWindow.MessageButtons.Ok);
    }

    public static bool ShowConfirmation(string message)
    {
        return Show(message, MessageBoxWindow.MessageType.Confirmation, MessageBoxWindow.MessageButtons.YesNo);
    }

    public static bool ShowConfirmationWithOwner(Window window, string message)
    {
        return ShowWithOwner(window, message, MessageBoxWindow.MessageType.Confirmation, MessageBoxWindow.MessageButtons.YesNo);
    }

    public static bool ShowRetry(string message)
    {
        return Show(message, MessageBoxWindow.MessageType.Warning, MessageBoxWindow.MessageButtons.Retry);
    }
}