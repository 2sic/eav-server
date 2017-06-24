using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Persistence.Efc.Models;
// using System.Data.Objects.DataClasses;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbShortcuts: BllCommandBase
    {
        public DbShortcuts(DbDataController c) : base(c) { }

        #region helpers
        ///// <summary>
        ///// Clone an Entity in Entity Framework 4
        ///// </summary>
        ///// <remarks>Source: http://www.codeproject.com/Tips/474296/Clone-an-Entity-in-Entity-Framework </remarks>
        //public T CopyEfEntity<T>(T entity) where T : EntityObject
        //{
        //    var copyKeys = false;
        //    var clone =  DbContext.SqlDb.CreateObject<T>();
        //    var pis = entity.GetType().GetProperties();

        //    foreach (var pi in from pi in pis let attrs = (EdmScalarPropertyAttribute[])pi.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false) from attr in attrs where copyKeys || !attr.EntityKeyProperty select pi)
        //        pi.SetValue(clone, pi.GetValue(entity, null), null);

        //    return clone;
        //}
        #endregion
        

        #region Assignment Object Types
        /// <summary>
        /// AssignmentObjectType with specified Name 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ToSicEavAssignmentObjectTypes GetAssignmentObjectType(string name) => DbContext.SqlDb.ToSicEavAssignmentObjectTypes.Single(a => a.Name == name);

        ///// <summary>
        ///// Get all AssignmentObjectTypes with Id and Name
        ///// </summary>
        //public Dictionary<int, string> GetAssignmentObjectTypes()
        //{
        //    return (from a in DbContext.SqlDb.ToSicEavAssignmentObjectTypes
        //            select new { a.AssignmentObjectTypeId, a.Name }).ToDictionary(a => a.AssignmentObjectTypeId, a => a.Name);
        //}


        #endregion
        


    }
}
