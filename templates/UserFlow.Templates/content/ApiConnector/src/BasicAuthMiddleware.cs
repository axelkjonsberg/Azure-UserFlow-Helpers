using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http; 
using Microsoft.Azure.Functions.Worker.Middleware;

namespace ProjectName;

public sealed class BasicAuthMiddleware : IFunctionsWorkerMiddleware
{
    private const string BasicAuthSchemeWithSpace = "Basic ";
    private const char UserPasswordSeparator = ':';

    public async Task Invoke(FunctionContext functionContext, FunctionExecutionDelegate next)
    {
        var httpRequest = await functionContext.GetHttpRequestDataAsync();
        if (httpRequest is null)
        {
            await next(functionContext);
            return;
        }

        httpRequest.Headers.TryGetValues("Authorization", out var authorizationHeaderValues);
        var authorizationHeaderValue = authorizationHeaderValues?.FirstOrDefault();

        if (!IsAuthorized(authorizationHeaderValue))
        {
            var unauthorizedResponse = httpRequest.CreateResponse(HttpStatusCode.Unauthorized);
            unauthorizedResponse.Headers.Add("WWW-Authenticate", """
                                                                 Basic realm="B2C"
                                                                 """);
            await unauthorizedResponse.WriteStringAsync("Unauthorized");
            functionContext.GetInvocationResult().Value = unauthorizedResponse;
            return;
        }

        await next(functionContext);
    }

    private static bool IsAuthorized(string? authorizationHeaderValue)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeaderValue) ||
            !authorizationHeaderValue.StartsWith(BasicAuthSchemeWithSpace, StringComparison.OrdinalIgnoreCase))
            return false;

        var encodedUserAndPassword = authorizationHeaderValue[BasicAuthSchemeWithSpace.Length..].Trim();
        string decodedUserAndPassword;
        try
        {
            decodedUserAndPassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUserAndPassword));
        }
        catch
        {
            return false;
        }

        var separatorIndex = decodedUserAndPassword.IndexOf(UserPasswordSeparator);
        if (separatorIndex < 0) return false;

        var providedUsername = decodedUserAndPassword[..separatorIndex];
        var providedPassword = decodedUserAndPassword[(separatorIndex + 1)..];

        var expectedUsername = Environment.GetEnvironmentVariable("BASIC_AUTH__USERNAME");
        var expectedPassword = Environment.GetEnvironmentVariable("BASIC_AUTH__PASSWORD");

        return !string.IsNullOrEmpty(expectedUsername)
            && !string.IsNullOrEmpty(expectedPassword)
            && string.Equals(providedUsername, expectedUsername, StringComparison.Ordinal)
            && string.Equals(providedPassword, expectedPassword, StringComparison.Ordinal);
    }
}
