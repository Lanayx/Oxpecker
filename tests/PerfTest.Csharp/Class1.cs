using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace PerfTest.Csharp;

public class BindingModelChild
{
    public string? Name { get; set; }
    public int Age { get; set; }
}

public class BindingModel
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public DateTime BirthDate { get; set; }
    public int StatusCode { get; set; }
    public BindingModelChild[] Children { get; set; } = Array.Empty<BindingModelChild>();
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
