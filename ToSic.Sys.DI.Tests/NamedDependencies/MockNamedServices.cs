namespace ToSic.NamedDependencies;

public interface IMockNamedService
{
    string NameId { get; }
}

internal class MockNamedServiceAbc : IMockNamedService
{
    public const string NameIdConst = "Abc";
    public const string NameIdRegister = NameIdConst;
    public string NameId => NameIdConst;
}


internal class MockNamedServiceDef : IMockNamedService
{
    public const string NameIdConst = "Def";
    public const string NameIdRegister = NameIdConst;
    public string NameId => NameIdConst;
}

internal class MockNamedServiceMultiple : IMockNamedService
{
    public const string NameIdConst = "Ghi";
    public const string NameIdRegister = NameIdConst;
    public string NameId => NameIdConst;
}

internal class MockNamedServiceMultipleSecond : IMockNamedService
{
    public const string NameIdConst = "GhiSecond";
    public const string NameIdRegister = MockNamedServiceMultiple.NameIdRegister;
    public string NameId => NameIdConst;
}