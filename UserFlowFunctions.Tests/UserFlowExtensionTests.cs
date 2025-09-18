using System.Net;
using System.Text.Json;
using UserFlowFunctions.ExternalId;
using UserFlowFunctions.Tests.Fakes;

namespace UserFlowFunctions.Tests;

public class UserFlowExtensionTests
{
    [Theory]
    [InlineData(true,  Protocol.OData.Data.Start,  Protocol.OData.Actions.StartContinue)]
    [InlineData(false, Protocol.OData.Data.Submit, Protocol.OData.Actions.SubmitContinue)]
    public async Task Continue_Emits_Expected_OData_And_Action(bool isStart, string expectedData, string expectedAction)
    {
        var (req, _) = HttpFakes.MakeHttp();
        var res = isStart
            ? await UserFlowExtension.StartContinueAsync(req)
            : await UserFlowExtension.SubmitContinueAsync(req);

        res.StatusCode.ShouldBe(HttpStatusCode.OK);
        res.GetHeader("Content-Type").ShouldContain("application/json");

        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var data   = doc.RootElement.GetProperty(Protocol.Json.Data);
        var action = data.GetProperty(Protocol.Json.Actions)[0];

        data.GetProperty(Protocol.Json.ODataType).GetString()
            .ShouldBe(expectedData);

        action.GetProperty(Protocol.Json.ODataType).GetString()
            .ShouldBe(expectedAction);
    }

    [Fact]
    public async Task StartSetPrefillValues_Emits_Inputs()
    {
        var (req, _) = HttpFakes.MakeHttp();

        var res = await UserFlowExtension.StartSetPrefillValuesAsync(
            req, new Dictionary<string, object> { ["city"] = "Oslo" });

        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var action = doc.RootElement
            .GetProperty(Protocol.Json.Data)
            .GetProperty(Protocol.Json.Actions)[0];

        action.GetProperty(Protocol.Json.ODataType).GetString()
            .ShouldBe(Protocol.OData.Actions.StartPrefill);

        action.GetProperty(Protocol.Json.Inputs)
              .GetProperty("city").GetString()
              .ShouldBe("Oslo");
    }

    [Fact]
    public async Task StartShowBlockPage_Emits_Title_And_Message()
    {
        var (req, _) = HttpFakes.MakeHttp();
        var res = await UserFlowExtension.StartShowBlockPageAsync(req, "blocked body", "blocked title");
        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var action = doc.RootElement
            .GetProperty(Protocol.Json.Data)
            .GetProperty(Protocol.Json.Actions)[0];

        action.GetProperty(Protocol.Json.ODataType).GetString()
            .ShouldBe(Protocol.OData.Actions.StartBlock);

        action.GetProperty(Protocol.Json.Title).GetString().ShouldBe("blocked title");
        action.GetProperty(Protocol.Json.Message).GetString().ShouldBe("blocked body");
    }

    [Fact]
    public async Task SubmitModifyAttributes_Emits_Attributes()
    {
        var (req, _) = HttpFakes.MakeHttp();

        var res = await UserFlowExtension.SubmitModifyAttributesAsync(
            req, new Dictionary<string, object> { ["email"] = "user@contoso.test" });

        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var action = doc.RootElement
            .GetProperty(Protocol.Json.Data)
            .GetProperty(Protocol.Json.Actions)[0];

        action.GetProperty(Protocol.Json.ODataType).GetString()
            .ShouldBe(Protocol.OData.Actions.SubmitModify);

        action.GetProperty(Protocol.Json.Attributes)
              .GetProperty("email").GetString()
              .ShouldBe("user@contoso.test");
    }

    [Fact]
    public async Task SubmitShowValidationError_Emits_Message_And_AttributeErrors()
    {
        var (req, _) = HttpFakes.MakeHttp();

        var res = await UserFlowExtension.SubmitShowValidationErrorAsync(
            req,
            "Please fix the below errors to proceed.",
            new Dictionary<string, string> { ["city"] = "City cannot contain any numbers" });

        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var action = doc.RootElement
            .GetProperty(Protocol.Json.Data)
            .GetProperty(Protocol.Json.Actions)[0];

        action.GetProperty(Protocol.Json.ODataType).GetString()
            .ShouldBe(Protocol.OData.Actions.SubmitValidate);

        action.GetProperty(Protocol.Json.Message).GetString()
            .ShouldBe("Please fix the below errors to proceed.");

        action.GetProperty("attributeErrors").GetProperty("city").GetString()
            .ShouldBe("City cannot contain any numbers");
    }

    [Fact]
    public async Task SubmitShowBlockPage_Emits_Title_And_Message()
    {
        var (req, _) = HttpFakes.MakeHttp();

        var res = await UserFlowExtension.SubmitShowBlockPageAsync(req, "blocked", "please wait");
        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var action = doc.RootElement
            .GetProperty(Protocol.Json.Data)
            .GetProperty(Protocol.Json.Actions)[0];

        action.GetProperty(Protocol.Json.ODataType).GetString()
            .ShouldBe(Protocol.OData.Actions.SubmitBlock);

        action.GetProperty(Protocol.Json.Title).GetString().ShouldBe("please wait");
        action.GetProperty(Protocol.Json.Message).GetString().ShouldBe("blocked");
    }
}
