using ToSic.Eav.Plumbing;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

partial class InsightsControllerReal
{

    private string LightSpeedStats()
    {
        var msg = H1("LightSpeed Stats").ToString();
        try
        {
            var countStats = _lightSpeedStats.ItemsCount;
            var sizeStats = _lightSpeedStats.Size;
            msg += P($"Apps in Cache: {countStats.Count}");
            msg += "<table id='table'>"
                   + InsightsHtmlTable.HeadFields("#", "ZoneId", "AppId", "Name", "Items in Cache", "Ca. Memory Use", "NameId")
                   + "<tbody>";
            var count = 0;
            var totalItems = 0;
            var totalMemory = 0L;
            foreach (var md in countStats)
            {
                var appState = AppState(md.Key);
                msg += InsightsHtmlTable.RowFields(
                    ++count,
                    SpecialField.Right(appState.ZoneId),
                    SpecialField.Right(md.Key),
                    appState.Name,
                    SpecialField.Right(md.Value),
                    SpecialField.Right(sizeStats.TryGetValue(md.Key, out var size) ? ByteToKByte(size) : Constants.NullNameId),
                    appState.NameId
                );
                totalItems += md.Value;
                totalMemory += size;
            }
            msg += "</tbody>";
            msg += "<tfoot>";
            msg += InsightsHtmlTable.RowFields(
                B("Total:"),
                "",
                "",
                "",
                SpecialField.Right(B($"{totalItems}")),
                SpecialField.Right(B(ByteToKByte(totalMemory))),
                ""
            );

            msg += "</tfoot>";
            msg += "</table>";
            msg += "\n\n";
            msg += InsightsHtmlParts.JsTableSort();
        }
        catch
        {
            // ignored
        }
        return msg;
    }

    private string ByteToKByte(long bytes)
    {
        const int kb = 1024;
        if (bytes < kb) return bytes + "b";

        const int mb = kb * kb;
        if (bytes < 10 * mb)
            return ((double)bytes / kb).ToAposString() + "kb";

        return ((double)bytes / mb).ToAposString() + "mb";
    }
}