﻿using System.Collections.Generic;

namespace ToSic.Eav.WebApi.Dto
{
    public class ContentTypeDto: IdNameDto
    {
        public string Label { get; set; }
        public string StaticName { get; set; }
        public string Scope { get; set; }
        public string Description { get; set; }
        public bool UsesSharedDef { get; set; }
        public int? SharedDefId { get; set; }
        public int Items { get; set; }
        public int Fields { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

}