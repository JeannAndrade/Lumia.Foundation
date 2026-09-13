using System.Net.Http.Json;
using System.Text.Json;
using LumiaFoundation.Abstractions.ErrorModel;

namespace LumiaFoundation.Http.Client;

internal static class ApiError
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<string> ReadMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorDetails>(JsonOptions, cancellationToken);
            if (!string.IsNullOrWhiteSpace(error?.Message))
                return error.Message;
        }
        catch (JsonException)
        {
            // Respostas de proxy e challenges JWT não precisam obedecer ao ErrorDetails.
        }

        return $"A API retornou {(int)response.StatusCode} sem corpo de erro reconhecível.";
    }
}
