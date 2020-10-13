//using System.Collections.Generic;
//using ToSic.Eav.Data;
//using ToSic.Eav.Documentation;

//namespace ToSic.Eav.DataSources
//{
//    [PrivateApi("experimental")]
//    public interface IHasListAndIndex<TIndex>
//    {
//        Dictionary<TIndex, IEntity> FastIndex { get; }

//        //IEnumerable<IEntity> List { get; }
//    }

//    public static class HasListAndIndexExtensions
//    {
//        public static IEntity FastOne<T>(this IHasListAndIndex<T> parent, T id)
//        {
//            parent.FastIndex.TryGetValue(id, out var result);
//            return result;
//        }
//    }
//}
