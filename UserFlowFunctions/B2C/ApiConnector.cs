// Ref: https://learn.microsoft.com/azure/active-directory-b2c/add-api-connector

using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace UserFlowFunctions.B2C;

/// <summary>
/// Helpers for Azure AD B2C API Connector responses.
/// Produces the exact wire format Azure B2C expects: Continue / ShowBlockPage (HTTP 200)
/// and ValidationError (HTTP 400 + body.status=400).
/// </summary>
/// <remarks>
/// See Microsoft docs for the required shapes and status codes. https://learn.microsoft.com/azure/active-directory-b2c/add-api-connector
/// </remarks>
public static class ApiConnector
{
    private const string _json = "application/json";
    /// <summary>The API connector contract version string required by B2C.</summary>
    public const string ApiVersion = "1.0.0";

    /// <summary>
    /// Returns a <c>200 OK</c> Continue response. Optional claims can prefill or override values.
    /// </summary>
    /// <param name="request">The incoming request (used to create the response).</param>
    /// <param name="claims">
    /// Optional key/value claims to include in the body (in addition to <c>version</c> and <c>action</c>, which are always applied).
    /// </param>
    /// <returns>HTTP 200 response with <c>{ "version": "1.0.0", "action": "Continue", ...claims }</c>.</returns>
    public static async Task<HttpResponseData> ContinueAsync(
        HttpRequestData request, IDictionary<string, object>? claims = null)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", _json);

        var body = new Dictionary<string, object>
        {
            ["version"] = ApiVersion,
            ["action"]  = "Continue"
        };

        if (claims is { Count: > 0 })
        {
            foreach (var keyValuePair in claims)
            {
                body[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        await response.WriteStringAsync(JsonSerializer.Serialize(body));
        return response;
    }

    /// <summary>
    /// Returns a <c>200 OK</c> block-page response shown to the end user with the supplied <paramref name="userMessage"/>.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="userMessage">Human-readable message to show on the block page.</param>
    /// <returns>HTTP 200 response with <c>{ "version": "1.0.0", "action": "ShowBlockPage", "userMessage": "…" }</c>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="userMessage"/> is empty.</exception>
    public static async Task<HttpResponseData> ShowBlockPageAsync(
        HttpRequestData request, string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            throw new ArgumentException("cannot be empty.", nameof(userMessage));

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", _json);

        var body = new
        {
            version = ApiVersion,
            action  = "ShowBlockPage",
            userMessage
        };

        await response.WriteStringAsync(JsonSerializer.Serialize(body));
        return response;
    }

    /// <summary>
    /// Returns a <c>400 Bad Request</c> validation error that keeps the attribute page displayed.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="userMessage">Human-readable validation message.</param>
    /// <returns>
    /// HTTP 400 response with <c>{ "version": "1.0.0", "status": 400, "action": "ValidationError", "userMessage": "…" }</c>.
    /// </returns>
    /// <remarks>
    /// For validation errors, Microsoft Entra External ID requires both HTTP status code <c>400</c>
    /// and <c>"status": 400</c> in the JSON body.
    /// </remarks>
    public static async Task<HttpResponseData> ValidationErrorAsync(
        HttpRequestData request, string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            throw new ArgumentException("cannot be empty.", nameof(userMessage));

        var response = request.CreateResponse(HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", _json);

        var body = new
        {
            version = ApiVersion,
            status  = 400,
            action  = "ValidationError",
            userMessage
        };

        await response.WriteStringAsync(JsonSerializer.Serialize(body));
        return response;
    }
}
