using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.Efc
{
    // Todo: this should become a standardized interface for talking to the DB at a very trivial level
    // everything which comes out of this must be EAV typed, nothing is allowed to be db-typed
    public class Efc11Mapper
    {
        public Efc11Mapper(EavDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        private readonly EavDbContext _dbContext;


        #region Language Definitions
        //public List<DimensionDefinition> LanguageDefinitions(int zoneId)
        //{
        //    return _dbContext.ToSicEavDimensions.Where(dd => dd.ZoneId == zoneId).Cast<DimensionDefinition>().ToList();
        //}



        #endregion
    }
}
