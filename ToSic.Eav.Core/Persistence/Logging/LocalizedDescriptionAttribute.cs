using System.ComponentModel;
using System.Resources;

namespace ToSic.Eav.Persistence.Logging;

/// <summary>
/// Attribute to describe and localize enumeration values.
/// 
/// Example:
/// public enum MyEnum
/// {
///     [LocalizedDescription("MyValue1Resource", typeof(MyEnum)]
///     MyValue1
/// }
/// </summary>
internal class LocalizedDescriptionAttribute(string resourceKey, Type resourceType, string resourceFolder)
    : DescriptionAttribute
{
    private readonly ResourceManager _resourceManager = new(resourceFolder + "." + resourceType.Name, resourceType.Assembly);
        
    //public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
    //{
    //    _resourceManager = new ResourceManager(resourceType.FullName, resourceType.Assembly);
    //    _resourceKey = resourceKey;
    //}

    public override string Description
    {
        get
        {
            var displayName = _resourceManager.GetString(resourceKey);
            return string.IsNullOrEmpty(displayName) ? $"[{resourceKey}]" : displayName;
        }
    }
}