namespace ToSic.Eav.Configuration
{
    public partial class Features : IFeaturesConfiguration
    {
        /// <inheritdoc />
        public string HelpLink
        {
            get => _helpLink;
            set => _helpLink = value;
        }
        private static string _helpLink = "https://2sxc.org/help?tag=features";

        /// <inheritdoc />
        public string InfoLinkRoot
        {
            get => _infoLinkRoot;
            set => _infoLinkRoot = value;
        }
        private static string _infoLinkRoot = "https://2sxc.org/r/f/";


    }
}
