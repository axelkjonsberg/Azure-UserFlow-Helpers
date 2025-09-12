# UserFlowFunctions

Helpers for **Azure AD B2C API Connectors** and **Microsoft Entra External ID custom authentication extensions** 
(Start/Submit).

Made for Azure Functions .NET **isolated** model on .NET 8.

## What is inside this repo?

- `B2CApiConnector` – return **Continue**, **ShowBlockPage**, or **ValidationError** responses in the exact JSON shape 
  B2C expects.
- `EntraUserFlowExtension` – return External ID **Start**/**Submit** actions:
  - `continueWithDefaultBehavior`
  - `setPrefillValues`
  - `modifyAttributeValues`
  - `showValidationError`
  - `showBlockPage`
  - Every action above wrapped in the `data` 
    envelope with the correct `@odata.type` values.

### Use these helpers

Copy the wanted `.cs` helper files into your Functions project (isolated worker).

### Quick examples

**B2C (API Connector)**
```csharp
[Function("B2C-Validate")]
public Task<HttpResponseData> Validate(
    [HttpTrigger(AuthorizationLevel.Anonymous, "POST")] HttpRequestData req)
{
    // return a field error
    return B2CApiConnector.ValidationErrorAsync(req, "Please enter a valid code.");
}
```

**Entra External ID (User Flow Extension)**
```csharp
[Function("ExtId-Submit")]
public Task<HttpResponseData> Submit(
    [HttpTrigger(AuthorizationLevel.Anonymous, "POST")] HttpRequestData req)
{
    // Example 1: Normalize/override submitted attributes and continue
    var attrs = new Dictionary<string, object> { ["email"] = "user@example.com".ToLowerInvariant() };
    return EntraUserFlowExtension.Submit_ModifyAttributesAsync(req, attrs);

    // Example 2: Show validation errors and keep the page displayed
    return EntraUserFlowExtension.Submit_ShowValidationErrorAsync(
         req,
         "Please fix the highlighted fields.",
         new Dictionary<string, string> { ["email"] = "Email must be a valid address." }
    );
}
```
