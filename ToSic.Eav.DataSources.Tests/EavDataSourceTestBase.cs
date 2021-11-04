//using System.Linq;
//using ToSic.Eav.DataSources;
//using ToSic.Testing.Shared;
//using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

//namespace ToSic.Eav.DataSourceTests
//{
//    public abstract class EavDataSourceTestBase: TestBaseDiEavFullAndDb
//    {
//        /// <summary>
//        /// Verify that the source returns an error as expected
//        /// </summary>
//        /// <param name="source"></param>
//        /// <param name="errTitle"></param>
//        /// <param name="streamName"></param>
//        public static void VerifyStreamIsError(IDataSource source, string errTitle, string streamName = Constants.DefaultStreamName)
//        {
//            IsNotNull(source);
//            var stream = source[streamName];
//            IsNotNull(stream);
//            AreEqual(1, stream.List.Count());
//            var firstAndOnly = stream.List.FirstOrDefault();
//            IsNotNull(firstAndOnly);
//            AreEqual(DataSourceErrorHandling.ErrorType, firstAndOnly.Type.Name);
//            AreEqual(DataSourceErrorHandling.GenerateTitle(errTitle), firstAndOnly.GetBestTitle());
//        }

//    }
//}
