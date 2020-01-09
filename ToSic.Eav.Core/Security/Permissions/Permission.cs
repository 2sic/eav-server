﻿using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Security
{
    /// <summary>
    /// Helper object for constants related to the permissions-type
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class Permission: EntityBasedType
    {
        public const string TypeName = "PermissionConfiguration";
        public const string FieldIdentity = "Identity";
        public const string FieldCondition = "Condition";
        public const string FieldGrant = "Grant";
        public const string FieldOwner = "Owner"; // todo: unclear - this probably should be a global field name

        public Permission(IEntity entity) : base(entity)
        {
        }

        /// <summary>
        /// The condition in this permission - like "this rule applies to admins"
        /// The condition is usually a text-code by the hosting CMS
        /// </summary>
        public string Condition => _condition ?? (_condition = Entity.GetBestValue<string>(FieldCondition));
        private string _condition;

        /// <summary>
        /// The identity this rule should apply to 
        /// This is usually a user guid or group-id; exact specs vary based on the hosting CMS
        /// </summary>
        public string Identity => _identity ?? (_identity = Entity.GetBestValue<string>(FieldIdentity));
        private string _identity;

        /// <summary>
        /// The rights granted by this permission, usually a series of characters like r=read, u=update, etc.
        /// </summary>
        public string Grant => _grant ?? (_grant = Entity.GetBestValue<string>(FieldGrant));
        private string _grant;
    }
}
