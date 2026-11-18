namespace Finance.Tests.Integration;

public class SmokeTests
{
    [Fact(Skip = "until Task 10")]
    public async Task Smoke_ApiStarts()
    {
        await using var factory = new ApiFactory();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
