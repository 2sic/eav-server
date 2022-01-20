// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    public interface ISiteLanguageState
    {
        string Code { get;  }
        string Culture { get;  }
        bool IsEnabled { get;  }
    }
}
