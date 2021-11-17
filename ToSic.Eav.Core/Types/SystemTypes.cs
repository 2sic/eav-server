using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.Types
{
    public class SystemTypes
    {
        // Obsolete Decorator
        public static string IsObsoleteDecoratorId = "852fe15e-bf23-44e6-a856-0a130203496c";
        public static Guid IsObsoleteDecoratorGuid = new Guid(IsObsoleteDecoratorId);

        // Priority Decorator
        public static string PriorityDecoratorId = "1291dbc0-9c52-4c79-8c95-698e5c3aa299";
        public static Guid PriorityDecoratorGuid = new Guid(PriorityDecoratorId);
    }
}
