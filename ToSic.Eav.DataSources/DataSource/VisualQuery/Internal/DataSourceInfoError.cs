namespace ToSic.Eav.DataSource.VisualQuery.Internal;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class DataSourceInfoError(string title, string message)
{
    public string Title { get; set; } = title;
    public string Message { get; set; } = message;
}