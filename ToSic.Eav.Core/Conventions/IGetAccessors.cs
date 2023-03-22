namespace ToSic.Eav.Conventions
{
    public interface IGetAccessors<out TCoreType>
    {
        TCoreType Get(string name);

        TValue Get<TValue>(string name);

        // ReSharper disable once MethodOverloadWithOptionalParameter
        TValue Get<TValue>(string name, string noParamOrder = Eav.Parameters.Protector, TValue fallback = default);
    }
}
