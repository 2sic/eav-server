using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Linq;

namespace ToSic.Eav.BLL.Parts
{
    public class DbShortcuts: BllCommandBase
    {
        public DbShortcuts(EavDataController c) : base(c) { }

        #region helpers
        /// <summary>
        /// Clone an Entity in Entity Framework 4
        /// </summary>
        /// <remarks>Source: http://www.codeproject.com/Tips/474296/Clone-an-Entity-in-Entity-Framework </remarks>
        public T CopyEfEntity<T>(T entity) where T : EntityObject
        {
            var copyKeys = false;
            var clone =  Context.SqlDb.CreateObject<T>();
            var pis = entity.GetType().GetProperties();

            foreach (var pi in from pi in pis let attrs = (EdmScalarPropertyAttribute[])pi.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false) from attr in attrs where copyKeys || !attr.EntityKeyProperty select pi)
                pi.SetValue(clone, pi.GetValue(entity, null), null);

            return clone;
        }
        #endregion
        

        #region Assignment Object Types
        /// <summary>
        /// AssignmentObjectType with specified Name 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AssignmentObjectType GetAssignmentObjectType(string name)
        {
            return Context.SqlDb.AssignmentObjectTypes.Single(a => a.Name == name);
        }

        /// <summary>
        /// Get all AssignmentObjectTypes with Id and Name
        /// </summary>
        public Dictionary<int, string> GetAssignmentObjectTypes()
        {
            return (from a in Context.SqlDb.AssignmentObjectTypes
                    select new { a.AssignmentObjectTypeID, a.Name }).ToDictionary(a => a.AssignmentObjectTypeID, a => a.Name);
        }


        #endregion
        


    }
}
