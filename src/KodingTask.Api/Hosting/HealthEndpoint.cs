namespace KodingTask.Api.Hosting;

public static class HealthEndpoint
{
    public const string Path = "/health";

    public static IApplicationBuilder UseHealthEndpoint(this WebApplication app)
    {
        var version = app.Configuration.Version();
        var commit = app.Configuration.Commit();

        return app.Use(async (context, next) =>
        {
            if (!context.Request.Path.Equals(Path, StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            context.Response.Headers.CacheControl = "no-store";
            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                context.Response.Headers.Allow = "GET, HEAD";
                return;
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json; charset=utf-8";
            if (HttpMethods.IsHead(context.Request.Method))
                return;

            await context.Response.WriteAsJsonAsync(new
            {
                status = "ok",
                version,
                commit,
                now = DateTimeOffset.UtcNow,
            });
        });
    }
}
