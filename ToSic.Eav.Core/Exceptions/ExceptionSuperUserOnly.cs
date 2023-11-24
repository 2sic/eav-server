using System;
using ToSic.Eav.Code.Help;

namespace ToSic.Eav;

public class ExceptionSuperUserOnly: ExceptionWithHelp
{
    public ExceptionSuperUserOnly(/*CodeHelp help, */Exception inner = null) : base(SuperUserHelp(null), inner)
    {
    }

    private static CodeHelp SuperUserHelp(string message) => new(name: "super-user-help", detect: null,
        uiMessage: message ?? "Dev/SuperUser 👨🏽‍💻 ERROR INFORMATION",
        detailsHtml: "Only SuperUsers and Devs 👨🏽‍💻 see this message. Normal users won't see it");

}