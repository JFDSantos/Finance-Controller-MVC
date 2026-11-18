namespace Finance.API.Health
{
    public static class HealthEndpoint
    {
        /// <summary>GET /health: liveness only. Deliberately does not touch the database.</summary>
        public static IEndpointRouteBuilder MapHealth(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/health", () => Results.Json(new { status = "ok" })).AllowAnonymous();
            return endpoints;
        }
    }
}
