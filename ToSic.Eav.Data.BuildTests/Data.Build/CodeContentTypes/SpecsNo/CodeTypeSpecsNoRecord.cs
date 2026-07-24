namespace ToSic.Eav.Data.Build.CodeContentTypes.SpecsNo;

public record CodeTypeSpecsNoRecord(
    int Id,
    string? Name,
    int Age,
    DateTime BirthDate,
    bool IsAlive
);