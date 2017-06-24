using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ToSic.Eav.WebApi.Formats
{
    public class EntityWithLanguages
    {
        public int Id;
        [NonSerialized] public Guid Guid;

        //[NonSerialized] public int RepoId;

        public Type Type;
        public bool IsPublished;
        public bool IsBranch;
        public string TitleAttributeName;
        public Dictionary<string, Attribute> Attributes;
        public int AppId;
    }

    public class Attribute
    {
        // public string Key;
        public ValueSet[] Values;
    }

    public class ValueSet
    {
        public object Value;
        public Dictionary<string, bool> Dimensions;
    }

    public class Type
    {
        public string Name;
        public string StaticName;
    }

    public class Metadata
    {
        /// <summary>
        /// Will return true if a target-type was assigned
        /// </summary>
        public bool HasMetadata => _targetType != Constants.NotMetadata;

        private int _targetType = Constants.NotMetadata;
        /// <summary>
        /// This is the AssignmentObjectTypeId - usually 1 (none), 2 (attribute), 4 (entity)
        /// </summary>
        public int TargetType {
            get
            {
                return _targetType;
            }
            set
            {
                if(value > 10)
                    throw new Exception("Unexpected TargetType (AssigmentObjectTypeId) - currently only expecting 1-4, maybe a bit more, got way more");
                _targetType = value;
            }
        }


        private string _keyType;

        public string KeyType
        {
            get
            {
                return _keyType;
            } 
            set
            {
                var newType = value.ToLower();
                if (newType == "string" || newType == "number" || newType == "guid")
                    _keyType = newType;
                else
                    throw new Exception("metadata keytype unknown. expected number, guid or string" );
            } 
        }

        public string Key { get; set; }

        /// <summary>
        /// The Keynumber is null or the int of the key as stored in "Key"
        /// </summary>
        public int? KeyNumber
        {
            get
            {
                if (KeyType != "number")
                    return null;
                int keyNum;
                if (!int.TryParse(Key, out keyNum))
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "Tried to retrieve a number-key for the meadata but conversion failed" });
                return keyNum;
            }
        }

        /// <summary>
        /// The KeyGuid is null or the guid of the key as stored in "Key"
        /// </summary>
        public Guid? KeyGuid
        {
            get
            {
                if (KeyType != "guid")
                    return null;
                Guid keyGuid;
                if (!Guid.TryParse(Key, out keyGuid))
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "Tried to retrieve a guid-key for the meadata but conversion failed" });
                return keyGuid;
            }
        }

        /// <summary>
        /// The KeyString is null or the string of the key as stored in "Key"
        /// </summary>
        public string KeyString => KeyType != "string" ? null : Key;
    }
}
