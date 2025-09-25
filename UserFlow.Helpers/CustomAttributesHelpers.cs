using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace UserFlow.Helpers;

/// <summary>
/// Helpers for directory-extension claim keys:
/// </summary>
[PublicAPI]
public static partial class CustomAttributesHelpers
{
    /// <summary>
    /// Builds a directory-extension claim key using an application (client) ID with no dashes.
    /// </summary>
    /// <param name="extensionsAppIdNoDashes">
    /// Application (client) ID in “N” format (32 hex digits, no hyphens).
    /// </param>
    /// <param name="attributeName">The extension attribute name (for example, <c>loyaltyId</c>).</param>
    /// <returns><c>extension_{extensionsAppIdNoDashes}_{attributeName}</c>.</returns>
    public static string ExtensionKey(string extensionsAppIdNoDashes, string attributeName)
        => $"extension_{extensionsAppIdNoDashes}_{attributeName}";

    /// <summary>
    /// Validates the application (client) ID and attribute name, removes dashes from the ID,
    /// and returns the directory-extension claim key.
    /// </summary>
    /// <param name="extensionsAppClientId">
    /// Application (client) ID (GUID with dashes).
    /// </param>
    /// <param name="attributeName">The extension attribute name.</param>
    /// <returns><c>extension_{appIdWithoutHyphens}_{attributeName}</c>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the client ID isn’t a valid GUID, the attribute name is empty, or the
    /// attribute name fails the allowed pattern.
    /// </exception>
    public static string BuildExtensionClaimKey(string extensionsAppClientId, string attributeName)
    {
        if (!Guid.TryParse(extensionsAppClientId, out var guid))
            throw new ArgumentException("Expected a GUID (Application/Client ID of b2c-extensions-app).", nameof(extensionsAppClientId));

        if (string.IsNullOrWhiteSpace(attributeName))
            throw new ArgumentException("Attribute name cannot be empty.", nameof(attributeName));

        // Start with a letter; allow letters, digits, and underscore thereafter.
        if (!AttributeNameRegex().IsMatch(attributeName))
        {
            throw new ArgumentException(
                "Attribute name should start with a letter and contain letters, digits, or underscore only.",
                nameof(attributeName));
        }

        var appIdNoDashes = guid.ToString("N"); // 32 hex digits, no hyphens.
        return ExtensionKey(appIdNoDashes, attributeName);
    }

    /// <summary>
    /// Non-throwing variant of <see cref="BuildExtensionClaimKey(string,string)"/>.
    /// </summary>
    /// <param name="extensionsAppClientId">Application (client) ID (GUID with dashes).</param>
    /// <param name="attributeName">Extension attribute name.</param>
    /// <param name="claimKey">Outputs the constructed claim key on success; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> on success; otherwise <c>false</c>.</returns>
    public static bool TryBuildExtensionClaimKey(string extensionsAppClientId, string attributeName, out string? claimKey)
    {
        claimKey = null;
        if (!Guid.TryParse(extensionsAppClientId, out var guid)) return false;
        if (string.IsNullOrWhiteSpace(attributeName)) return false;
        if (!AttributeNameRegex().IsMatch(attributeName)) return false;

        claimKey = ExtensionKey(guid.ToString("N"), attributeName);
        return true;
    }

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]*$")]
    private static partial Regex AttributeNameRegex();
}
