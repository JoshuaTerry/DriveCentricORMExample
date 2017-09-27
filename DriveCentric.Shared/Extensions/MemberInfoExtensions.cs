using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DriveCentric.Shared.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="MemberInfo" />. These are mostly convenience and
    /// shortcut methods to keep code cleaner by handling null checks and other logic.
    /// </summary>
    /// <remarks>
    /// Some of these methods were originally defined in <see cref="TypeExtensions" /> but were moved to
    /// cover the parent Type, <see cref="MemberInfo" />.
    /// </remarks>
    public static class MemberInfoExtensions
    {
        #region Public Methods

        /// <summary>
        /// If this <see cref="MemberInfo" /> is annotated with the given <see cref="Attribute" /> type ,
        /// that attribute is returned. Otherwise, <c>null</c>.
        /// </summary>
        /// <remarks>
        /// Note that this will only return a single instance of the attribute. If the attribute can be
        /// defined multiple times, use <see cref="GetAttributes{T}(Type, bool)" /> instead. This will
        /// return the first discovered attribute definition.
        /// </remarks>
        /// <typeparam name="TAttribute">Attribute type</typeparam>
        /// <param name="self">This <see cref="MemberInfo" /></param>
        /// <param name="inherit">If true, all parent types will be checked as well</param>
        /// <returns></returns>
        public static TAttribute GetAttribute<TAttribute>(this MemberInfo self, bool inherit = false)
            where TAttribute : Attribute
        {
            if (self.IsAttributeDefined<TAttribute>(inherit))
            {
                return self.GetCustomAttribute<TAttribute>();
            }

            return null;
        }

        /// <summary>
        /// If this <see cref="MemberInfo" /> is annotated with the given <see cref="Attribute" /> type ,
        /// all instances of that attribute type are returned. Otherwise, an empty list.
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type</typeparam>
        /// <param name="type">This <see cref="MemberInfo" /></param>
        /// <param name="inherit">If true, all parent types will be checked as well</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo self, bool inherit = false)
            where TAttribute : Attribute
        {
            var attributes = new List<TAttribute>();

            if (self.IsAttributeDefined<TAttribute>(inherit))
            {
                attributes.AddRange(self.GetCustomAttributes<TAttribute>(inherit));
            }

            return attributes;
        }

        /// <summary>
        /// Determines whether or not this <see cref="MemberInfo" /> has an <see cref="Attribute" /> of
        /// the given Type defined.
        /// <para>
        /// To access the defined attribute(s), use <see cref="GetAttribute{T}(MemberInfo, bool)" /> or
        /// <see cref="GetAttributes{T}(MemberInfo, bool)" />.
        /// </para>
        /// </summary>
        /// <typeparam name="TAttribute">Attribute Type</typeparam>
        /// <param name="self"></param>
        /// <param name="inherit">If true, all parent types well be checked as well</param>
        /// <returns></returns>
        public static bool IsAttributeDefined<TAttribute>(this MemberInfo self, bool inherit = false)
        {
            return self.IsDefined(typeof(TAttribute), inherit);
        }

        /// <summary>
        /// Determines whether or not this has an <see cref="Attribute" /> of the given Type defined.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="attributeType">Attribute Type</param>
        /// <param name="inherit">If true, all parent types well be checked as well</param>
        /// <returns></returns>
        public static bool IsAttributeDefined(this MemberInfo self, Type attributeType, bool inherit = false)
        {
            return self.IsDefined(attributeType, inherit);
        }
        /// <summary>
        /// Returns CustomAttributes for the MemberInfo specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this MemberInfo member)
        {
            return member.GetCustomAttributes(typeof(T), true).Cast<T>().SingleOrDefault();
        }
        /// <summary>
        /// Gets Attributes of Type T from a PropertyInfo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetAttributes<T>(this PropertyInfo property)
        {
            return property.GetAttributes<T>(GetMetadataTypes(property.DeclaringType));
        }
        /// <summary>
        /// Returns a collection of Attributes Type T from the collection types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        private static IEnumerable<T> GetAttributes<T>(this PropertyInfo property, IEnumerable<Type> types)
        {
            List<T> attributes = new List<T>();
            foreach (Type type in types)
            {
                PropertyInfo metaProperty = type.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (metaProperty != null)
                {
                    attributes.AddRange(Attribute.GetCustomAttributes(metaProperty, true).Where(a => a is T).Cast<T>());
                }
            }

            return attributes;
        }
        /// <summary>
        /// Returns collection of Attributes of type T for the type specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetAttributes<T>(this Type type)
        {
            return GetMetadataTypes(type).GetAttributes<T>();
        }
        /// <summary>
        /// Returns collection of attributes of Type T for the collection of types passed in
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="types"></param>
        /// <returns></returns>
        private static IEnumerable<T> GetAttributes<T>(this IEnumerable<Type> types)
        {
            List<T> attributes = new List<T>();

            foreach (Type type in types)
            {
                attributes.AddRange(type.GetCustomAttributes(true).Where(a => a is T).Cast<T>());
            }

            return attributes;
        }
        /// <summary>
        /// Returns collection of Types 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<Type> GetMetadataTypes(Type type)
        {
            yield return type;

            var meta = type.GetAttribute<MetadataTypeAttribute>();

            if (meta != null)
                yield return meta.MetadataClassType;
        }
        #endregion Public Methods
    }

}
