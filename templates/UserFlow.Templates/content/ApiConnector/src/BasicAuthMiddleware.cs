using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

public sealed class BasicAuthMiddleware : IFunctionsWorkerMiddleware
{
    private const string BasicAuthSchemeWithSpace = "Basic ";
    private const char HeaderSplitChar = ':';
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        // Only apply to HTTP-trigger invocations
        var request = await context.GetHttpRequestDataAsync();
        if (request is null)
        {
            await next(context);
            return;
        }

        var authHeaderValue = request.Headers.TryGetValues("Authorization", out var headerAuthValues)
            ? headerAuthValues?.FirstOrDefault()
            : null;

        if (!IsAuthorized(authHeaderValue))
        {
            var unatuhorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
            unatuhorizedResponse.Headers.Add("WWW-Authenticate", @"Basic realm=""B2C""");

            await unatuhorizedResponse.WriteStringAsync("Unauthorized");
            context.GetInvocationResult().Value = unatuhorizedResponse;
            return;
        }

        await next(context);
    }

    private static bool IsAuthorized(string? headerValue)
    {
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith(BasicAuthSchemeWithSpace, StringComparison.OrdinalIgnoreCase))
            return false;

        var encodedUserAndPassword = header.Substring(BasicAuthSchemeWithSpace.Length).Trim();
        string decodedUserAndPassword;
        try
        {
            decodedUserAndPassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUserAndPassword));
        }
        catch
        {
            return false;
        }

        var splitIndex = decodedUserAndPassword.IndexOf(UserPasswordSplitChar);
        if (splitIndex < 0)
            return false;

        var user = decodedUserAndPassword[..splitIndex];
        var password = decodedUserAndPassword[(splitIndex + 1)..];

        var expectedUser = Environment.GetEnvironmentVariable("BASIC_AUTH__USERNAME");
        var expectedPassword = Environment.GetEnvironmentVariable("BASIC_AUTH__PASSWORD");

        return !string.IsNullOrEmpty(expectedUser)
            && !string.IsNullOrEmpty(expectedPassword)
            && string.Equals(user, expectedUser, StringComparison.Ordinal)
            && string.Equals(password, expectedPassword, StringComparison.Ordinal);
    }
}
