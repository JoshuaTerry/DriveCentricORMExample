using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DriveCentric.Shared.Helpers
{
    public static class PathHelper
    {
        public const string FieldExcludePrefix = "^";
        public const string IncludeEverythingField = "*";

        /// <summary>
        /// Convert a property expression to a property name or path.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property">Property expression</param>
        /// <param name="shouldContainObjectPath">TRUE to include full path.</param>
        /// <returns></returns>
        public static string NameFor<T>(Expression<Func<T, object>> property, bool shouldContainObjectPath = false)
        {
            var member = property.Body as MemberExpression;
            if (member == null)
            {
                var unary = property.Body as UnaryExpression;
                if (unary != null)
                {
                    member = unary.Operand as MemberExpression;
                }
            }
            if (shouldContainObjectPath && member != null)
            {
                var path = member.Expression.ToString();
                var objectPath = member.Expression.ToString().Split('.').Where(a => !a.Equals("First()")).ToArray();
                if (objectPath.Length >= 2)
                {
                    path = String.Join(".", objectPath, 1, objectPath.Length - 1);
                    return $"{path}.{member.Member.Name}";
                }
            }
            return member?.Member.Name ?? String.Empty;
        }

        /// <summary>
        /// A builder for a field list
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        public class FieldListBuilder<T>
        {
            private List<string> fields;
            private string pathPrefix;

            public FieldListBuilder()
            {
                pathPrefix = string.Empty;
                fields = new List<string>();
            }

            private FieldListBuilder(string prefix, List<string> fields)
            {
                pathPrefix = prefix;
                this.fields = fields;
            }

            public override string ToString()
            {
                return string.Join(",", fields);
            }

            /// <summary>
            /// Remove all existing paths.
            /// </summary>
            public FieldListBuilder<T> Clear()
            {
                fields.Clear();
                return this;
            }

            /// <summary>
            /// Include a path in the field list being built.
            /// </summary>
            public FieldListBuilder<T> Include(Expression<Func<T, object>> path)
            {
                fields.Add(pathPrefix + NameFor(path, true));
                return this;
            }

            /// <summary>
            /// Force all fields are to be included.
            /// </summary>
            public FieldListBuilder<T> IncludeAll()
            {
                fields.Add(PathHelper.IncludeEverythingField);
                return this;
            }


            /// <summary>
            /// Exclude a path from the field list being built.
            /// </summary>
            public FieldListBuilder<T> Exclude(Expression<Func<T, object>> path)
            {
                string field = NameFor(path, true);
                int pos = field.LastIndexOf('.');
                if (pos > 0)
                {
                    field = field.Substring(0, pos + 1) + FieldExcludePrefix + field.Substring(pos + 1);
                }
                else
                {
                    field = FieldExcludePrefix + field;
                }
                fields.Add(pathPrefix + field);
                return this;
            }

            /// <summary>
            /// Include a path to a reference (entity) property and return a new FieldListBuilder for child properties.
            /// </summary>
            public FieldListBuilder<T1> IncludeReference<T1>(Expression<Func<T, object>> path)
            {
                string field = pathPrefix + NameFor(path, true);
                fields.Add(field);
                string prefix = field + ".";
                return new FieldListBuilder<T1>(prefix, fields);
            }

            /// <summary>
            /// Generate the field list.
            /// </summary>
            /// <param name="builder"></param>
            public static implicit operator string(FieldListBuilder<T> builder)
            {
                return builder.ToString();
            }
        }


    }
}
