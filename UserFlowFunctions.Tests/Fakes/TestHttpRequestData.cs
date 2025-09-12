using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace UserFlowFunctions.Tests.Fakes;

public sealed class TestHttpRequestData(FunctionContext functionContext, Uri url, Stream? body = null) : HttpRequestData(functionContext)
{
    public override Stream Body { get; } = body ?? new MemoryStream();
    public override HttpHeadersCollection Headers { get; } = [];
    public override IReadOnlyCollection<IHttpCookie> Cookies { get; } = [];
    public override Uri Url { get; } = url;
    public override IEnumerable<ClaimsIdentity> Identities => [];
    public override string Method => "POST";

    public override HttpResponseData CreateResponse() => new TestHttpResponseData(FunctionContext);
}
