using System.Collections.Immutable;

namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// The internal system to manipulate value lists.
/// </summary>
/// <remarks>
/// This is used in the builders to create the values for the attributes.
/// It can also be used by external code to create values, but it's not really meant for that, so it's not public API.
/// It's more of an internal helper class, which is why it's not in the Sys namespace.
///
/// Important: everything is **functional** meaning that object given in will never be modified.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class ValueListAssembler
{
    public IImmutableList<IValue> Replace(IEnumerable<IValue> values, IValue? oldValueOrNullToAdd, IValue newValue)
    {
        var editable = values.ToListOpt();
        // note: should preserve order
        var index = oldValueOrNullToAdd == null
            ? -1
            : editable.IndexOf(oldValueOrNullToAdd);
        if (index == -1)
            return editable.Append(newValue).ToImmutableSafe();
  
        editable[index] = newValue;
        return editable.ToImmutableSafe();
    }

}