namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// Experimental - base class for any kind of aspect and it's state
    /// </summary>
    public class AspectState<TAspect> where TAspect : AspectDefinition
    {
        public AspectState(TAspect definition, bool isEnabled)
        {
            Definition = definition;
            IsEnabled = isEnabled;
        }

        public TAspect Definition { get; }

        public bool IsEnabled { get; }
    }
}
