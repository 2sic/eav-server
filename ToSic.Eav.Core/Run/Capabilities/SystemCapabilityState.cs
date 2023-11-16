//using System.Collections.Generic;
//using ToSic.Eav.Configuration;
//using ToSic.Eav.Data.Raw;
//using ToSic.Lib.Helpers;

//namespace ToSic.Eav.Run.Capabilities
//{
//    public class SystemCapabilityState: AspectState<SystemCapabilityDefinition>, IHasRawEntity<IRawEntity>
//    {
//        public SystemCapabilityState(SystemCapabilityDefinition definition, bool isEnabled) : base(definition, isEnabled)
//        {
//        }

//        public IRawEntity RawEntity => _newEntity.Get(() => new RawEntity
//        {
//            Guid = Definition.Guid,
//            Values = new Dictionary<string, object>(Definition.RawEntity.Attributes(null))
//            {
//                [nameof(IsEnabled)] = IsEnabled,
//            }
//        });

//        private readonly GetOnce<IRawEntity> _newEntity = new GetOnce<IRawEntity>();
//    }
//}
