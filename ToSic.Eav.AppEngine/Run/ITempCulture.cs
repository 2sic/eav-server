﻿// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    // todo: sometime replace this with DimensionDefinition
    public interface ITempCulture
    {

         string Key { get;  }
         string Text { get;  }
         bool Active { get;  }
         //bool AllowStateChange { get;  }
    }
}
