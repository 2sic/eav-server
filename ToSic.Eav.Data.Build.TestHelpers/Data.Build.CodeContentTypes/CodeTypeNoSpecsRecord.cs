namespace ToSic.Eav.Data.Build.CodeContentTypes;

public record CodeTypeNoSpecsRecord(
    int Id,
    string? Name,
    int Age,
    DateTime BirthDate,
    bool IsAlive
);