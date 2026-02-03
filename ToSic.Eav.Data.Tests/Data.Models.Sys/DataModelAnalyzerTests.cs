using ToSic.Eav.Models;
using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.Models.Sys;

public class DataModelAnalyzerTests
{
    private void AssertTypeName<T>(string name)
        where T : class, IWrapperWip =>
        Equal(name, string.Join(",", DataModelAnalyzerTestAccessors.GetContentTypeNamesTac<T>()));

    private void AssertStreamNames<T>(string namesCsv)
        where T : class, IWrapperWip =>
        Equal(namesCsv, string.Join(",", DataModelAnalyzerTestAccessors.GetStreamNameListTac<T>()));

    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    class NotDecorated : IWrapperWip;

    [Fact]
    public void NotDecoratedDataModelType() =>
        AssertTypeName<NotDecorated>(nameof(NotDecorated));

    [Fact]
    public void NotDecoratedDataModelStream() =>
        AssertStreamNames<NotDecorated>(nameof(NotDecorated));

    [Fact]
    public void NotDecoratedDataModelStreamList() =>
        AssertStreamNames<NotDecorated>(nameof(NotDecorated));

    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    class NotDecoratedModel : IWrapperWip;

    [Fact]
    public void NotDecoratedModelStreamList() =>
        AssertStreamNames<NotDecoratedModel>(nameof(NotDecoratedModel) + "," + nameof(NotDecorated));

    // Objects starting with an "I" won't have the "I" removed in the name checks
    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    class INotDecoratedModel : IWrapperWip;

    [Fact]
    public void INotDecoratedModelStreamList() =>
        AssertStreamNames<INotDecoratedModel>(nameof(INotDecoratedModel) + ",INotDecorated");

    // ReSharper disable once ArrangeTypeMemberModifiers
    interface INotDecorated : IWrapperWip;

    [Fact]
    public void INotDecoratedType() =>
        AssertTypeName<INotDecorated>(nameof(INotDecorated) + ',' + nameof(INotDecorated).Substring(1));

    [Fact]
    public void INotDecoratedStream() =>
        AssertStreamNames<INotDecorated>(nameof(INotDecorated) + ",NotDecorated");


    private const string ForContentType1 = "Abc";
    private const string StreamName1 = "AbcStream";
    [ModelSpecs(ContentType = ForContentType1, Stream = StreamName1)]
    // ReSharper disable once ArrangeTypeMemberModifiers
    class Decorated : IWrapperWip;

    [Fact]
    public void DecoratedType() =>
        AssertTypeName<Decorated>(ForContentType1);

    [Fact]
    public void DecoratedStream() =>
        AssertStreamNames<Decorated>(StreamName1);


    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    class InheritDecorated : Decorated;

    [Fact]
    public void InheritDecoratedType() =>
        AssertTypeName<InheritDecorated>(nameof(InheritDecorated));

    [Fact]
    public void InheritDecoratedStream() =>
        AssertStreamNames<InheritDecorated>(nameof(InheritDecorated));


    private const string ForContentTypeReDecorated = "ReDec";
    private const string StreamNameReDecorated = "ReDecStream";
    [ModelSpecs(ContentType = ForContentTypeReDecorated, Stream = StreamNameReDecorated + ",Abc")]
    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    class InheritReDecorated : InheritDecorated;

    [Fact]
    public void InheritReDecoratedType() =>
        AssertTypeName<InheritReDecorated>(ForContentTypeReDecorated);
    [Fact]
    public void InheritReDecoratedStream() =>
        AssertStreamNames<InheritReDecorated>(StreamNameReDecorated + ",Abc");


    private const string ForContentTypeIDecorated = "IDec";
    private const string StreamNameIDecorated= "IRedecStream";
    [ModelSpecs(ContentType = ForContentTypeIDecorated, Stream = StreamNameIDecorated)]
    interface IDecorated : IWrapperWip;

    [Fact]
    public void IDecoratedType() =>
        AssertTypeName<IDecorated>(ForContentTypeIDecorated);

    [Fact]
    public void IDecoratedStream() =>
        AssertStreamNames<IDecorated>(StreamNameIDecorated);

}