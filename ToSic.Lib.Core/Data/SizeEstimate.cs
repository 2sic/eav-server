namespace ToSic.Lib.Data;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public record SizeEstimate(int Known = 0, int Estimated = 0, bool Unknown = false, bool Error = false)
{
    public int Total => Known + Estimated;

    public string Icon => Error
        ? "⚠️"
        : Unknown || Known == 0
            ? "❔"
            : "✅";

    public static SizeEstimate operator +(SizeEstimate a, SizeEstimate b) => new(
        a.Known + b.Known,
        a.Estimated + b.Estimated,
        Unknown: a.Unknown || b.Unknown,
        Error: a.Error || b.Error
    );

    public static SizeEstimate operator -(SizeEstimate a, SizeEstimate b) => new(
        a.Known - b.Known,
        a.Estimated - b.Estimated,
        Unknown: a.Unknown || b.Unknown,
        Error: a.Error || b.Error
    );
}