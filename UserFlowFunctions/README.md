# UserFlowHelpers

Helpers for Microsoft Entra External ID custom authentication extensions and Azure AD B2C API Connectors.

## THIS PACKAGE IS IN PREVIEW STATE; EXPECT CHANGES

---

## This package helps with:

* **External ID custom authentication extensions**: Produce the documented `data` envelope with `@odata.type` action items for **Start** and **Submit** events.
* **Azure AD B2C API Connectors**: Produce the documented JSON bodies for **Continue**, **ShowBlockPage**, and **ValidationError**, including the required HTTP status codes.

Use this when you are implementing user-flow extensions (or B2C API connectors) and want to use the exact responses 
which the Azure Entra ID/B2C endpoints expect.

---

## Typical scenarios

* Prefill attribute values before the page renders.
* Modify submitted attributes, then continue.
* Show a blocking page with a message.
* Return field-level validation errors while keeping the page displayed.

(For B2C API Connectors, return the **Continue / ShowBlockPage / ValidationError** payloads and status codes.)

---

## Relevant Entra External ID/AzureAd B2C documentation

* Entra External ID custom authentication extensions (Start / Submit):

    * [https://learn.microsoft.com/entra/identity-platform/custom-extension-overview](https://learn.microsoft.com/entra/identity-platform/custom-extension-overview)
    * [https://learn.microsoft.com/entra/identity-platform/custom-extension-onattributecollectionstart-retrieve-return-data](https://learn.microsoft.com/entra/identity-platform/custom-extension-onattributecollectionstart-retrieve-return-data)
    * [https://learn.microsoft.com/entra/identity-platform/custom-extension-onattributecollectionsubmit-retrieve-return-data](https://learn.microsoft.com/entra/identity-platform/custom-extension-onattributecollectionsubmit-retrieve-return-data)
* Azure AD B2C API Connector response examples:

    * [https://learn.microsoft.com/azure/active-directory-b2c/add-api-connector](https://learn.microsoft.com/azure/active-directory-b2c/add-api-connector)

---

## License

MIT

---

## Keywords

`Azure AD B2C` · `Microsoft Entra External ID` · `authentication extension` · `user flow` · `API Connector` · 
`Start` · `Submit` · `ValidationError` · `ShowBlockPage` · `Continue` · `Azure Functions` · `isolated worker` · 
`ciam`
