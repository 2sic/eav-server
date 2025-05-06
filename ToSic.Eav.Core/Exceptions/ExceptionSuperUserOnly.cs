using ToSic.Lib.Code.Help;

namespace ToSic.Eav;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ExceptionSuperUserOnly(Exception inner = null) : ExceptionWithHelp(SuperUserHelp(null), inner)
{
    private static CodeHelp SuperUserHelp(string message) => new(name: "super-user-help", detect: null,
        uiMessage: message ?? "Dev/SuperUser 👨🏽‍💻 ERROR INFORMATION",
        detailsHtml: "Only SuperUsers and Devs 👨🏽‍💻 see this message. Normal users won't see it");

}