namespace ToSic.Eav.Metadata
{
    public class Decorators
    {
        public static string MessageField = "Message";

        // Is-Obsolete Decorator
        public static string IsObsoleteDecoratorId = "852fe15e-bf23-44e6-a856-0a130203496c";

        // Is-Default Decorator
        public static string IsDefaultDecorator = "529ba3a2-d7d4-4f40-a81b-ff819de03a9e";

        // Priority Decorator - not included ATM
        //public static string PriorityDecoratorId = "1291dbc0-9c52-4c79-8c95-698e5c3aa299";
        //public static string PriorityDefaultField = "IsDefault";

        // Is-Recommended Decorator
        public static string RecommendedDecoratorId = "c740085a-d548-41f3-8d06-0a48b8692345";



        // This marks entities which should allow saving empty
        public static string SaveEmptyDecoratorId = "8c78dabf-e0ad-4c26-b750-48138ecb8a39";

        // Marks Metadata to be global 13.00
        public static string IsSharedDecoratorId = "18d2b1db-e1ed-45bc-8746-cd8885651063";

        // Note Decorator 13.03
        public static string NoteDecoratorName = "NoteDecorator";
        public static string NoteDecoratorId = "5e958dc6-2922-4d68-835c-7b9711538b12";

        // Edit UI configuration 13.03
        public static string EditUiConfigurationDecoratorId = "4f6d1484-4672-43d5-9c48-94ff3ec11069";

        //public static string LightSpeedOutputDecoratorId = "be34f64b-7d1f-4ad0-b488-dabbbb01a186";

        // OpenGraph WIP v14 - should be moved to 2sxc
        public static string OpenGraphName = "OpenGraph";
        public static string OpenGraphId = "1f1c8118-8ea5-4db5-8e3d-f5ef2131050b";

        // Export Configuration v15
        // moved to the ExportDecorator
        //public static string SystemExportDecorator = "32698880-1c2e-41ab-bcfc-420091d3263f";
    }
}
