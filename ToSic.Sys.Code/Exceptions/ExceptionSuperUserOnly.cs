using ToSic.Sys.Code.Help;

namespace ToSic.Sys.Exceptions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExceptionSuperUserOnly(Exception? inner = null) : ExceptionWithHelp(SuperUserHelp(null), inner)
{
    private static CodeHelp SuperUserHelp(string? message)
        => new()
        {
            Name = "super-user-help",
            Detect = null,
            UiMessage = message ?? "Dev/SuperUser 👨🏽‍💻 ERROR INFORMATION",
            DetailsHtml = "Only SuperUsers and Devs 👨🏽‍💻 see this message. Normal users won't see it."
        };

}