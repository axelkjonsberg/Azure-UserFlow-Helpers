// Response contract wraps actions in "data" with @odata.type and action-specific payloads.
// Refs:
//  - OnAttributeCollectionStart: https://learn.microsoft.com/entra/identity-platform/custom-extension-onattributecollectionstart-retrieve-return-data
//  - OnAttributeCollectionSubmit: https://learn.microsoft.com/entra/identity-platform/custom-extension-onattributecollectionsubmit-retrieve-return-data

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker.Http;

namespace UserFlowFunctions;

/// <summary>
/// Helpers for Microsoft Entra External ID custom authentication extensions (Start/Submit).
/// Produces the exact response envelope and action objects documented by Microsoft:
/// <c>{ "data": { "@odata.type": "...ResponseData", "actions": [ { "@odata.type": "...action", ... } ] } }</c>.
/// </summary>
/// <remarks>
/// See the Start and Submit response schemas for the allowed action types and payloads.
/// https://learn.microsoft.com/entra/identity-platform/custom-extension-onattributecollectionstart-retrieve-return-data
/// https://learn.microsoft.com/entra/identity-platform/custom-extension-onattributecollectionsubmit-retrieve-return-data
/// </remarks>
public static class EntraUserFlowExtension
{
    private const string Json = "application/json";

    // Start
    /// <summary>Return an action to continue with default behavior (Start event).</summary>
    public static Task<HttpResponseData> Start_ContinueAsync(HttpRequestData req)
        => WriteAsync(req, new StartResponse([
            new TypedAction("microsoft.graph.attributeCollectionStart.continueWithDefaultBehavior")
        ]));

    /// <summary>Prefill input values before the attribute page renders (Start event).</summary>
    public static Task<HttpResponseData> Start_SetPrefillValuesAsync(
        HttpRequestData req, IDictionary<string, object> inputs)
        => WriteAsync(req, new StartResponse([new SetPrefillValuesAction(inputs)]));

    /// <summary>Show a block page at Start with an optional title.</summary>
    public static Task<HttpResponseData> Start_ShowBlockPageAsync(
        HttpRequestData req, string message, string? title = null)
        => WriteAsync(req, new StartResponse([
            new BlockAction("microsoft.graph.attributeCollectionStart.showBlockPage", message, title)
        ]));


    // Submit
    /// <summary>Continue with default behavior after submission (Submit event).</summary>
    public static Task<HttpResponseData> Submit_ContinueAsync(HttpRequestData req)
        => WriteAsync(req, new SubmitResponse([
            new TypedAction("microsoft.graph.attributeCollectionSubmit.continueWithDefaultBehavior")
        ]));

    /// <summary>Modify submitted attributes (e.g., normalize casing) and continue.</summary>
    public static Task<HttpResponseData> Submit_ModifyAttributesAsync(
        HttpRequestData req, IDictionary<string, object> attributes)
        => WriteAsync(req, new SubmitResponse([new ModifyAttributesAction(attributes)]));

    /// <summary>Show field-level validation messages and keep the page displayed.</summary>
    public static Task<HttpResponseData> Submit_ShowValidationErrorAsync(
        HttpRequestData req, string message, IDictionary<string, string>? attributeErrors = null)
        => WriteAsync(req, new SubmitResponse([
            new ValidationErrorAction(message, attributeErrors ?? new Dictionary<string, string>())
        ]));

    /// <summary>Show a block page at Submit with an optional title.</summary>
    public static Task<HttpResponseData> Submit_ShowBlockPageAsync(
        HttpRequestData req, string message, string? title = null)
        => WriteAsync(req, new SubmitResponse([
            new BlockAction("microsoft.graph.attributeCollectionSubmit.showBlockPage", message, title)
        ]));


    //  Writer + Models

    /// <summary>
    /// Serializes a response payload inside <c>{ "data": ... }</c> with the proper content type and HTTP 200.
    /// </summary>
    private static async Task<HttpResponseData> WriteAsync<T>(HttpRequestData req, T payload)
    {
        var res = req.CreateResponse(HttpStatusCode.OK);
        res.Headers.Add("Content-Type", Json);
        var json = JsonSerializer.Serialize(new { data = payload }, JsonOptions);
        await res.WriteStringAsync(json);
        return res;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary><c>data</c> payload for Start events.</summary>
    public record StartResponse(List<object> Actions)
    {
        /// <summary>OData discriminator for Start response data.</summary>
        [JsonPropertyName("@odata.type")]
        public string ODataType { get; } = "microsoft.graph.onAttributeCollectionStartResponseData";

        /// <summary>Ordered list of Start actions.</summary>
        [JsonPropertyName("actions")] public List<object> Actions { get; } = Actions;
    }

    /// <summary><c>data</c> payload for Submit events.</summary>
    public record SubmitResponse(List<object> Actions)
    {
        /// <summary>OData discriminator for Submit response data.</summary>
        [JsonPropertyName("@odata.type")]
        public string ODataType { get; } = "microsoft.graph.onAttributeCollectionSubmitResponseData";

        /// <summary>Ordered list of Submit actions.</summary>
        [JsonPropertyName("actions")] public List<object> Actions { get; } = Actions;
    }

    /// <summary>Base class for all action items (sets <c>@odata.type</c>).</summary>
    public record TypedAction
    {
        /// <summary>OData discriminator for the specific action.</summary>
        [JsonPropertyName("@odata.type")]
        public string Type { get; init; }

        /// <summary>Create a typed action with the given <c>@odata.type</c>.</summary>
        public TypedAction(string type) => Type = type;
    }

    /// <summary>Block page action (Start or Submit).</summary>
    public record BlockAction : TypedAction
    {
        /// <summary>Optional title for the block page.</summary>
        [JsonPropertyName("title")]
        public string? Title { get; init; }

        /// <summary>Message to display on the block page.</summary>
        [JsonPropertyName("message")]
        public string Message { get; init; }

        /// <summary>Create a block action for Start/Submit with <paramref name="type"/>.</summary>
        public BlockAction(string type, string message, string? title = null) : base(type)
        {
            Message = message;
            Title = title;
        }
    }

    /// <summary>Validation error action with message and per-field errors (Submit only).</summary>
    public record ValidationErrorAction : TypedAction
    {
        /// <summary>Top-level message to show above the form.</summary>
        [JsonPropertyName("message")]
        public string Message { get; init; }

        /// <summary>Per-attribute error messages keyed by attribute name.</summary>
        [JsonPropertyName("attributeErrors")]
        public IDictionary<string, string> AttributeErrors { get; init; }

        /// <summary>Create a Submit validation error action.</summary>
        public ValidationErrorAction(string message, IDictionary<string, string> attributeErrors)
            : base("microsoft.graph.attributeCollectionSubmit.showValidationError")
        {
            Message = message;
            AttributeErrors = attributeErrors;
        }
    }

    /// <summary>Set prefill values before the page renders (Start only).</summary>
    public record SetPrefillValuesAction(IDictionary<string, object> Inputs)
        : TypedAction("microsoft.graph.attributeCollectionStart.setPrefillValues")
    {
        /// <summary>Key/value inputs to prefill on the page.</summary>
        [JsonPropertyName("inputs")] public IDictionary<string, object> Inputs { get; } = Inputs;
    }

    /// <summary>Modify submitted attributes before continuing (Submit only).</summary>
    public record ModifyAttributesAction(IDictionary<string, object> Attributes)
        : TypedAction("microsoft.graph.attributeCollectionSubmit.modifyAttributeValues")
    {
        /// <summary>Key/value attributes to override.</summary>
        [JsonPropertyName("attributes")] public IDictionary<string, object> Attributes { get; } = Attributes;
    }

    // helpers

    /// <summary>Reads and parses the request body JSON, returning the root element and raw text.</summary>
    public static async Task<(JsonElement? Root, string Raw)> TryParseBodyAsync(this HttpRequestData req)
    {
        using var sr = new StreamReader(req.Body);
        var raw = await sr.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(raw)) return (null, raw);
        using var doc = JsonDocument.Parse(raw);
        return (doc.RootElement.Clone(), raw);
    }

    /// <summary>Case-insensitive string property getter for a <see cref="JsonElement"/>.</summary>
    public static string? TryGetString(this JsonElement root, string name)
    {
        return (from p in root.EnumerateObject()
                where string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)
                select p.Value.ValueKind == JsonValueKind.String
                    ? p.Value.GetString()
                    : p.Value.GetRawText()).FirstOrDefault();
    }
}
