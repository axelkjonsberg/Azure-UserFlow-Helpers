using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

public sealed class BasicAuthMiddleware : IFunctionsWorkerMiddleware
{
    private const string BasicAuthSchemeWithSpace = "Basic ";
    private const char UserPasswordSeparator = ':';

    public async Task Invoke(FunctionContext functionContext, FunctionExecutionDelegate next)
    {
        HttpRequestData? httpRequest = await functionContext.GetHttpRequestDataAsync();
        if (httpRequest is null)
        {
            await next(functionContext);
            return;
        }

        httpRequest.Headers.TryGetValues("Authorization", out var authorizationHeaderValues);
        string? authorizationHeaderValue = authorizationHeaderValues?.FirstOrDefault();

        if (!IsAuthorized(authorizationHeaderValue))
        {
            var unauthorizedResponse = httpRequest.CreateResponse(HttpStatusCode.Unauthorized);
            unauthorizedResponse.Headers.Add("WWW-Authenticate", @"Basic realm=""B2C""");
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

        string encodedUserAndPassword = authorizationHeaderValue.Substring(BasicAuthSchemeWithSpace.Length).Trim();
        string decodedUserAndPassword;
        try
        {
            decodedUserAndPassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUserAndPassword));
        }
        catch
        {
            return false;
        }

        int separatorIndex = decodedUserAndPassword.IndexOf(UserPasswordSeparator);
        if (separatorIndex < 0) return false;

        string providedUsername = decodedUserAndPassword[..separatorIndex];
        string providedPassword = decodedUserAndPassword[(separatorIndex + 1)..];

        string? expectedUsername = Environment.GetEnvironmentVariable("BASIC_AUTH__USERNAME");
        string? expectedPassword = Environment.GetEnvironmentVariable("BASIC_AUTH__PASSWORD");

        return !string.IsNullOrEmpty(expectedUsername)
            && !string.IsNullOrEmpty(expectedPassword)
            && string.Equals(providedUsername, expectedUsername, StringComparison.Ordinal)
            && string.Equals(providedPassword, expectedPassword, StringComparison.Ordinal);
    }
}
