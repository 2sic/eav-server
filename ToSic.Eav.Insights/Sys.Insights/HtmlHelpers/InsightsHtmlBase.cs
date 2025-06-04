using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using ToSic.Lib.Coding;
using ToSic.Razor.Blade;
using ToSic.Razor.Html5;

namespace ToSic.Eav.Sys.Insights.HtmlHelpers;

public class InsightsHtmlBase: IInsightsLinker
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

    internal A LinkTo(string label, string view, int? appId = null, NoParamOrder noParamOrder = default,
        string key = null, string type = null, string nameId = null, string more = null)
    { 
        var link = UrlTo(view, appId, key: key, type: type, nameId: nameId, more: more);
        return Tag.A(label).Href(link);
    }

    public string LinkTo(string name, NoParamOrder protector = default, string label = default, string parameters = default)
    {
        throw new NotImplementedException();
    }

    string IInsightsLinker.LinkBack() => LinkBack().ToString();

    string IInsightsLinker.LinkTo(string label, string view, int? appId, NoParamOrder noParamOrder, string key,
        string type, string nameId, string more) =>
        LinkTo(label, view, appId, noParamOrder, key, type, nameId, more).ToString();

    internal A LinkBack() => Tag.A( HtmlEncode("🔙 Back")).On("click", "history.back();");

    protected bool NiceLink = true;

    private string UrlTo(string view, int? appId = null, NoParamOrder noParamOrder = default,
        string key = null, string type = null, string nameId = null, string more = null)
    {
        var link = (NiceLink ? $"./{view}?" : $"details?view={view}")
                   + (appId != null ? "&appid=" + appId : "")
                   + (type != null ? "&type=" + type : "")
                   + (key != null ? "&key=" + UrlEncoder.Default.Encode(key) : "")
                   + (nameId != null ? "&nameId=" + nameId : "")
                   + (more != null ? "&" + more : "");
        return link;
    }


    #endregion
}