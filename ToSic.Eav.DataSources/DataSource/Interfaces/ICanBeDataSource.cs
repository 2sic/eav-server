namespace ToSic.Eav.DataSource;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ICanBeDataSource
{
    IDataSource DataSource { get; }
}