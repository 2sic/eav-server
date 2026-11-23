//namespace ToSic.Sys.DI;

///// <summary>
///// Experimental special replacement for IServiceProvider
///// </summary>
///// <param name="inner"></param>
//public sealed class ContextualServiceProvider(IServiceProvider inner) : ISysServiceProvider, IServiceProvider
//{
//    private readonly ExecutionContext? _capturedContext = ExecutionContext.Capture();
//    private readonly bool _capturedHasOverrides = AmbientOverrideTracker.HasActiveOverrides;

//    private static readonly ContextCallback ResolveCallback = static s =>
//    {
//        var state = (State)s!;
//        state.Result = state.Provider.GetService(state.ServiceType);
//    };

//    public object? GetService(Type serviceType)
//    {
//        if (serviceType == typeof(IServiceProvider))
//            return this;

//        if (AmbientOverrideTracker.HasActiveOverrides || !_capturedHasOverrides || _capturedContext == null)
//            return inner.GetService(serviceType);

//        var state = new State(inner, serviceType);
//        ExecutionContext.Run(_capturedContext.CreateCopy(), ResolveCallback, state);
//        return state.Result;
//    }

//    private sealed class State(IServiceProvider provider, Type serviceType)
//    {
//        public IServiceProvider Provider { get; } = provider;
//        public Type ServiceType { get; } = serviceType;
//        public object? Result { get; set; }
//    }
//}