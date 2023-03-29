using System;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data.Raw
{
    public class RawFromAnonymous: RawEntity
    {
        public RawFromAnonymous(object original)
        {
            var dic = original.ObjectToDictionary(mutable: true, caseInsensitive: true);
                // ReSharper disable VirtualMemberCallInConstructor
            if (dic.ContainsKey(nameof(Id)))
            {
                Id = dic[nameof(Id)].ConvertOrDefault<int>();
                dic.Remove(nameof(Id));
            }

            if (dic.ContainsKey(nameof(Guid)))
            {
                Guid = dic[nameof(Guid)].ConvertOrDefault<Guid>();
                dic.Remove(nameof(Guid));
            }

            if (dic.ContainsKey(nameof(Created)))
            {
                Created = dic[nameof(Created)].ConvertOrDefault<DateTime>();
                dic.Remove(nameof(Created));
            }

            if (dic.ContainsKey(nameof(Modified)))
            {
                Modified = dic[nameof(Modified)].ConvertOrDefault<DateTime>();
                dic.Remove(nameof(Modified));
            }
                // ReSharper restore VirtualMemberCallInConstructor

            Values = dic;
        }

        
    }
}
