using ToSic.Eav.Data.Sys.Values;

namespace ToSic.Eav.Data.Build;

public static class ContentTypeTestAccessors
{

    public static IContentTypeField ContentTypeFieldTac(this ContentTypeAssemblyKit ctAssemblyKit, int appId, string name, string dataType, bool isTitle = false, int attId = 0, int index = 0)
        => ctAssemblyKit.Field.Create(appId: appId, name: name, type: ValueTypeHelpers.Get(dataType), isTitle: isTitle, id: attId, sortOrder: index);


    public static IContentType CreateContentTypeTac(this ContentTypeAssembler assembler,
        int appId,
        string name,
        int? id = default,
        string nameId = default,
        string scope = default,
        IList<IContentTypeField> attributes = default)
    {
        return assembler.Create(appId: appId,
            id: id ?? 0,
            name: name,
            nameId: nameId,
            scope: scope ?? "TestScope",
            attributes: attributes);
    }

    public static IAttribute CreateTypedAttributeTac(this AttributeAssembler assembler, string name, ValueTypes type, IList<IValue>? values = null)
        => assembler.Create(name, type, values);
}