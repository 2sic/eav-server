using System;
using System.Text;
using System.Web;
using ToSic.Razor.Blade;
using ToSic.Razor.Html5;

namespace ToSic.Eav.WebApi.Sys.Insights
{
    internal class InsightsHtmlBase
    {
        internal static string HtmlEncode(string text)
        {
            if (text == null) return "";
            var chars = HttpUtility.HtmlEncode(text).ToCharArray();
            var result = new StringBuilder(text.Length + (int)(text.Length * 0.1));

            foreach (var c in chars)
            {
                var value = Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        internal static string EmojiTrueFalse(bool value) => HtmlEncode(value ? "✅" : "⛔");

        internal static string HoverLabel(string label, string text, string classes)
            => Tag.Span(label).Class(classes).Title(text).ToString();

        #region Linking

        internal A DemoLink(string labelAndLink) => Tag.A(labelAndLink).Href(labelAndLink);

        internal A LinkTo(string label, string view,
            int? appId = null, string noParamOrder = Eav.Parameters.Protector,
            string key = null, string type = null, string nameId = null, string more = null)
        {
            Eav.Parameters.Protect(noParamOrder, "...");
            var link = UrlTo(view, appId, key: key, type: type, nameId: nameId, more: more);
            return Tag.A(label).Href(link);
        }

        protected bool NiceLink = true;

        private string UrlTo(string view, int? appId = null, string noParamOrder = Eav.Parameters.Protector,
            string key = null, string type = null, string nameId = null, string more = null)
        {
            Eav.Parameters.ProtectAgainstMissingParameterNames(noParamOrder, nameof(UrlTo), "...");
            var link = (NiceLink ? $"./{view}?" : $"details?view={view}")
                       + (appId != null ? "&appid=" + appId : "")
                       + (type != null ? "&type=" + type : "")
                       + (key != null ? "&key=" + key : "")
                       + (nameId != null ? "&nameId=" + nameId : "")
                       + (more != null ? "&" + more : "");
            return link;
        }


        #endregion
    }
}
