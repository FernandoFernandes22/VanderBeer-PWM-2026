namespace VanderBeer.App.Services;

public static class ApiSettings
{
    public static string BaseUrl => Environment.GetEnvironmentVariable("VANDERBEER_API_URL") ?? (DeviceInfo.Platform == DevicePlatform.Android
        ? "http://10.0.2.2:5162/"
        : "http://localhost:5162/");
}
