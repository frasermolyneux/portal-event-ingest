using System.Net;
using System.Collections;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Helpers;

internal class FakeHttpResponseData : HttpResponseData
{
    public FakeHttpResponseData(FunctionContext context) : base(context)
    {
        Headers = new HttpHeadersCollection();
        Body = new MemoryStream();
        Cookies = new FakeHttpCookies();
    }

    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; }
    public override Stream Body { get; set; }
    public override HttpCookies Cookies { get; }
}

internal class FakeHttpCookies : HttpCookies
{
    public override void Append(string name, string value) { }

    public override void Append(IHttpCookie cookie) { }

    public override IHttpCookie CreateNew() => new FakeHttpCookie(string.Empty, string.Empty);
}

internal class FakeHttpCookie(string name, string value) : IHttpCookie
{
    public string Name { get; } = name;
    public string Value { get; } = value;
    public string? Domain { get; set; }
    public DateTimeOffset? Expires { get; set; }
    public bool? HttpOnly { get; set; }
    public double? MaxAge { get; set; }
    public string? Path { get; set; }
    public SameSite SameSite { get; set; }
    public bool? Secure { get; set; }
}
