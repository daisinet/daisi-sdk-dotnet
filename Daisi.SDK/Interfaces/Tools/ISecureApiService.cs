namespace Daisi.SDK.Interfaces.Tools;

/// <summary>
/// Interface for secure API calls that require API keys.
/// API keys are stored on the ORC and never exposed to hosts.
///
/// Pattern:
/// 1. Tool calls toolContext.Services.GetService&lt;ISecureApiService&gt;()
/// 2. Sends SecureApiRequest with service ID + params (never the API key)
/// 3. Host forwards to ORC via gRPC
/// 4. ORC looks up API key from secure config, makes the HTTP call, returns response
/// 5. Host never sees the API key
/// </summary>
public interface ISecureApiService
{
    Task<SecureApiResponse> CallAsync(SecureApiRequest request, CancellationToken ct = default);
}

public class SecureApiRequest
{
    /// <summary>
    /// The service identifier (e.g., "openweathermap", "newsapi").
    /// Maps to a registered API key on the ORC.
    /// </summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// The API endpoint path (e.g., "/v1/weather").
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method: GET or POST.
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// Query parameters to include in the request.
    /// </summary>
    public Dictionary<string, string> QueryParams { get; set; } = new();

    /// <summary>
    /// Optional request body (for POST requests).
    /// </summary>
    public string? Body { get; set; }
}

public class SecureApiResponse
{
    /// <summary>
    /// Whether the API call was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// HTTP status code from the external API.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Response body from the external API.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Error message if the call failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
