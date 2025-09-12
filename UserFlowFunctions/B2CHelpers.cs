using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace UserFlowFunctions;

public static partial class B2CHelpers
{
    /// <summary>
    /// Reads and parses the request body once and returns both the raw JSON text
    /// and a cloned <see cref="JsonElement"/> you can safely enumerate later.
    /// <para>
    /// Use this in Azure Functions (isolated worker) handlers when you need to
    /// examine posted JSON from B2C/External ID. Remember the request body stream
    /// is forward-only; this helper buffers it for you.
    /// </para>
    /// </summary>
    public static async Task<(JsonElement? Root, string Raw)> TryParseBodyAsync(this HttpRequestData req)
    {
        using var sr = new StreamReader(req.Body);
        var raw = await sr.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(raw)) return (null, raw);
        using var doc = JsonDocument.Parse(raw);
        return (doc.RootElement.Clone(), raw);
    }

    /// <summary>
    /// Convenience accessor for a case-insensitive string property on a
    /// <see cref="JsonElement"/> (e.g., reading <c>email</c> or custom fields).
    /// Returns <c>null</c> if not present. If the value isn’t a JSON string,
    /// this returns the raw JSON text (useful for simple numbers/booleans).
    /// </summary>
    public static string? TryGetString(this JsonElement root, string name)
    {
        return (from jsonProperty in root.EnumerateObject()
            where string.Equals(jsonProperty.Name, name, StringComparison.OrdinalIgnoreCase)
            select jsonProperty.Value.ValueKind == JsonValueKind.String
                ? jsonProperty.Value.GetString()
                : jsonProperty.Value.GetRawText()).FirstOrDefault();
    }

    /// <summary>
    /// Builds a B2C directory extension claim key using the already-normalized
    /// extensions app ID (no dashes), for example:
    /// <c>extension_00001111aaaa2222bbbb3333cccc4444_loyaltyId</c>.
    /// <para>
    /// Use this when you <em>already</em> have the <c>b2c-extensions-app</c> GUID in
    /// "N" format. For the "format everything for me" version, see
    /// <see cref="BuildExtensionClaimKey(string, string)"/>.
    /// </para>
    /// </summary>
    public static string ExtensionKey(string extensionsAppIdNoDashes, string attributeName)
        => $"extension_{extensionsAppIdNoDashes}_{attributeName}";

    /// <summary>
    /// Validates the <c>b2c-extensions-app</c> Application (client) ID, removes dashes,
    /// verifies the attribute name, and returns the full extension claim key:
    /// <c>extension_{appIdWithoutHyphens}_{attributeName}</c>.
    /// <para>
    /// This is the exact naming Azure AD B2C / Microsoft Graph expects for
    /// extension properties.
    /// </para>
    /// <para>
    /// How to find the extensions app ID: Azure portal → Azure AD B2C → App registrations →
    /// <c>b2c-extensions-app. Do not modify…</c> → Application (client) ID.
    /// </para>
    /// </summary>
    /// <param name="b2CExtensionsAppClientId">The <c>b2c-extensions-app</c> Application (client) ID (a GUID with dashes).</param>
    /// <param name="attributeName">Your custom attribute name as created in B2C (e.g., <c>loyaltyId</c>).</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the app ID isn’t a valid GUID, or the attribute name is empty/invalid.
    /// </exception>
    public static string BuildExtensionClaimKey(string b2CExtensionsAppClientId, string attributeName)
    {
        if (!Guid.TryParse(b2CExtensionsAppClientId, out var guid))
            throw new ArgumentException("Expected a GUID (Application/Client ID of b2c-extensions-app).", nameof(b2CExtensionsAppClientId));

        if (string.IsNullOrWhiteSpace(attributeName))
            throw new ArgumentException("Attribute name cannot be empty.", nameof(attributeName));

        // Attribute name guidance: B2C shows/uses the name you created (e.g., loyaltyId).
        // For safety, we enforce a conservative pattern: start with a letter, then letters/digits/underscore.
        // Relax this if your tenant uses other valid characters.
        if (!AttributeNameRegex().IsMatch(attributeName))
            throw new ArgumentException(
                "Attribute name should start with a letter and contain letters, digits, or underscore only.",
                nameof(attributeName));

        var appIdNoDashes = guid.ToString("N"); // 32 hex digits, no hyphens (Graph examples use this form).
        return ExtensionKey(appIdNoDashes, attributeName);
    }

    /// <summary>
    /// Best-effort, non-throwing variant of <see cref="BuildExtensionClaimKey(string, string)"/>.
    /// </summary>
    public static bool TryBuildExtensionClaimKey(string b2CExtensionsAppClientId, string attributeName, out string? key)
    {
        key = null;
        if (!Guid.TryParse(b2CExtensionsAppClientId, out var guid)) return false;
        if (string.IsNullOrWhiteSpace(attributeName)) return false;
        if (!AttributeNameRegex().IsMatch(attributeName)) return false;

        key = ExtensionKey(guid.ToString("N"), attributeName);
        return true;
    }

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]*$")]
    private static partial Regex AttributeNameRegex();
}
