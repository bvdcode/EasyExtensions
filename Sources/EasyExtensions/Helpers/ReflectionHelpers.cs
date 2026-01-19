// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
                Type[] types = Array.Empty<Type>();
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

        /// <summary>
        /// Copy matching properties from source to destination.
        /// </summary>
        /// <param name="source"> Source object. </param>
        /// <param name="destination"> Destination object. </param>
        public static void CopyMatchingProperties(object source, object destination)
        {
            if (source == null || destination == null)
            {
                return;
            }
            var sourceProperties = source.GetType().GetProperties();
            var destinationProperties = destination.GetType().GetProperties();
            foreach (var sourceProperty in sourceProperties)
            {
                var destinationProperty = destinationProperties.FirstOrDefault(x => x.Name == sourceProperty.Name);
                destinationProperty?.SetValue(destination, sourceProperty.GetValue(source));
            }
        }
    }
}
