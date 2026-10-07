using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Modules.Ledger;

public static class LedgerModuleExtensions
{
    public static IServiceCollection AddLedgerModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Ledger domain services, handlers, repositories, and DbContext
        return services;
    }

    public static IEndpointRouteBuilder MapLedgerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ledger")
            .WithTags("Ledger Module");

        group.MapGet("/health", () => Results.Ok(new { Module = "Ledger", Status = "Ready" }))
            .WithName("GetLedgerStatus")
            .Produces(StatusCodes.Status200OK);

        return app;
    }
}
