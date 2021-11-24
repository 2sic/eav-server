using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSourceTests.RelationshipFilterTests
{
    internal class RelationshipTestCase : RelationshipTestBase
    {
        internal string Name;
        internal RelationshipFilter Api;
        internal RelationshipFilter Config;

        internal IEnumerable<IEntity> All => Api.InForTests()[Constants.DefaultStreamName].ListForTests();
        internal int CountAll => All.Count();

        internal int CountApi => Api.ListForTests().Count();
        internal int CountConfig => Config.ListForTests().Count();


        public string Type, 
            Relationship, 
            Filter, 
            RelatedAttribute, 
            CompareMode, 
            Separator, 
            Direction;
        internal RelationshipTestCase(string name, 
            string type, 
            string relationship = null, 
            string filter = null, 
            string relAttribute = null,
            string compareMode = null,
            string separator = null,
            string direction = null)
        {
            Name = name;
            Type = type;
            Relationship = relationship;
            Filter = filter;
            RelatedAttribute = relAttribute;
            CompareMode = compareMode;
            Separator = separator;
            Direction = direction;
        }

        private RelationshipTestCase BuildObjects()
        {
            // test using api configuration
            Api = FilterWithApi(Type, 
                Relationship, 
                Filter,
                relAttrib: RelatedAttribute,
                compareMode:CompareMode,
                separator: Separator,
                direction: Direction);

            // Identical test with configuration providing
            Config = FilterWithConfig(Type,
                Relationship,
                Filter,
                relAttrib: RelatedAttribute,
                compareMode: CompareMode,
                separator: Separator,
                direction: Direction);

            return this;
        }

        internal void Run(bool expectsResults, bool shouldReturnAll = false, int exactCount = -1)
        {
            BuildObjects();

            var x = CountApi + CountConfig; // access the streams to ensure it's logged
            Trace.Write("Log after accessing DSs\n\n" + Log.Dump());

            Api.ListForTests().ToList().ForEach(e => Trace.WriteLine($"item ({e.EntityId}):'{e.GetBestTitle()}'"));

            Assert.IsTrue(expectsResults ? CountApi > 0 : CountApi == 0, $"test: {Name} - found-Count:{CountApi} > 0");
            if(exactCount != -1)
                Assert.AreEqual(exactCount, CountApi, $"test: {Name} - missed expected exact count");

            if (shouldReturnAll)
                Assert.AreEqual(CountApi, CountAll, $"test: {Name} - all == count not met");
            else
                Assert.IsTrue(CountApi < CountAll, $"test: {Name} - foundCount < allComps");

            Assert.AreEqual(CountApi, CountConfig, $"test: {Name} - api and config should be the same");
        }
    }
}
