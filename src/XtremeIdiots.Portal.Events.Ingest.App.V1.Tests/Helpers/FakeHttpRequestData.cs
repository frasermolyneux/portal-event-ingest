using System.Net;
using System.Security.Claims;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Helpers;

internal class FakeHttpRequestData : HttpRequestData
{
    public FakeHttpRequestData(FunctionContext context, Uri url, Stream? body = null) : base(context)
    {
        Url = url;
        Headers = new HttpHeadersCollection();
        Body = body ?? new MemoryStream();
    }

    public override Stream Body { get; }
    public override HttpHeadersCollection Headers { get; }
    public override IReadOnlyCollection<IHttpCookie> Cookies => Array.Empty<IHttpCookie>();
    public override Uri Url { get; }
    public override IEnumerable<ClaimsIdentity> Identities => Array.Empty<ClaimsIdentity>();
    public override string Method => "POST";

    public override HttpResponseData CreateResponse()
    {
        return new FakeHttpResponseData(FunctionContext);
    }
}
