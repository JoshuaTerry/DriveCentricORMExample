using DriveCentric.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DriveCentric.Shared.Helpers
{
    public static class ReflectionHelper
    {
        #region Private Fields

        private static List<Assembly> localAssemblies = null;

        #endregion Private Fields

        #region Public Methods

        public static Type FindTypeByName(string typeFullName)
        {
            return FindTypeByName(typeFullName, GetLocalAssemblies());
        }

        public static Type FindTypeByName(string typeFullName, Assembly assembly)
        {
            return FindTypeByName(typeFullName, new[] { assembly });
        }

        public static Type FindTypeByName(string typeFullName, IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .FirstOrDefault(assembly => assembly.DefinedTypes.Any(t => t.FullName == typeFullName))?
                .GetType(typeFullName);
        }

        public static List<Type> GetDecoratedWith<T>()
            where T : Attribute
        {
            return GetLocalAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsAttributeDefined<T>())
                .ToList();
        }

        public static List<Type> GetDecoratedWith<T>(Assembly assembly)
            where T : Attribute
        {
            return assembly.GetTypes()
                .Where(x => x.IsAttributeDefined<T>())
                .ToList();
        }

        public static List<Type> GetDerivedTypes<T>()
        {
            return GetDerivedTypes<T>(GetLocalAssemblies().ToList());
        }

        public static List<Type> GetDerivedTypes<T>(Assembly assembly)
        {
            return GetDerivedTypes<T>(new List<Assembly>() { assembly });
        }

        public static List<Type> GetDerivedTypes<T>(List<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(x => x.GetTypes())
                .ToList()
                .Where(x => typeof(T).IsAssignableFrom(x) && x.IsClass)
                .Where(x => (x != typeof(T)))
                .ToList();
        }

        public static List<Type> GetImplementingTypes<T>()
        {
            return GetImplementingTypes<T>(GetLocalAssemblies());
        }

        public static List<Type> GetImplementingTypes<T>(Assembly assembly)
        {
            return GetImplementingTypes<T>(new List<Assembly>() { assembly });
        }

        public static List<Type> GetImplementingTypes<T>(IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(x => x.GetTypes())
                .ToList()
                .Where(t => t.GetInterfaces().Contains(typeof(T)))
                .ToList();
        }

        public static IEnumerable<Type> GetEnums()
        {
            return GetEnums(GetLocalAssemblies());
        }

        public static IEnumerable<Type> GetEnums(Assembly assembly)
        {
            return GetEnums(new List<Assembly>() { assembly });
        }

        public static IEnumerable<Type> GetEnums(IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(x => x.GetTypes())
                .Where(t => t.IsEnum && t.IsPublic);
        }

        public static List<TInterface> GetInstancesOf<TInterface>()
        {
            return GetInstancesOf<TInterface>(GetLocalAssemblies());
        }

        public static List<TInterface> GetInstancesOf<TInterface>(Assembly assembly)
        {
            return GetInstancesOf<TInterface>(new[] { assembly });
        }

        public static List<TInterface> GetInstancesOf<TInterface>(IEnumerable<Assembly> assembly)
        {
            var instanceList = new List<TInterface>();

            GetImplementingTypes<TInterface>().ForEach(t =>
            {
                instanceList.Add((TInterface)Activator.CreateInstance(t));
            });

            return instanceList;
        }

        public static IEnumerable<Assembly> GetLocalAssemblies()
        {
            if (localAssemblies == null)
            {
                string assemblyUri = GetURI(Assembly.GetCallingAssembly());

                localAssemblies = AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .Where(x => !x.IsDynamic && GetURI(x).Contains(assemblyUri))
                    .ToList();
            }
            return localAssemblies;
        }

        #endregion Public Methods

        #region Private Methods

        private static string GetURI(Assembly assembly)
        {
            string codeBase = assembly.CodeBase;

            if (!codeBase.StartsWith("file:"))
            {
                codeBase = Path.GetDirectoryName(codeBase);
                return new Uri(codeBase).AbsolutePath;
            }
            else
            {
                return Path.GetDirectoryName(new Uri(codeBase).AbsolutePath);
            }
        }

        #endregion Private Methods

    }
}
