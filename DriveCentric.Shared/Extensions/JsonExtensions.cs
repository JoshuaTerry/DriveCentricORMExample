using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DriveCentric.Shared.Extensions
{
    public static class JsonExtensions
    {
        public static KeyValuePair<string, object> ConvertToType<T1>(KeyValuePair<string, JToken> pair)
        {
            var returnValue = new KeyValuePair<string, object>();

            var type = typeof(T1);
            var property = type.GetProperty(pair.Key);

            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (pair.Value.Type == JTokenType.String &&
                    Nullable.GetUnderlyingType(property.PropertyType) == typeof(Guid) &&
                    !string.IsNullOrWhiteSpace(pair.Value.ToString()))
                {
                    Guid guidValue;
                    guidValue = Guid.Parse(pair.Value.ToString());

                    returnValue = new KeyValuePair<string, object>(pair.Key, guidValue);
                }
                else
                {
                    var test = !pair.Value.IsNullOrEmpty();
                    returnValue = !pair.Value.IsNullOrEmpty() ? new KeyValuePair<string, object>(pair.Key, pair.Value.ToObject(Nullable.GetUnderlyingType(property.PropertyType))) : new KeyValuePair<string, object>(pair.Key, null);
                }
            }
            else
            {
                if (pair.Value.IsNullOrEmpty() && pair.Value.Type != JTokenType.String)
                {
                    throw new NullReferenceException("Updated value of property cannot be null or empty.");
                }

                returnValue = new KeyValuePair<string, object>(pair.Key, pair.Value.ToObject(property.PropertyType));
            }

            return returnValue;
        }

        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.String && token.ToString() == string.Empty) ||
                   (token.Type == JTokenType.String && token.ToString().ToLower() == "null") ||
                   (token.Type == JTokenType.String && token.ToString() == string.Empty) ||
                   (token.Type == JTokenType.Null);
        }
    }
}
