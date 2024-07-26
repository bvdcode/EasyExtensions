using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace EasyExtensions.Helpers
{
    /// <summary>
    /// Reflection helpers.
    /// </summary>
    public static class ReflectionHelpers
    {
        /// <summary>
        /// Get all types inherited from interface.
        /// </summary>
        /// <typeparam name="TInterface"> Interface type. </typeparam>
        /// <returns> All types inherited from interface. </returns>
        public static IEnumerable<Type> GetTypesOfInterface<TInterface>() where TInterface : class
        {
            List<Type> result = new List<Type>();
            var callingAssembly = Assembly.GetCallingAssembly();
            var entryAssembly = Assembly.GetEntryAssembly();
            var executingAssembly = Assembly.GetExecutingAssembly();
            var assemblies = new List<Assembly> { callingAssembly, entryAssembly, executingAssembly };
            assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
            assemblies = assemblies
                .Where(x => x != null)
                .Distinct()
                .ToList();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = new Type[0];
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }
                foreach (Type type in types)
                {
                    if (type.GetInterfaces().Length > 0 && type.GetInterfaces().Any(x => x == typeof(TInterface) && !type.IsAbstract))
                    {
                        if (!result.Any(x => x.FullName == type.FullName))
                        {
                            result.Add(type);
                        }
                    }
                }
            }
            return result.Distinct();
        }
    }
}