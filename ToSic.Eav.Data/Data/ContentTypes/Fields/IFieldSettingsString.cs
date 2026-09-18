using ToSic.Eav.Models;

namespace ToSic.Eav.Data.ContentTypes.Fields;

[WorkInProgressApi("WIP v22")]
public interface IFieldSettingsString : IModelFromEntity
{
    [PrivateApi]
    public static class Constants
    {
        public static string ContentTypeName = "@String";
    }
}