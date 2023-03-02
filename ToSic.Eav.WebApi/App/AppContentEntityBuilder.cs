using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Api.Api01;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.WebApi.App
{
    // TODO:
    // - Probably move to EAV
    // - Probably use constants for the switch-cases
    // - probably rename to AppData
    internal class AppContentEntityBuilder: HelperBase
    {
        public AppContentEntityBuilder(ILog parentLog) : base(parentLog, "Api.Bldr")
        {
        }


        /// <summary>
        /// Construct an import-friendly, type-controlled value-dictionary to create or update an entity
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> CreateEntityDictionary(string contentType, Dictionary<string, object> newContentItem, AppState appState)
        {
            Log.A($"create ent dic a#{appState.AppId}, type:{contentType}");
            // Retrieve content-type definition and check all the fields that this content-type has
            //var appState = Eav.Apps.State.Get(appId);
            var listOfTypes = appState.GetContentType(contentType);
            var attribs = listOfTypes.Attributes;

            var cleanedNewItem = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var attrDef in attribs)
            {
                var attrName = attrDef.Name;
                if (!newContentItem.ContainsKey(attrName)) continue;
                var foundValue = newContentItem[attrName];
                switch (attrDef.Type)
                {
                    case ValueTypes.String:
                    case ValueTypes.Hyperlink:
                        if (foundValue is string)
                            cleanedNewItem.Add(attrName, foundValue.ToString());
                        else
                            throw ValueMappingError(attrDef, foundValue);
                        break;
                    case ValueTypes.Boolean:
                        if (bool.TryParse(foundValue.ToString(), out var bolValue))
                            cleanedNewItem.Add(attrName, bolValue);
                        else
                            throw ValueMappingError(attrDef, foundValue);
                        break;
                    case ValueTypes.DateTime:
                        if (DateTime.TryParse(foundValue.ToString(), out var dtm))
                            cleanedNewItem.Add(attrName, dtm);
                        else
                            throw ValueMappingError(attrDef, foundValue);
                        break;
                    case ValueTypes.Number:
                        if (decimal.TryParse(foundValue.ToString(), out var dec))
                            cleanedNewItem.Add(attrName, dec);
                        else
                            throw ValueMappingError(attrDef, foundValue);
                        break;
                    case ValueTypes.Entity:
                        var relationships = new List<int>();

                        if (foundValue is IEnumerable foundEnum) // it's a list!
                            foreach (var item in foundEnum)
                                relationships.Add(CreateSingleRelationshipItem(item));
                        else // not a list
                            relationships.Add(CreateSingleRelationshipItem(foundValue));

                        cleanedNewItem.Add(attrName, relationships);

                        break;
                    default:
                        throw new Exception(
                            $"Tried to create attribute '{attrName}' but the type is not known: '{attrDef.Type}'");
                }

                // todo: maybe one day get default-values and insert them if not supplied by JS
            }

            AddPublishState(newContentItem, cleanedNewItem);

            return cleanedNewItem;
        }

        // add "PublishState" in "values" (before it can be removed when there is no "PublishState" attribute)
        private void AddPublishState(IDictionary<string, object> values, IDictionary<string, object> cleaned)
        {
            if (!values.ContainsKey(SaveApiAttributes.SavePublishingState)) return;
            cleaned.Add(SaveApiAttributes.SavePublishingState, values[SaveApiAttributes.SavePublishingState]);
        }

        /// <summary>
        /// In case of an error, show a nicer, consistent message
        /// </summary>
        /// <param name="attributeDefinition"></param>
        /// <param name="foundValue"></param>
        private static Exception ValueMappingError(IAttributeBase attributeDefinition, object foundValue)
            => new Exception(
                $"Tried to create {attributeDefinition.Name} and couldn't convert to correct {attributeDefinition.Type}: '{foundValue}'");

        /// <summary>
        /// Takes input from JSON which could be in many formats like Category=ID or Category={id=#} 
        /// and then converts it to an item in the relationships-list
        /// </summary>
        /// <param name="foundValue"></param>
        private int CreateSingleRelationshipItem(object foundValue)
        {
            Log.A("create relationship");
            try
            {
                // the object foundNumber is either just an Id, or an Id/Title combination
                // Try to see if it's already a number, else check if it's a JSON property
                if (!int.TryParse(foundValue.ToString(), out var foundNumber))
                {
                    switch (foundValue)
                    {
                        case JsonElement jn when jn.ValueKind == JsonValueKind.Number:
                            foundNumber = jn.GetInt32();
                            break;
                        case JsonElement jo when jo.ValueKind == JsonValueKind.Object:
                        {
                            if (jo.TryGetProperty("Id", out var foundId))
                                foundNumber = foundId.GetInt32();
                            else if (jo.TryGetProperty("id", out foundId))
                                foundNumber = foundId.GetInt32();
                            break;
                        }
                    }
                }
                Log.A($"relationship found:{foundNumber}");
                return foundNumber;
            }
            catch
            {
                throw new Exception("Tried to find Id of a relationship - but only found " + foundValue);
            }
        }
    }
}
