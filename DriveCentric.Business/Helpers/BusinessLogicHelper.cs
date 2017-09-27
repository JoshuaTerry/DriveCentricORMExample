using DriveCentric.Business.Interfaces;
using DriveCentric.Shared.Extensions;
using DriveCentric.Shared.Helpers;
using DriveCentric.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace DriveCentric.Business.Helpers
{
    /// <summary>
    /// Static helper class for getting a business logic instance for an entity type 
    /// in order to perform common entity logic such as validation.
    /// </summary>
    public static class BusinessLogicHelper
    {
        // Dictionary to map entity types to business logic types.
        private static Dictionary<Type, Type> entityLogicDict;

        /// <summary>
        /// Initialize the mapping dictionary.
        /// </summary>
        private static void Initialize()
        {
            if (entityLogicDict == null)
            {
                entityLogicDict = new Dictionary<Type, Type>()
                {
                    // Hard-coded mappings can go here.
                };

                // Derive mappings from generic types used in classes based on EntityLogicBase.
                foreach (var type in ReflectionHelper.GetImplementingTypes<IEntityLogic>(typeof(BusinessLogicHelper).Assembly))
                {
                    Type[] genericTypes = type.BaseType.GenericTypeArguments;
                    if (genericTypes != null && genericTypes.Length == 1)
                    {
                        entityLogicDict[genericTypes[0]] = type;
                    }
                }
            }
        }

        /// <summary>
        /// Force initialization - used in unit testing.
        /// </summary>
        internal static void ForceInitialize()
        {
            entityLogicDict = null;
            Initialize();
        }

        /// <summary>
        /// Get a business logic instance for an entity type.
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="unitOfWork">Unit of work</param>
        public static IEntityLogic GetBusinessLogic<T>(IUnitOfWork unitOfWork) where T : class, IEntity
        {
            return GetBusinessLogic(unitOfWork, typeof(T));
        }

        /// <summary>
        /// Get a business logic instance for a type.
        /// </summary>
        /// <param name="unitOfWork">Unit of work</param>
        /// <param name="entityType">Type, which should be an entity type.</param>
        public static IEntityLogic GetBusinessLogic(IUnitOfWork unitOfWork, Type entityType)
        {
            Initialize();
            Type logicType = entityLogicDict.GetValueOrDefault(entityType, null);
            if (logicType != null)
            {
                return unitOfWork.GetBusinessLogic(logicType) as IEntityLogic;
            }

            return unitOfWork.GetBusinessLogic<EntityLogicBase>();
        }

    }
}
