using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using ToSic.Eav.Repository.Efc.Sys.DbEntities;

namespace ToSic.Eav.Repository.Efc.Tests.SaveData;

public class ExistingDraftSaveGuardTests
{
    private static readonly Guid Guid = new("1b51ad3e-d197-4c3a-8825-10d14957bd41");
    private static ExistingDraftSaveCondition Expected => new(4, 10, Guid, 7, 20, 9);
    private static TsDynDataEntity Published() => new() { AppId = 4, EntityId = 10, EntityGuid = Guid, Version = 7, IsPublished = true, ContentTypeId = 3, Owner = "" };
    private static TsDynDataEntity Draft() => new() { AppId = 4, EntityId = 20, EntityGuid = Guid, Version = 9, PublishedEntityId = 10, ContentTypeId = 3, Owner = "" };

    [Fact] public void Matches_AcceptsExactPair_InEitherRowOrder()
    {
        Assert.True(ExistingDraftSaveGuard.Matches(Expected, [Published(), Draft()]));
        Assert.True(ExistingDraftSaveGuard.Matches(Expected, [Draft(), Published()]));
    }

    [Theory]
    [InlineData("published-version")]
    [InlineData("draft-version")]
    [InlineData("binding")]
    [InlineData("deleted")]
    [InlineData("app")]
    [InlineData("guid")]
    [InlineData("published")]
    [InlineData("type")]
    public void Matches_RejectsChangedIdentityOrRevision(string change)
    {
        var published = Published();
        var draft = Draft();
        switch (change)
        {
            case "published-version": published.Version++; break;
            case "draft-version": draft.Version++; break;
            case "binding": draft.PublishedEntityId = 11; break;
            case "deleted": draft.TransDeletedId = 1; break;
            case "app": draft.AppId = 5; break;
            case "guid": draft.EntityGuid = System.Guid.NewGuid(); break;
            case "published": draft.IsPublished = true; break;
            case "type": draft.ContentTypeId = 5; break;
        }
        Assert.False(ExistingDraftSaveGuard.Matches(Expected, [published, draft]));
    }

    [Fact] public void Matches_RejectsMissingOrExtraRows()
    {
        Assert.False(ExistingDraftSaveGuard.Matches(Expected, [Published()]));
        Assert.False(ExistingDraftSaveGuard.Matches(Expected, [Published(), Draft(), Draft()]));
    }

    [Fact] public void UnconditionalSave_DoesNotInspectTransactionOrStorage()
    {
        var options = new SaveOptions { Languages = [], PrimaryLanguage = "en-us" };
        var pairs = new[] { new EntityPair<SaveOptions>(null!, options) };

        ExistingDraftSaveGuard.RequireOwnedTransaction(null!, pairs);
        ExistingDraftSaveGuard.Demand(null!, pairs);
    }

    [Fact] public void ConditionalSave_RejectsAmbientTransaction_BeforeAccessingStorage()
    {
        var options = new SaveOptions { Languages = [], PrimaryLanguage = "en-us", ExistingDraftCondition = Expected };
        var pairs = new[] { new EntityPair<SaveOptions>(null!, options) };
        using var transaction = new System.Transactions.TransactionScope();

        Assert.Throws<NotSupportedException>(() => ExistingDraftSaveGuard.RequireOwnedTransaction(null!, pairs));
    }
}
