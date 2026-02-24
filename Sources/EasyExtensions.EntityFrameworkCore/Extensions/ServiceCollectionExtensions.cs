// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using Gridify;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EasyExtensions.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Sets up Gridify.
        /// </summary>
        public static IServiceCollection AddGridifyMappers(this IServiceCollection services)
        {
            GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
            services.AddGridifyMappers(Assembly.GetCallingAssembly());
            return services;
        }
    }
}
