using System.Text.Json;

namespace LumiaFoundation.AspNetCore.ExceptionHandlers.ErrorModel
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public required string ExceptionType { get; set; }
        public override string ToString() => JsonSerializer.Serialize(this);
    }
}