namespace Finance.API.Health
{
    public static class HealthEndpoint
    {
        /// <summary>
        /// GET /health: liveness plus the running build. Deliberately does not touch the database.
        /// version = RENDER_GIT_COMMIT ?? APP_VERSION ?? "unknown", read once at startup, so the deploy
        /// workflow can confirm that the release it just migrated is the one answering.
        /// </summary>
        public static IEndpointRouteBuilder MapHealth(this IEndpointRouteBuilder endpoints)
        {
            var config = endpoints.ServiceProvider.GetRequiredService<IConfiguration>();
            var version = FirstNonBlank(config["RENDER_GIT_COMMIT"], config["APP_VERSION"]) ?? "unknown";
            endpoints.MapGet("/health", () => Results.Json(new { status = "ok", version })).AllowAnonymous();
            return endpoints;
        }

        private static string? FirstNonBlank(params string?[] values) =>
            values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();
    }
}
