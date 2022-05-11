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
                       + HeadFields("#", "AppId", "Items in Cache", "Estimated Size")
                       + "<tbody>";
                var count = 0;
                var total = 0;
                foreach (var md in countStats)
                {
                    msg += RowFields(
                        ++count,
                        md.Key,
                        md.Value,
                        sizeStats.TryGetValue(md.Key, out var size) ? ByteToKByte(size) : "unknown"
                    );
                    total += md.Value;
                }
                msg += RowFields(
                    "Total:",
                    countStats.Count,
                    total
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
