namespace ToSic.Eav.Data.Publishing.Sys;

internal class PublishingServiceUnknown(WarnUseOfUnknown<PublishingServiceUnknown> _)
    : ServiceBase($"{LogScopes.NotImplemented}.Publsh"), IDataPublishingService
{
    public void DoInsidePublishing(Action<PublishingOptions> action)
    {
        var l = Log.Fn();
        var versioningActionInfo = new PublishingOptions { ForceDraft = false };
        action.Invoke(versioningActionInfo);
        l.Done();
    }
}
