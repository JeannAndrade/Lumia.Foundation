// Presentation/Extensions/OpenApiServiceExtensions.cs (ou onde ficam as demais Configure*)
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace LumiaFoundation.AspNetCore.Commons.Extensions;

public static class OpenApiServiceExtensions
{
    public static void ConfigureOpenApi(this IServiceCollection services, string title, string version) =>
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer(new ApiInfoDocumentTransformer(title, version));
        });
}

internal sealed class ApiInfoDocumentTransformer(string title, string version) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new() { Title = title, Version = version };
        return Task.CompletedTask;
    }
}