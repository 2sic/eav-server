using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources
{
    internal class DataSourceErrorHandling
    {
        public static string ErrorType = "Error";
        public static string ErrorTitle = "Title";
        public static IEntity CreateErrorEntity(IDataSource source, string stream, string title, string message)
        {
            var values = new Dictionary<string, object>
            {
                {ErrorTitle, "Error: " + title},
                {"SourceName", source?.Name},
                {"SourceLabel", source?.Label },
                {"SourceGuid", source?.Guid },
                {"SourceStream", stream },
                {"Message", message }
            };

            var errorEntity = Build.Entity(values, titleField: ErrorTitle, typeName: ErrorType);
            return errorEntity;
        }

        public static ImmutableArray<IEntity> CreateErrorList(IDataSource source, string stream, string title, string message)
        {
            // Construct the IEntity and return as ImmutableArray
            var entity = CreateErrorEntity(source, stream, title, message);
            return new[] { entity }.ToImmutableArray();
        }
    }
}
