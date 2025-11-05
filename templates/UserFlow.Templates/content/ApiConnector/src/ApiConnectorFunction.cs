using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using UserFlow.Helpers.B2C;

namespace ProjectName;

public sealed class ApiConnectorFunction
{
    private const string FunctionName = "Signup_BeforeCreate";
    private const string Route = "b2c/signup-before-create";

    [Function(FunctionName)]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)]
        HttpRequestData req)
    {
        using var jsonDocument = await JsonDocument.ParseAsync(req.Body);
        var jsonRootElement = jsonDocument.RootElement;

        // (EDIT THIS REGION) vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv (EDIT THIS REGION)
        // Replace the code below with whatever you want to validate/do in your user flow API Connector
        //
        // - Read whatever claims your user flow sends.
        // - Do some logic and decide between Continue, ShowBlockPage, or ValidationError response.
        // - Optionally return claims on Continue to prefill/override values.
        //
        // Example below: validate the email claim value
        var email = TryGetString(jsonRootElement, "email");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return await ApiConnector.ValidationErrorAsync(req, "Email address is missing or malformed.");
        }

        var claims = new Dictionary<string, object>
        {
            ["email"] = email.ToLowerInvariant()
        };
        // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        return await ApiConnector.ContinueAsync(req, claims);
    }

    private static string? TryGetString(JsonElement jsonElement, string name)
    {
        return (from jsonProperty in jsonElement.EnumerateObject()
            where string.Equals(jsonProperty.Name, name, StringComparison.OrdinalIgnoreCase)
            select jsonProperty.Value.ValueKind == JsonValueKind.String
                ? jsonProperty.Value.GetString()
                : jsonProperty.Value.GetRawText()).FirstOrDefault();
    }
}
