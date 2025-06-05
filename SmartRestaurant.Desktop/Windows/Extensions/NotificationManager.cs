using System.Windows;
using System.Windows.Media.Animation;
using static SmartRestaurant.Desktop.Windows.Extensions.NotificationWindow;

namespace SmartRestaurant.Desktop.Windows.Extensions;

public static class NotificationManager
{
    private static readonly List<NotificationWindow> ActiveNotifications = new();
    private const int MaxNotifications = 3;
    private const int NotificationHeight = 80;
    private const int Margin = 10;

    private static readonly Dictionary<NotificationPosition, List<NotificationWindow>> PositionedNotifications = new()
    {
        { NotificationPosition.TopRight, new List<NotificationWindow>() },
        { NotificationPosition.TopLeft, new List<NotificationWindow>() },
        { NotificationPosition.BottomRight, new List<NotificationWindow>() },
        { NotificationPosition.BottomLeft, new List<NotificationWindow>() },
        { NotificationPosition.TopCenter, new List<NotificationWindow>() },
        { NotificationPosition.BottomCenter, new List<NotificationWindow>() }
    };

    public static void ShowNotification(
        MessageType type,
        string message,
        NotificationPosition position = NotificationPosition.BottomRight,
        int duration = 3000)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var positionList = PositionedNotifications[position];

            if (positionList.Count >= MaxNotifications)
            {
                var oldest = positionList.Last();
                oldest.Close();
            }

            var notification = new NotificationWindow(type, message, duration);
            PositionNotification(notification, position);
            notification.Show();

            ActiveNotifications.Add(notification);
            positionList.Insert(0, notification);

            notification.Closed += (s, e) =>
            {
                ActiveNotifications.Remove(notification);
                positionList.Remove(notification);
                UpdatePositions(position);
            };

            UpdatePositions(position);
        });
    }

    private static void PositionNotification(NotificationWindow notification, NotificationPosition position)
    {
        double startX;
        double startY;
        var positionList = PositionedNotifications[position];

        switch (position)
        {
            case NotificationPosition.TopLeft:
                startX = 10;
                startY = 10;
                break;

            case NotificationPosition.TopRight:
                startX = SystemParameters.WorkArea.Width - notification.Width - 10;
                startY = 10;
                break;

            case NotificationPosition.BottomLeft:
                startX = 10;
                startY = SystemParameters.WorkArea.Height - NotificationHeight - 10;
                break;

            case NotificationPosition.BottomRight:
                startX = SystemParameters.WorkArea.Width - notification.Width - 10;
                startY = SystemParameters.WorkArea.Height - NotificationHeight - 10;
                break;

            case NotificationPosition.TopCenter:
                startX = (SystemParameters.WorkArea.Width - notification.Width) / 2;
                startY = 10;
                break;

            case NotificationPosition.BottomCenter:
                startX = (SystemParameters.WorkArea.Width - notification.Width) / 2;
                startY = SystemParameters.WorkArea.Height - NotificationHeight - 10;
                break;

            default:
                startX = SystemParameters.WorkArea.Width - notification.Width - 10;
                startY = 10;
                break;
        }

        notification.Left = startX;
        notification.Top = startY;
    }

    private static void UpdatePositions(NotificationPosition position)
    {
        var positionList = PositionedNotifications[position];

        if (positionList.Count == 0) return;

        double startY;

        switch (position)
        {
            case NotificationPosition.TopLeft:
            case NotificationPosition.TopRight:
            case NotificationPosition.TopCenter:
                startY = 10;
                for (int i = 0; i < positionList.Count; i++)
                {
                    MoveNotification(positionList[i], positionList[i].Left, startY);
                    startY += NotificationHeight + Margin;
                }
                break;

            case NotificationPosition.BottomLeft:
            case NotificationPosition.BottomRight:
            case NotificationPosition.BottomCenter:
                startY = SystemParameters.WorkArea.Height - NotificationHeight - 10;
                for (int i = positionList.Count - 1; i >= 0; i--)
                {
                    MoveNotification(positionList[i], positionList[i].Left, startY);
                    startY -= NotificationHeight + Margin;
                }
                break;
        }
    }

    private static void MoveNotification(NotificationWindow notification, double toX, double toY)
    {
        DoubleAnimation moveAnimX = new DoubleAnimation
        {
            To = toX,
            Duration = TimeSpan.FromMilliseconds(300),
            DecelerationRatio = 0.5
        };

        DoubleAnimation moveAnimY = new DoubleAnimation
        {
            To = toY,
            Duration = TimeSpan.FromMilliseconds(300),
            DecelerationRatio = 0.5
        };

        notification.BeginAnimation(Window.LeftProperty, moveAnimX);
        notification.BeginAnimation(Window.TopProperty, moveAnimY);
    }
}
