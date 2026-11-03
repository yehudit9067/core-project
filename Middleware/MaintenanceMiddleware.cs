namespace TravelApi.Middleware;

/// <summary>
/// חוסם את כל הבקשות ומחזיר 503 כאשר "MaintenanceMode" מוגדר ל-true ב-appsettings.
/// </summary>
public class MaintenanceMiddleware
{
    private readonly RequestDelegate _next;

    public MaintenanceMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context, IConfiguration config)
    {
        if (config.GetValue<bool>("MaintenanceMode"))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json; charset=utf-8";

            // Hebrew text is written as-is; the UTF-8 charset above keeps it correct.
            await context.Response.WriteAsJsonAsync(new
            {
                error = "השירות אינו זמין כעת. נא לנסות שוב מאוחר יותר."
            });
            return;
        }

        await _next(context);
    }
}

public static class MaintenanceMiddlewareExtensions
{
    public static IApplicationBuilder UseMaintenance(this IApplicationBuilder app) =>
        app.UseMiddleware<MaintenanceMiddleware>();
}
