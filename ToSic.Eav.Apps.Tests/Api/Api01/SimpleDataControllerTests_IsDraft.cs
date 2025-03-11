using ToSic.Eav.Apps.Internal.Api01;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Tests.Api.Api01;

[TestClass]
// ReSharper disable once InconsistentNaming
public class SimpleDataControllerTests_IsDraft
{
    private static (bool ShouldPublish, bool ShouldBranchDrafts) TestGetPublishSpecs(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var r = SimpleDataEditService.GetPublishSpecs(publishedState, existingIsPublished, writePublishAllowed,
            new Log("test"));
        return (r.ShouldPublish, r.ShouldBranchDrafts);
    }

    /// <summary>
    /// Scenarios when creating new.
    /// 1.	No published state – result should be default,
    /// so depending on user permissions it's published or draft.
    /// This should be the same as previous implementation. 
    /// </summary>
    /// <param name="publishedState"></param>
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("null")]
    [DataRow("NULL")]
    [DataRow("NUll")]
    public void New_NoPublishedState(object publishedState)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState, 
            existingIsPublished: null,
            writePublishAllowed: true);

        IsTrue(published); // true is default
        IsFalse(branch); // false because is it new one, so no branch
    }

    /// <summary>
    /// Scenarios when creating new.
    /// 1.	No published state – result should be default,
    /// so depending on user permissions it's published or draft.
    /// This should be the same as previous implementation. 
    /// </summary>
    /// <param name="publishedState"></param>
    [TestMethod]
    [DataRow(null)]
    public void New_NoPublishedState_WritePublishNotAllowed(object publishedState)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: null,
            writePublishAllowed: false);

        IsFalse(published); // false because publish is not allowed
        IsFalse(branch); // false because is it new one, so no branch
    }

    /// <summary>
    /// Scenarios when creating new.
    /// 2.	"true", 1 etc. – should be published, assuming user permissions allow this.
    /// Basically it should not set the published for saving at all, as the default is true,
    /// and permissions may not work if you set it.
    /// </summary>
    /// <param name="publishedState"></param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(1)]
    [DataRow("true")]
    [DataRow("TRUE")]
    [DataRow("TRue")]
    public void New_True(object publishedState)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: null,
            writePublishAllowed: true);

        IsTrue(published); // true (and publish is allowed)
        IsFalse(branch); // false because is it new one, so no branch
    }

    /// <summary>
    /// Scenarios when creating new.
    /// 2.	"true", 1 etc. – should be published, assuming user permissions allow this.
    /// Basically it should not set the published for saving at all, as the default is true,
    /// and permissions may not work if you set it.
    /// </summary>
    /// <param name="publishedState"></param>
    [TestMethod]
    [DataRow(true)]
    public void New_True_WritePublishNotAllowed(object publishedState)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: null,
            writePublishAllowed: false);

        IsFalse(published); // false because publish is not allowed
        IsFalse(branch); // false because is it new one, so no branch
    }

    /// <summary>
    /// Scenarios when creating new.
    /// 3.	False, 0, etc. – should not be published
    /// </summary>
    /// <param name="publishedState"></param>
    [TestMethod]
    [DataRow(false)]
    [DataRow(0)]
    [DataRow("false")]
    [DataRow("FALSE")]
    [DataRow("FAlse")]
    public void New_False(object publishedState)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: null,
            writePublishAllowed: true);

        IsFalse(published); // false and publish is allowed
        IsFalse(branch); // false because is it new one, so no branch
    }

    /// <summary>
    /// Scenarios when creating new.
    /// 3.	False, 0, etc. – should not be published
    /// </summary>
    /// <param name="publishedState"></param>
    [TestMethod]
    [DataRow(false)]
    public void New_False_WritePublishNotAllowed(object publishedState)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: null,
            writePublishAllowed: false);

        IsFalse(published); // false because publish is not allowed
        IsFalse(branch); // false because is it new one, so no branch
    }

    /// <summary>
    /// Existing data which is published
    /// 1.	No published state – unchanged, shouldn't set at all, because if user permisisons restrict to draft-only,
    /// that should happen automatically. Result
    /// a.	If user is allowed to publish, it's published
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(null, true, true)]
    public void ExistingPublished_NoPublishedState(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsTrue(published); // true because existing is published and save publish is allowed
        IsFalse(branch); // false because is it allowed to save publish
    }

    /// <summary>
    /// Existing data which is published
    /// 1.	No published state – unchanged, shouldn't set at all, because if user permisisons restrict to draft-only,
    /// that should happen automatically. Result
    /// b.	If user is draft-only, it's should draft/fork
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(null, true, false)]
    public void ExistingPublished_NoPublishedState_WritePublishNotAllowed(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsFalse(published); // false because save publish is not allowed
        IsTrue(branch); // true because save publish is not allowed
    }

    /// <summary>
    /// Existing data which is published
    /// 2.	True, 1, etc. – unchanged, shouldn't set at all, again because of user permissions. Result
    /// a.	If user is allowed to publish, it's published
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(true, true, true)]
    public void ExistingPublished_True(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsTrue(published); // true (and save publish is allowed)
        IsFalse(branch); // false because save publish is allowed
    }

    /// <summary>
    /// Existing data which is published
    /// 2.	True, 1, etc. – unchanged, shouldn't set at all, again because of user permissions. Result
    /// b.	If user is draft-only, it's should draft/fork
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(true, true, false)]
    public void ExistingPublished_True_WritePublishNotAllowed(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsFalse(published); // false because save publish is not allowed
        IsTrue(branch); // true because save publish is not allowed
    }

    /// <summary>
    /// Existing data which is published
    /// 3.	False: depending on user permissions. Result
    /// b.	If user is allowed to change published (full write permissions), then this should result in no publish, no branch
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(false, true, true)]
    public void ExistingPublished_False(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsFalse(published); // false (also save publish is allowed)
        IsFalse(branch); // false because save publish is allowed
    }

    /// <summary>
    /// Existing data which is published
    /// 3.	False: depending on user permissions. Result
    /// a.	If user can only create draft, then it should branch, as the user is not allowed to affect published
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(false, true, false)]
    public void ExistingPublished_False_WritePublishNotAllowed(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsFalse(published); // false because save publish is not allowed
        IsTrue(branch); // true because save publish is not allowed
    }

    /// <summary>
    /// Existing data which is published
    /// 4.	"draft" – result always draft and no published
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow("draft", true, true)]
    public void ExistingPublished_Draft(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsFalse(published); // false (even that publish is allowed)
        IsTrue(branch); // true because result always draft
    }

    /// <summary>
    /// Existing data which is published
    /// 4.	"draft" – result always draft and no published
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow("draft", true, false)]
    public void ExistingPublish_Draft_WritePublishNotAllowed(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsFalse(published); // false because save publish is not allowed
        IsTrue(branch); // true because save publish is not allowed
    }

    /// <summary>
    /// Existing data which is draft ONLY
    /// 1.	No state: unchanged, should remain draft
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(null, false, true)]
    [DataRow(null, false, false)]
    public void ExistingDraft_NoPublishedState(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsFalse(published); // false, because it is draft
        IsFalse(branch); // false (it is already draft)
    }

    /// <summary>
    /// Existing data which is draft ONLY
    /// 2.	True, 1, etc. – should save changes and publish
    /// b.	If user is allowed to save published, then publish should happen
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(true, false, true)]
    public void ExistingDraft_True(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsTrue(published); // true because it is allowed to save publish
        IsFalse(branch); // false because it is allowed to save publish
    }

    /// <summary>
    /// Existing data which is draft ONLY
    /// 2.	True, 1, etc. – should save changes and publish
    /// a.	If user is only allowed to create drafts, then publishing should not happen, draft only
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(true, false, false)]
    public void ExistingDraft_True_WritePublishNotAllowed(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsFalse(published); // false because it is not allowed to save publish
        IsFalse(branch); // false because it already a draft
    }

    /// <summary>
    /// Existing data which is draft ONLY
    /// 3.	False, 0, etc. – Result always draft and no published
    ///  </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(false, false, true)]
    [DataRow(false, false, false)]
    public void ExistingDraft_False(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsFalse(published); // false because no publish
        IsFalse(branch); // false because it is already draft
    }

    /// <summary>
    /// Existing data which is draft ONLY
    /// 4.	"draft" – result always draft and no published
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow("DRAFT", false, true)]
    [DataRow("DRaft", false, false)]
    public void ExistingDraft_Draft(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        IsFalse(published); // false because no publish
        IsFalse(branch); // false because it is already draft
    }

    /// <summary>
    /// Existing Data which is draft and published
    /// 1.	No state: unchanged, the updated data should only be in the draft
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(null, false, true)]
    [DataRow(null, false, false)]
    public void ExistingDraftAndPublish_NoPublishedState(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        // the updated data should only be in the draft
        IsFalse(published); // false, because it is draft
        IsFalse(branch); // false (it is already draft)
    }

    /// <summary>
    /// Existing Data which is draft and published
    /// 2.	True: Result 
    /// a.	If user may write published, then all is now published
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(true, false, true)]
    public void ExistingDraftAndPublish_True(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        // draft become published
        IsTrue(published); // true because it is allowed to save publish
        IsFalse(branch); // false because it is allowed to save publish
    }

    /// <summary>
    /// Existing Data which is draft and published
    /// 2.	True: Result 
    /// b.	If user may not do that, it's ignored, draft is updated only
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(true, false, false)]
    public void ExistingDraftAndPublish_True_WritePublishNotAllowed(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);
            
        // draft is updated only
        IsFalse(published); // false because it is not allowed to save publish
        IsFalse(branch); // false because it is already a draft
    }

    /// <summary>
    /// Existing Data which is draft and published
    /// 3.	False: Result based on permissions
    /// a.	User may change published: all is now unpublished
    /// b.	User may not change published: ignore; update the draft only
    ///  </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow(false, false, true)]
    [DataRow(false, false, false)]
    public void ExistingDraftAndPublish_False(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);
            
        // publish is unpublished (or draft update only)
        IsFalse(published); // false because no publish
        IsFalse(branch); // false because it is already draft
    }

    /// <summary>
    /// Existing Data which is draft and published
    /// 4.	"draft" Result always draft and published, only draft was updated
    /// </summary>
    /// <param name="publishedState"></param>
    /// <param name="existingIsPublished"></param>
    /// <param name="writePublishAllowed"></param>
    [TestMethod]
    [DataRow("DRAFT", false, true)]
    [DataRow("DRaft", false, false)]
    public void ExistingDraftAndPublish_Draft(object publishedState, bool? existingIsPublished, bool writePublishAllowed)
    {
        var (published, branch) = TestGetPublishSpecs(
            publishedState: publishedState,
            existingIsPublished: existingIsPublished,
            writePublishAllowed: writePublishAllowed);

        // only draft was updated
        IsFalse(published); // false because no publish
        IsFalse(branch); // false because it is already draft
    }
}