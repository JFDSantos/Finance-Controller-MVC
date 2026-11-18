using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Finance.Tests.Integration;

public class HttpRulesTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    public void Dispose() => _factory.Dispose();

    private HttpClient Client(ApiFactory? f = null) =>
        (f ?? _factory).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Post_WithFormContentType_Returns415()
    {
        var res = await Client().PostAsync("/api/Income",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["a"] = "b" }));
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, res.StatusCode);
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        Assert.Equal("UNSUPPORTED_MEDIA_TYPE", doc.RootElement.GetProperty("code").GetString());
        Assert.True(doc.RootElement.TryGetProperty("message", out _));
    }

    [Fact]
    public async Task Post_WithJsonContentTypeAndNoToken_Returns401()
    {
        var res = await Client().PostAsync("/api/Income",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Post_WithJsonCharsetParameter_IsAccepted()
    {
        var content = new StringContent("{}", Encoding.UTF8);
        content.Headers.ContentType = new("application/json") { CharSet = "utf-8" };
        var res = await Client().PostAsync("/api/Income", content);
        Assert.NotEqual(HttpStatusCode.UnsupportedMediaType, res.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutBody_IsNotBlockedByJsonRule()
    {
        var res = await Client().DeleteAsync("/api/Income?id=1");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Controllers_AreUnder_ApiPrefix()
    {
        var c = Client();
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/Category")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/Category")).StatusCode);
    }

    [Fact]
    public async Task ForwardedProtoHttps_IsHonoredByHttpsRedirection()
    {
        using var f = new ApiFactory(new Dictionary<string, string?> { ["HTTPS_PORT"] = "443" });
        var c = Client(f);

        // Plain http without the proxy header is redirected to https.
        var plain = await c.GetAsync("http://localhost/api/Category");
        Assert.Equal(HttpStatusCode.TemporaryRedirect, plain.StatusCode);

        // Behind the TLS-terminating proxy the app sees https: no redirect.
        var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Category");
        req.Headers.Add("X-Forwarded-Proto", "https");
        req.Headers.Add("X-Forwarded-For", "203.0.113.7");
        var res = await c.SendAsync(req);
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
