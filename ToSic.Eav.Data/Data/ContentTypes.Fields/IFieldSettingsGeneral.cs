using ToSic.Eav.Models;

namespace ToSic.Eav.Data.ContentTypes.Fields;

public interface IFieldSettingsGeneral : IModelFromEntity
{
    [PrivateApi]
    public static class Constants
    {
        public const string ContentTypeName = "@All";
    }

    /// <summary>
    /// Description of this field.
    /// </summary>
    string Notes { get; }

    /// <summary>
    /// The official input-type - usually something like `@string-default`
    /// </summary>
    string InputType { get; }

    /// <summary>
    /// Determines if this field is ephemeral, meaning it is not stored in the database and only exists temporarily during processing.
    /// </summary>
    bool IsEphemeral { get; }

    /// <summary>
    /// The formulas associated with this field, which can be used for calculations or transformations.
    /// </summary>
    [ContentTypeField(Type = ValueTypes.Entity)]
    object Formulas { get; }

}