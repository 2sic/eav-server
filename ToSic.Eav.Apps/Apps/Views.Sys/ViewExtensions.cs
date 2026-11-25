using ToSic.Eav.Apps.Views.Sys;

namespace ToSic.Sxc.Blocks.Sys.Views;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ViewExtensions
{

    public static bool IsRazor(this IView view) => view.Type == ViewConstants.TypeRazorValue;

    /// <summary>
    /// Get the engine name for DI injection
    /// </summary>
    public static string GetEngineName(this IView view)
        => view.Type == ViewConstants.TypeRazorValue
            ? "razor"
            : "token";

    public static IEntity? GetQueryEntity(this IView view) => view.Entity.Children(ViewConstants.FieldPipeline).FirstOrDefault();

}