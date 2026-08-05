namespace ToSic.Eav.Models;

// Record equality already works for the ModelFromEntity
// because it's a record, automatically testing for the properties - which will match
// So this file is not necessary, but we added it anyhow
// because it would be our common pattern
// and without the file, we may end up wasting time, trying to re-implement it (already happened).
//
// There are also unit tests, proving that this works

partial record ModelFromEntity; //: IMultiWrapper<IEntity> //, IEquatable<IEntity>
