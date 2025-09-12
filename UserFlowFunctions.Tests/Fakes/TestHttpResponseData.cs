using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace UserFlowFunctions.Tests.Fakes;

public sealed class TestHttpResponseData(FunctionContext functionContext) : HttpResponseData(functionContext)
{
    public override HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public override HttpHeadersCollection Headers { get; set; } = [];
    public override Stream Body { get; set; } = new MemoryStream();

    public override HttpCookies Cookies { get; } = new TestHttpResponseCookies();

    private sealed class TestHttpResponseCookies : HttpCookies
    {
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<IHttpCookie> _cookies = [];

        public override void Append(string name, string value)
        {
            var httpCookie = CreateNew();
            _cookies.Add(httpCookie);
        }

        public override void Append(IHttpCookie cookie)
        {
            _cookies.Add(cookie);
        }

        public override IHttpCookie CreateNew()
        {
            return new HttpCookie("some-name", "some-value");
        }
    }
}
