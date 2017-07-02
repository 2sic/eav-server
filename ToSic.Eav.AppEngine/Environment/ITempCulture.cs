namespace ToSic.SexyContent.Environment.Interfaces
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
