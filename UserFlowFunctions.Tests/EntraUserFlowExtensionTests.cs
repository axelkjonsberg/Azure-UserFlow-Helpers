using System.Net;
using System.Text.Json;
using UserFlowFunctions.Tests.Fakes;

namespace UserFlowFunctions.Tests;

public class EntraUserFlowExtensionTests
{
    [Fact]
    public async Task Start_Continue_Uses_Correct_OData_And_Action()
    {
        var (req, _) = HttpFakes.MakeHttp();

        var res = await EntraUserFlowExtension.Start_ContinueAsync(req);
        res.StatusCode.ShouldBe(HttpStatusCode.OK);
        res.GetHeader("Content-Type").ShouldContain("application/json");

        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");

        data.GetProperty("@odata.type").GetString()
            .ShouldBe("microsoft.graph.onAttributeCollectionStartResponseData");

        var action = data.GetProperty("actions")[0];
        action.GetProperty("@odata.type").GetString()
            .ShouldBe("microsoft.graph.attributeCollectionStart.continueWithDefaultBehavior");
    }

    [Fact]
    public async Task Start_SetPrefillValues_Emits_Inputs()
    {
        var (req, _) = HttpFakes.MakeHttp();

        var res = await EntraUserFlowExtension.Start_SetPrefillValuesAsync(
            req, new Dictionary<string, object> { ["city"] = "Oslo" });

        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var action = doc.RootElement.GetProperty("data").GetProperty("actions")[0];

        action.GetProperty("@odata.type").GetString()
            .ShouldBe("microsoft.graph.attributeCollectionStart.setPrefillValues");
        action.GetProperty("inputs").GetProperty("city").GetString().ShouldBe("Oslo");
    }

    [Fact]
    public async Task Start_ShowBlockPage_Allows_Title_And_Message()
    {
        var (req, _) = HttpFakes.MakeHttp();
        var res = await EntraUserFlowExtension.Start_ShowBlockPageAsync(req, "blocked body", "blocked title");
        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var action = doc.RootElement.GetProperty("data").GetProperty("actions")[0];

        action.GetProperty("@odata.type").GetString()
            .ShouldBe("microsoft.graph.attributeCollectionStart.showBlockPage");
        action.GetProperty("title").GetString().ShouldBe("blocked title");
        action.GetProperty("message").GetString().ShouldBe("blocked body");
    }

    [Fact]
    public async Task Submit_ModifyAttributes_Emits_Attributes()
    {
        var (req, _) = HttpFakes.MakeHttp();

        var res = await EntraUserFlowExtension.Submit_ModifyAttributesAsync(
            req, new Dictionary<string, object> { ["email"] = "user@contoso.test" });

        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var action = doc.RootElement.GetProperty("data").GetProperty("actions")[0];

        action.GetProperty("@odata.type").GetString()
            .ShouldBe("microsoft.graph.attributeCollectionSubmit.modifyAttributeValues");
        action.GetProperty("attributes").GetProperty("email").GetString().ShouldBe("user@contoso.test");
    }

    [Fact]
    public async Task Submit_ShowValidationError_Emits_Message_And_AttributeErrors()
    {
        var (req, _) = HttpFakes.MakeHttp();

        var res = await EntraUserFlowExtension.Submit_ShowValidationErrorAsync(
            req,
            "Please fix the below errors to proceed.",
            new Dictionary<string, string> { ["city"] = "City cannot contain any numbers" });

        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var action = doc.RootElement.GetProperty("data").GetProperty("actions")[0];

        action.GetProperty("@odata.type").GetString()
            .ShouldBe("microsoft.graph.attributeCollectionSubmit.showValidationError");
        action.GetProperty("message").GetString().ShouldBe("Please fix the below errors to proceed.");
        action.GetProperty("attributeErrors").GetProperty("city").GetString()
            .ShouldBe("City cannot contain any numbers");
    }

    [Fact]
    public async Task Submit_ShowBlockPage_Allows_Title_And_Message()
    {
        var (req, _) = HttpFakes.MakeHttp();

        var res = await EntraUserFlowExtension.Submit_ShowBlockPageAsync(req, "blocked", "please wait");
        var json = await HttpFakes.ReadBodyAsync(res);
        using var doc = JsonDocument.Parse(json);
        var action = doc.RootElement.GetProperty("data").GetProperty("actions")[0];

        action.GetProperty("@odata.type").GetString()
            .ShouldBe("microsoft.graph.attributeCollectionSubmit.showBlockPage");
        action.GetProperty("title").GetString().ShouldBe("please wait");
        action.GetProperty("message").GetString().ShouldBe("blocked");
    }
}
