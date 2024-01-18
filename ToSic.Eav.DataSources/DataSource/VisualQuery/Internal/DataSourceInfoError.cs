namespace ToSic.Eav.DataSource.VisualQuery.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class DataSourceInfoError(string title, string message)
{
    public string Title { get; set; } = title;
    public string Message { get; set; } = message;
}