namespace SmartRestaurant.Desktop.Helpers.Session;

public static class SessionManager
{
    public static Guid CurrentUserId { get; set; }
    public static string? FirstName { get; set; }
    public static string? LastName { get; set; }
    public static string? Role { get; set; }

    public static void Reset()
    {
        CurrentUserId = Guid.Empty;
        FirstName = null;
        LastName = null;
        Role = null;
    }
}