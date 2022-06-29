namespace ToSic.Eav.Data
{
    /// <summary>
    /// WIP interface to mark anything that is an entity or is based on an entity
    /// to simplify checking if something can be used
    ///
    /// 2022-06-29 2dm - started this idea, but not completed. ATM doesn't serve a purpose yet
    /// </summary>
    public interface ICanBeEntity
    {
        IEntity Entity { get; }
    }
}
