﻿using System.Collections.Generic;
using ToSic.Eav.Data;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights
{
    public partial class InsightsControllerReal
    {

        internal static string MetadataTable(string msg, List<IEntity> metadata)
        {
            try
            {
                msg += P($"Assigned Items: {metadata.Count}\n");
                msg += "<table id='table'>"
                       + InsightsHtmlTable.HeadFields( "#", "Id", Eav.Data.Attributes.TitleNiceName, "Content-Type", "Target", "Key" )
                       + "<tbody>";
                var count = 0;
                foreach (var md in metadata)
                {
                    var mdFor = md.MetadataFor;
                    var key = !string.IsNullOrEmpty(mdFor.KeyString)
                        ? "\"" + mdFor.KeyString + "\""
                        : mdFor.KeyNumber != null
                            ? "#" + mdFor.KeyNumber
                            : mdFor.KeyGuid != null
                                ? "{" + mdFor.KeyGuid + "}"
                                : "(directly attached)";

                    msg += InsightsHtmlTable.RowFields(
                        ++count,
                        md.EntityId,
                        md.GetBestTitle(),
                        md.Type.Name,
                        mdFor.TargetType,
                        key
                    );
                }
                msg += "</tbody>";
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
    }
}
