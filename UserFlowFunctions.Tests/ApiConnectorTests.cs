using System.Net;
using System.Text.Json;
using UserFlowFunctions.B2C;
using UserFlowFunctions.Tests.Fakes;

namespace UserFlowFunctions.Tests;

public class ApiConnectorTests
{
    private const string _validExtensionKey = "extension_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa_Custom";

    [Fact]
    public async Task Continue_Merges_Claims_And_Sets_Action_And_Version()
    {
        var (req, _) = HttpFakes.MakeHttp();

        var claims = new Dictionary<string, object>
        {
            ["postalCode"] = "12349",
            [_validExtensionKey] = "value"
        };

        var res = await ApiConnector.ContinueAsync(req, claims);

        res.StatusCode.ShouldBe(HttpStatusCode.OK);
        res.GetHeader("Content-Type").ShouldContain("application/json");

        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("version").GetString().ShouldBe(ApiConnector.ApiVersion);
        root.GetProperty("action").GetString().ShouldBe("Continue");
        root.GetProperty("postalCode").GetString().ShouldBe("12349");
        root.GetProperty(_validExtensionKey).GetString().ShouldBe("value");
    }

    [Fact]
    public async Task ShowBlockPage_Is_200_With_Message()
    {
        var (req, _) = HttpFakes.MakeHttp();
        var res = await ApiConnector.ShowBlockPageAsync(req, "There was a problem with your request.");
        res.StatusCode.ShouldBe(HttpStatusCode.OK);

        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("version").GetString().ShouldBe(ApiConnector.ApiVersion);
        root.GetProperty("action").GetString().ShouldBe("ShowBlockPage");
        root.GetProperty("userMessage").GetString().ShouldBe("There was a problem with your request.");
    }

    [Fact]
    public async Task ValidationError_Is_400_And_BodyStatus_400()
    {
        var (req, _) = HttpFakes.MakeHttp();
        var res = await ApiConnector.ValidationErrorAsync(req, "Please enter a valid Postal Code.");

        res.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        res.GetHeader("Content-Type").ShouldContain("application/json");

        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("version").GetString().ShouldBe(ApiConnector.ApiVersion);
        root.GetProperty("action").GetString().ShouldBe("ValidationError");
        root.GetProperty("status").GetInt32().ShouldBe(400);
        root.GetProperty("userMessage").GetString().ShouldBe("Please enter a valid Postal Code.");
    }
}
