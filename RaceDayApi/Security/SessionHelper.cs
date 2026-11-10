namespace RaceDayApi.Security;

public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string Role = "Role";
}

public static class SessionHelper
{
    public static int? GetUserId(this ISession session) => session.GetInt32(SessionKeys.UserId);

    public static string? GetRole(this ISession session) => session.GetString(SessionKeys.Role);
}