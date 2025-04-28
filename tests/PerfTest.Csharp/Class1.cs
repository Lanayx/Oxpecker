using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace PerfTest.Csharp;

public record BindingModelChild
{
    required public string? Name { get; init; }
    required public int Age { get; init; }
}

public record BindingModel
{
    required public Guid Id { get; init; }
    required public string? FirstName { get; init; }
    required public string? MiddleName { get; init; }
    required public string? LastName { get; init; }
    required public DateTime BirthDate { get; init; }
    required public int StatusCode { get; init; }
    required public BindingModelChild[] Children { get; init; } = Array.Empty<BindingModelChild>();
}

public class ModelBindingTest
{
    public static void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/bindModel", (HttpContext ctx, [FromForm]BindingModel x) =>
        {
            ctx.Response.StatusCode = x.StatusCode;
        }).DisableAntiforgery();
    }
}
