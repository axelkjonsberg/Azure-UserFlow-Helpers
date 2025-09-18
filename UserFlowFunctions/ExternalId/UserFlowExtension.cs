// Response contract wraps actions in "data" with @odata.type and action-specific payloads.
// Refs:
//  - OnAttributeCollectionStart: https://learn.microsoft.com/entra/identity-platform/custom-extension-onattributecollectionstart-retrieve-return-data
//  - OnAttributeCollectionSubmit: https://learn.microsoft.com/entra/identity-platform/custom-extension-onattributecollectionsubmit-retrieve-return-data

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Microsoft.Azure.Functions.Worker.Http;

namespace UserFlowFunctions.ExternalId;

/// <summary>
/// Helpers for Microsoft Entra External ID custom authentication extensions (Start/Submit).
/// Produces the exact response envelope and action objects as documented by Microsoft.
/// </summary>
/// <remarks>
/// See the Start and Submit response schemas for the allowed action types and payloads.
/// https://learn.microsoft.com/entra/identity-platform/custom-extension-onattributecollectionstart-retrieve-return-data
/// https://learn.microsoft.com/entra/identity-platform/custom-extension-onattributecollectionsubmit-retrieve-return-data
/// </remarks>
[PublicAPI]
public static class UserFlowExtension
{
    private const string _json = "application/json";

    // Start
    /// <summary>
    /// Returns a continuation response for the Start event.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    public static Task<HttpResponseData> StartContinueAsync(HttpRequestData request)
        => WriteAsync(request, new StartResponse([
            new TypedAction(Protocol.OData.Actions.StartContinue)
        ]));

    /// <summary>
    /// Returns a Start response that pre-fills attribute values before the page renders.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    /// <param name="inputs">Key/value inputs to prefill.</param>
    public static Task<HttpResponseData> StartSetPrefillValuesAsync(
        HttpRequestData request, IDictionary<string, object> inputs)
        => WriteAsync(request, new StartResponse([new SetPrefillValuesAction(inputs)]));

    /// <summary>
    /// Returns a Start response that displays a blocking page with an optional title.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    /// <param name="message">Message to display.</param>
    /// <param name="title">Optional title.</param>
    public static Task<HttpResponseData> StartShowBlockPageAsync(
        HttpRequestData request, string message, string? title = null)
        => WriteAsync(request, new StartResponse([
            new BlockAction(Protocol.OData.Actions.StartBlock, message, title)
        ]));


    // Submit
    /// <summary>
    /// Returns a continuation response for the Submit event.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    public static Task<HttpResponseData> SubmitContinueAsync(HttpRequestData request)
        => WriteAsync(request, new SubmitResponse([
            new TypedAction(Protocol.OData.Actions.SubmitContinue)
        ]));

    /// <summary>
    /// Returns a Submit response that modifies submitted attributes and continues.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    /// <param name="attributes">Key/value attributes to override.</param>
    public static Task<HttpResponseData> SubmitModifyAttributesAsync(
        HttpRequestData request, IDictionary<string, object> attributes)
        => WriteAsync(request, new SubmitResponse([new ModifyAttributesAction(attributes)]));

    /// <summary>
    /// Returns a Submit response that shows field-level validation messages and keeps the page displayed.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    /// <param name="message">Top-level validation message.</param>
    /// <param name="attributeErrors">Per-attribute errors.</param>
    public static Task<HttpResponseData> SubmitShowValidationErrorAsync(
        HttpRequestData request, string message, IDictionary<string, string>? attributeErrors = null)
        => WriteAsync(request, new SubmitResponse([
            new ValidationErrorAction(message, attributeErrors ?? new Dictionary<string, string>())
        ]));

    /// <summary>
    /// Returns a Submit response that displays a blocking page with an optional title.
    /// </summary>
    /// <param name="request">Incoming request.</param>
    /// <param name="message">Message to display.</param>
    /// <param name="title">Optional title.</param>
    public static Task<HttpResponseData> SubmitShowBlockPageAsync(
        HttpRequestData request, string message, string? title = null)
        => WriteAsync(request, new SubmitResponse([
            new BlockAction(Protocol.OData.Actions.SubmitBlock, message, title)
        ]));

    /// <summary>
    /// Serializes a response payload inside <c>{ "data": … }</c> with content type <c>application/json</c>
    /// and HTTP 200. Uses camelCase property names and omits nulls.
    /// </summary>
    private static async Task<HttpResponseData> WriteAsync<T>(HttpRequestData request, T payload)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", _json);
        var json = JsonSerializer.Serialize(new { data = payload }, _jsonOptions);
        await response.WriteStringAsync(json);
        return response;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary><c>data</c> payload for Start events.</summary>
    public record StartResponse(List<object> Actions)
    {
        /// <summary>OData discriminator for Start response data.</summary>
        [JsonPropertyName(Protocol.Json.ODataType)]
        public string ODataType { get; } = Protocol.OData.Data.Start;

        /// <summary>Ordered list of Start actions.</summary>
        [JsonPropertyName(Protocol.Json.Actions)] public List<object> Actions { get; } = Actions;
    }

    /// <summary><c>data</c> payload for Submit events.</summary>
    public record SubmitResponse(List<object> Actions)
    {
        /// <summary>OData discriminator for Submit response data.</summary>
        [JsonPropertyName(Protocol.Json.ODataType)]
        public string ODataType { get; } = Protocol.OData.Data.Submit;

        /// <summary>Ordered list of Submit actions.</summary>
        [JsonPropertyName(Protocol.Json.Actions)] public List<object> Actions { get; } = Actions;
    }

    /// <summary>Base class for all action items (sets <c>@odata.type</c>).</summary>
    public record TypedAction
    {
        /// <summary>OData discriminator for the specific action.</summary>
        [JsonPropertyName(Protocol.Json.ODataType)]
        public string Type { get; init; }

        /// <summary>Create a typed action with the given <c>@odata.type</c>.</summary>
        public TypedAction(string type) => this.Type = type;
    }

    /// <summary>Block page action (Start or Submit).</summary>
    public record BlockAction : TypedAction
    {
        /// <summary>Optional title for the block page.</summary>
        [JsonPropertyName(Protocol.Json.Title)]
        public string? Title { get; init; }

        /// <summary>Message to display on the block page.</summary>
        [JsonPropertyName(Protocol.Json.Message)]
        public string Message { get; init; }

        /// <summary>Create a block action for Start/Submit with <paramref name="type"/>.</summary>
        public BlockAction(string type, string message, string? title = null) : base(type)
        {
            this.Message = message;
            this.Title = title;
        }
    }

    /// <summary>Validation error action with message and per-field errors (Submit only).</summary>
    public record ValidationErrorAction : TypedAction
    {
        /// <summary>Top-level message to show above the form.</summary>
        [JsonPropertyName(Protocol.Json.Message)]
        public string Message { get; init; }

        /// <summary>Per-attribute error messages keyed by attribute name.</summary>
        [JsonPropertyName(Protocol.Json.AttributeErrors)]
        public IDictionary<string, string> AttributeErrors { get; init; }

        /// <summary>Create a Submit validation error action.</summary>
        public ValidationErrorAction(string message, IDictionary<string, string> attributeErrors)
            : base(Protocol.OData.Actions.SubmitValidate)
        {
            this.Message = message;
            this.AttributeErrors = attributeErrors;
        }
    }

    /// <summary>Set prefill values before the page renders (Start only).</summary>
    public record SetPrefillValuesAction(IDictionary<string, object> Inputs)
        : TypedAction(Protocol.OData.Actions.StartPrefill)
    {
        /// <summary>Key/value inputs to prefill on the page.</summary>
        [JsonPropertyName(Protocol.Json.Inputs)] public IDictionary<string, object> Inputs { get; } = Inputs;
    }

    /// <summary>Modify submitted attributes before continuing (Submit only).</summary>
    public record ModifyAttributesAction(IDictionary<string, object> Attributes)
        : TypedAction(Protocol.OData.Actions.SubmitModify)
    {
        /// <summary>Key/value attributes to override.</summary>
        [JsonPropertyName(Protocol.Json.Attributes)] public IDictionary<string, object> Attributes { get; } = Attributes;
    }
}
