using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys
{
    public partial class InsightsControllerReal
    {

        private string LightSpeedStats()
        {
            var msg = H1("LightSpeed Stats").ToString();
            try
            {
                var countStats = _lightSpeedStats.ItemsCount;
                var sizeStats = _lightSpeedStats.Size;
                msg += P($"Assigned Items: {countStats.Count}");
                msg += "<table id='table'>"
                       + HeadFields("#", "ZoneId", "AppId", "Name", "Items in Cache", "Ca. Memory Use", "NameId")
                       + "<tbody>";
                var count = 0;
                var totalItems = 0;
                var totalMemory = 0L;
                foreach (var md in countStats)
                {
                    var appState = AppState(md.Key);
                    msg += RowFields(
                        ++count,
                        appState.ZoneId,
                        md.Key,
                        appState.Name,
                        md.Value,
                        sizeStats.TryGetValue(md.Key, out var size) ? ByteToKByte(size) : "unknown",
                        appState.NameId
                    );
                    totalItems += md.Value;
                    totalMemory += size;
                }
                msg += RowFields(
                    B("Total:"),
                    "",
                    "",
                    "",
                    B($"{totalItems}"),
                    B(ByteToKByte(totalMemory)),
                    ""
                );

                msg += "</tbody>";
                msg += "</table>";
                msg += "\n\n";
                msg += JsTableSort();
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
                return (bytes / kb).ToString("N0") + "kb";

            return (bytes / mb).ToString("N" + "mb");
        }
    }
}
