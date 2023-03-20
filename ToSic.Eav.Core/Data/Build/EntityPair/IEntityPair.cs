using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Process
{
    /// <summary>
    /// Interface marking <see cref="EntityPair{TPartner}"/>s.
    /// This is for internal use, to ensure covariance.
    /// </summary>
    /// <typeparam name="TPartner"></typeparam>
    /// <remarks>
    /// Added in 15.04
    /// </remarks>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface IEntityPair<out TPartner>: ICanBeEntity
    {
        TPartner Partner { get; }
    }
}
