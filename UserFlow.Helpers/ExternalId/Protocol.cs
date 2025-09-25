using JetBrains.Annotations;

namespace UserFlow.Helpers.ExternalId;

/// <summary>
/// Well-known JSON property names and <c>@odata.type</c> discriminator values
/// for Microsoft Entra External ID user-flow Start/Submit responses.
/// </summary>
[PublicAPI]
public static class Protocol
{
    /// <summary>Well-known JSON property names used in Start/Submit payloads.</summary>
    public static class Json
    {
        /// <summary>OData discriminator property name.</summary>
        public const string ODataType = "@odata.type";

        /// <summary>Envelope property that contains response data.</summary>
        public const string Data = "data";

        /// <summary>List of action objects applied by the user flow.</summary>
        public const string Actions = "actions";

        /// <summary>Inputs to prefill on the Start page.</summary>
        public const string Inputs = "inputs";

        /// <summary>Attribute values to modify on Submit.</summary>
        public const string Attributes = "attributes";

        /// <summary>Optional title for a block page action.</summary>
        public const string Title = "title";

        /// <summary>Message text for block/validation actions.</summary>
        public const string Message = "message";

        /// <summary>Per-attribute error messages keyed by attribute name.</summary>
        public const string AttributeErrors = "attributeErrors";
    }

    /// <summary>Well-known <c>@odata.type</c> values used in the response contract.</summary>
    public static class OData
    {
        /// <summary><c>@odata.type</c> values for the <c>data</c> envelope.</summary>
        public static class Data
        {
            /// <summary>Start-event data envelope type.</summary>
            public const string Start = "microsoft.graph.onAttributeCollectionStartResponseData";

            /// <summary>Submit-event data envelope type.</summary>
            public const string Submit = "microsoft.graph.onAttributeCollectionSubmitResponseData";
        }

        /// <summary><c>@odata.type</c> values for action objects.</summary>
        public static class Actions
        {
            /// <summary>Continue with default behavior (Start).</summary>
            public const string StartContinue = "microsoft.graph.attributeCollectionStart.continueWithDefaultBehavior";

            /// <summary>Prefill values before rendering (Start).</summary>
            public const string StartPrefill = "microsoft.graph.attributeCollectionStart.setPrefillValues";

            /// <summary>Show a block page (Start).</summary>
            public const string StartBlock = "microsoft.graph.attributeCollectionStart.showBlockPage";

            /// <summary>Continue with default behavior (Submit).</summary>
            public const string SubmitContinue = "microsoft.graph.attributeCollectionSubmit.continueWithDefaultBehavior";

            /// <summary>Modify submitted attributes (Submit).</summary>
            public const string SubmitModify = "microsoft.graph.attributeCollectionSubmit.modifyAttributeValues";

            /// <summary>Show field-level validation (Submit).</summary>
            public const string SubmitValidate = "microsoft.graph.attributeCollectionSubmit.showValidationError";

            /// <summary>Show a block page (Submit).</summary>
            public const string SubmitBlock = "microsoft.graph.attributeCollectionSubmit.showBlockPage";
        }
    }
}
