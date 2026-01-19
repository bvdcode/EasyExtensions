// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace EasyExtensions.AspNetCore.Authorization.Handlers
{
    /// <summary>
    /// This authorization handler will bypass all requirements
    /// </summary>
    public class AllowAnonymousAuthorizationHandler : IAuthorizationHandler
    {
        /// <summary>
        /// <see cref="IAuthorizationHandler"/> contract.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (IAuthorizationRequirement requirement in context.PendingRequirements)
            {
                context.Succeed(requirement); //Simply pass all requirements
            }
            return Task.CompletedTask;
        }
    }
}
