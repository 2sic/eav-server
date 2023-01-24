﻿using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Decorators
{
    internal class MetadataForDecorator: ForExpectedBase
    {
        public static string ContentTypeNameId = "4c88d78f-5f3e-4b66-95f2-6d63b7858847";
        public static string ContentTypeName = "MetadataForDecorator";

        public MetadataForDecorator(IEntity entity) : base(entity) { }

        /// <summary>
        /// An optional name to specify more exactly what it is.
        /// For example if this targets Entities, this would specify what type
        /// </summary>
        public string TargetName => GetThis("");

    }
}