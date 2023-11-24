using System;
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
internal class LocalizedDescriptionAttribute : DescriptionAttribute
{
    private readonly string _resourceKey;
        
    private readonly ResourceManager _resourceManager;
        
    //public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
    //{
    //    _resourceManager = new ResourceManager(resourceType.FullName, resourceType.Assembly);
    //    _resourceKey = resourceKey;
    //}

    public LocalizedDescriptionAttribute(string resourceKey, Type resourceType, string resourceFolder)
    {
        _resourceManager = new ResourceManager(resourceFolder + "." + resourceType.Name, resourceType.Assembly);
        _resourceKey = resourceKey;
    }

    public override string Description
    {
        get
        {
            var displayName = _resourceManager.GetString(_resourceKey);
            return string.IsNullOrEmpty(displayName) ? $"[{_resourceKey}]" : displayName;
        }
    }
}