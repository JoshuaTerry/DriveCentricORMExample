using DriveCentric.Shared.Helpers;
using DriveCentric.Shared.Interfaces;
using DriveCentric.Shared.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace DriveCentric.WebApi.Helpers
{
    public class DynamicTransmogrifier
    {
        private const int MAX_RECURSION_DEPTH = 10;

        /* Fields is a comma delimited list of property names or paths (e.g. State.County) that should be included in the response.
         * Fields can be excluded:  This can be useful to exclude properties like Region.ParentRegion or Region.ChildRegions (because they are recursive.) 
         * To exclude a field, prefix it with "^": "^ParentRegion,^ChildRegions". 
         * If a field list is nothing but excludes, all other fields will be included.
         * To exclude a field from a referenced entity: "ChildRegion.^ParentRegion"
         * To force all fields to be included:  "*,ChildRegion.Code"
         */
        public IDataResponse ToDynamicResponse<T>(IDataResponse<T> response, string fields = null)
        {
            var dynamicResponse = new DataResponse<dynamic>
            {
                ErrorMessages = response.ErrorMessages,
                IsSuccessful = response.IsSuccessful,
                TotalResults = response.TotalResults,
                VerboseErrorMessages = response.VerboseErrorMessages
            };

            if (response.Data == null)
            {
                dynamicResponse.Data = null;
            }
            else if (response.Data is IEnumerable<ICanTransmogrify>)
            {
                dynamicResponse.Data = ToDynamicList((response.Data as IEnumerable<ICanTransmogrify>), fields);
            }
            else if (response.Data is ICanTransmogrify)
            {
                dynamicResponse.Data = ToDynamicObject((response.Data as ICanTransmogrify), fields);
            }
            else if (IsSimple(response.Data.GetType()))
            {
                dynamicResponse.Data = response.Data;
            }
            return dynamicResponse;
        }

        internal bool IsSimple(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(type.GetGenericArguments()[0]);
            }
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal);
        }

        internal dynamic ToDynamicList<T>(IEnumerable<T> data, string fields = null)
            where T : ICanTransmogrify
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                fields = null;
            }
            if (fields == null)
            {
                return data;
            }

            string upperCaseFields = fields?.ToUpper();

            List<string> listOfFields = upperCaseFields?.Split(',').ToList() ?? new List<string>();
            return RecursivelyTransmogrify(data, listOfFields);
        }

        internal dynamic ToDynamicObject<T>(T data, string fields = null)
            where T : ICanTransmogrify
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                fields = null;
            }

            if (fields == null)
            {
                return data;
            }

            string upperCaseFields = fields?.ToUpper();

            List<string> listOfFields = upperCaseFields?.Split(',').ToList() ?? new List<string>();
            return RecursivelyTransmogrify(data, listOfFields);
        }

        private dynamic RecursivelyTransmogrify<T>(T data, List<string> fieldsToInclude = null, IEnumerable<Guid> visited = null, int level = 0)
            where T : ICanTransmogrify
        {
            if (level >= MAX_RECURSION_DEPTH)
            {
                return null;
            }

            dynamic returnObject = new ExpandoObject();
            if (visited == null)
            {
                visited = new List<Guid> { data.Id };
            }
            else if (visited.Contains(data.Id))
            {
                // To avoid infinite loops, if we have processed this object before, just return the Id
                ((IDictionary<string, object>)returnObject)["Id"] = data.Id;
                return returnObject;
            }
            else
            {
                visited = visited.Union(new Guid[] { data.Id });
            }
            Type type = data.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            PropertyInfo[] properties = type.GetProperties(flags);

            // Determine if there are any fields being excluded.
            int excludesCount = fieldsToInclude.Count(p => p.Contains(PathHelper.FieldExcludePrefix));
            bool hasExcludes = (excludesCount > 0);

            // Include all fields if field list is empty, or field list contains only fields to exclude.
            bool includeEverything = (fieldsToInclude == null || fieldsToInclude.Count == 0 || excludesCount == fieldsToInclude.Count || fieldsToInclude.Contains(PathHelper.IncludeEverythingField));

            foreach (PropertyInfo property in properties)
            {
                object fieldValue = property.GetValue(data, null);
                string propertyNameUppercased = property.Name.ToUpper();
                string propertyNameExclude = PathHelper.FieldExcludePrefix + propertyNameUppercased;
                string propertyNameAsAccessor = propertyNameUppercased + ".";

                if (fieldValue is IEnumerable<ICanTransmogrify> && (includeEverything && (!hasExcludes && fieldsToInclude.Contains(propertyNameExclude))
                                          || fieldsToInclude.Any(a => a.StartsWith(propertyNameAsAccessor))))
                {
                    var strippedFieldList = StripFieldList(fieldsToInclude, propertyNameUppercased);
                    ((IDictionary<string, object>)returnObject)[property.Name] = RecursivelyTransmogrify((fieldValue as IEnumerable<ICanTransmogrify>), strippedFieldList, visited, level + 1);
                }
                else if (fieldValue is ICanTransmogrify && (includeEverything && (!hasExcludes && fieldsToInclude.Contains(propertyNameExclude))
                                          || fieldsToInclude.Any(a => a.StartsWith(propertyNameAsAccessor))))
                {
                    var strippedFieldList = StripFieldList(fieldsToInclude, propertyNameUppercased);
                    ((IDictionary<string, object>)returnObject)[property.Name] = RecursivelyTransmogrify((fieldValue as ICanTransmogrify), strippedFieldList, visited, level + 1);
                }
                else if ((includeEverything && !(hasExcludes && fieldsToInclude.Contains(propertyNameExclude))
                                          ||
                                       fieldsToInclude.Contains(propertyNameUppercased)))
                {
                    ((IDictionary<string, object>)returnObject)[property.Name] = fieldValue;
                }
            }

            return returnObject;
        }

        private List<string> StripFieldList(List<string> fieldsToInclude, string propertyNameUppercased)
        {
            if (fieldsToInclude.Count == 0)
            {
                return fieldsToInclude;
            }
            string currentProperty = $"{propertyNameUppercased}.";
            int length = currentProperty.Length;
            return fieldsToInclude.Where(a => a.StartsWith(currentProperty)).Select(a => a.Substring(length)).ToList();
        }

        private dynamic RecursivelyTransmogrify<T>(IEnumerable<T> entities, List<string> fieldsToInclude = null, IEnumerable<Guid> visited = null, int level = 0)
            where T : ICanTransmogrify
        {
            if (level >= MAX_RECURSION_DEPTH)
            {
                return null;
            }

            var list = new List<ExpandoObject>();

            foreach (var item in entities)
            {
                list.Add(RecursivelyTransmogrify(item, fieldsToInclude, visited, level + 1));
            }

            return list;
        }
    }
} 