﻿using ToSic.Lib.Coding;

namespace ToSic.Eav.Apps.Internal.Insights;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IInsightsLinker
{
    string LinkTo(string label, string view, int? appId = null, NoParamOrder noParamOrder = default,
        string key = null, string type = null, string nameId = null, string more = null);

    string LinkTo(string name, NoParamOrder protector = default, string label = default, string parameters = default);

    string LinkBack();
}