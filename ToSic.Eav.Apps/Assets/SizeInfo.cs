using System;
using ToSic.Eav.Documentation;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Apps.Assets
{
    /// <summary>
    /// Size information for files
    /// </summary>
    /// <remarks>
    /// Added in v14.04
    /// </remarks>
    [PublicApi]
    public class SizeInfo
    {
        private const int Factor = 1024;

        public SizeInfo(int bytes) => Bytes = bytes;

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Bytes { get; }

        /// <summary>
        /// Size in KB
        /// </summary>
        /// <returns></returns>
        public decimal Kb => Rounded((decimal)Bytes / Factor);

        /// <summary>
        /// Size in MB
        /// </summary>
        /// <returns></returns>
        public decimal Mb => Rounded((decimal)Bytes / Factor / Factor);

        /// <summary>
        /// Size in GB
        /// </summary>
        /// <returns></returns>
        public decimal Gb => Rounded((decimal)Bytes / Factor / Factor / Factor);

        /// <summary>
        /// Best size based on the number. Will be in KB, MB or GB.
        /// The unit is found on BestUnit
        /// </summary>
        /// <returns></returns>
        public decimal BestSize => Rounded(BestSizeCache.Size);

        /// <summary>
        /// Best unit to use based on the effective size. 
        /// </summary>
        /// <returns></returns>
        public string BestUnit => BestSizeCache.Unit;

        /// <summary>
        /// Trunc/rounding factor used on the numbers.
        /// If you change it, the precision of the numbers returned would change.
        /// we don't plan to publish this, as the web designer must usually to a ToString(#,##) for it to look right anyhow
        /// </summary>
        private int Decimals { get; set; } = 4;

        private decimal Rounded(decimal number) => Math.Round(number, Decimals);


        private (decimal Size, string Unit) BestSizeCache => _bestSizeCache.Get(() =>
        {
            if (Bytes < Factor * Factor) return (Kb, "KB");
            if (Bytes < Factor * Factor * Factor) return (Mb, "MB");
            return (Gb, "GB");
        });
        private readonly GetOnce<(decimal, string)> _bestSizeCache = new GetOnce<(decimal, string)>();
    }
}
