namespace ToSic.Eav
{
    static class State
    {

        #region Metadata & Assignment-Types
        ///// <summary>
        ///// Retrieve the Assignment-Type-ID which is used to determine which type of object
        ///// an entity is assigned to (because just the object ID would be ambiguous)
        ///// </summary>
        ///// <param name="typeName"></param>
        ///// <returns></returns>
        //public static int GetAssignmentTypeId(string typeName)
        //    => DataSource.GetCache(null).GetAssignmentObjectTypeId(typeName);


        //public static void MetadataEnsureTypeAndSingleEntity(int zoneId, int appId, string scope, string setName,
        //    string label, int appAssignment, OrderedDictionary values)
        //{
        //    var eavContext = EavDataController.Instance(zoneId, appId);

        //    var contentType = !eavContext.AttribSet.AttributeSetExists(setName, eavContext.AppId)
        //        ? eavContext.AttribSet.AddContentTypeAndSave(setName, label, setName, scope)
        //        : eavContext.AttribSet.GetAttributeSet(setName);

        //    if (values == null)
        //        values = new OrderedDictionary();

        //    eavContext.Entities.AddEntity(contentType, values, null, eavContext.AppId, appAssignment);

        //    Purge(eavContext.ZoneId, eavContext.ZoneId);
        //}


        #endregion

        //#region purge cache stuff
        //public static void Purge(int zoneId, int appId, bool global = false)
        //{
        //    if (global)
        //        DataSource.GetCache(null).PurgeGlobalCache();
        //    else
        //        DataSource.GetCache(null).PurgeCache(zoneId, appId);
        //}

        //public static void DoAndPurge(int zoneId, int appId, Action action, bool global=false)
        //{
        //    action.Invoke();
        //    Purge(zoneId, appId, global);
        //}
        //#endregion
        
            

        #region ContentType Commands

        //public static IEnumerable<IContentType> ContentTypes(int zoneId, int appId, string scope = null, bool includeAttributeTypes = false)
        //{
        //    var contentTypes = ((BaseCache)DataSource.GetCache(zoneId, appId)).GetContentTypes();
        //    var set = contentTypes.Select(c => c.Value)
        //        .Where(c => includeAttributeTypes || !c.Name.StartsWith("@"));
        //    if(scope != null)
        //        set = set.Where(p => p.Scope == scope);
        //    return set.OrderBy(c => c.Name); 
        //}
        #endregion




        }
}
