using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using NSubstitute;

namespace UserFlowFunctions.Tests.Fakes;

internal static class HttpFakes
{
    public static (HttpRequestData Req, HttpResponseData Res) MakeHttp(string? json = null)
    {
        var functionContext  = Substitute.For<FunctionContext>();
        var body = json is null ? new MemoryStream() : new MemoryStream(Encoding.UTF8.GetBytes(json));
        var requestData  = new TestHttpRequestData(functionContext, new Uri("https://localhost/unit-test"), body);
        var responseData  = requestData.CreateResponse(); // TestHttpResponseData
        return (requestData, responseData);
    }

    public static async Task<string> ReadBodyAsync(HttpResponseData responseData)
    {
        responseData.Body.Position = 0;
        using var streamReader = new StreamReader(responseData.Body, Encoding.UTF8, leaveOpen: true);
        return await streamReader.ReadToEndAsync();
    }

    public static IEnumerable<string> GetHeader(this HttpResponseData responseData, string name)
        => responseData.Headers.TryGetValues(name, out var values) ? values : [];
}
