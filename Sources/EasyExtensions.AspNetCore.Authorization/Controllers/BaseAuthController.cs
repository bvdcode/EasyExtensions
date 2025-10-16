using System;
using System.Net;
using EasyExtensions.Helpers;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using EasyExtensions.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using EasyExtensions.AspNetCore.Extensions;
using EasyExtensions.AspNetCore.Authorization.Models.Dto;
using EasyExtensions.AspNetCore.Authorization.Abstractions;
using EasyExtensions.AspNetCore.Authorization.Models.Dto.Enums;
using Microsoft.AspNetCore.Http;

namespace EasyExtensions.AspNetCore.Authorization.Controllers
{
    /// <summary>
    /// Base controller for authentication-related operations.
    /// </summary>
    public abstract class BaseAuthController<TUser>(IPasswordHashService _passwordHasher, ITokenProvider _tokenProvider) : ControllerBase
    {
        /// <summary>
        /// Changes the password for the currently authenticated user.
        /// </summary>
        /// <param name="request">The request containing the current and new passwords.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">Invalid request or current password is incorrect.</response>
        /// <response code="401">Unauthorized - user is not authenticated.</response>
        /// <response code="500">Internal server error.</response>
        /// <remarks>
        /// This endpoint allows an authenticated user to change their password by providing their current password and a new password.
        /// The current password is verified before updating to the new password.
        /// </remarks>
        [Authorize]
        [HttpPost("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            Guid userId = User.GetUserId();
            string? phc = await FindUserPhcAsync(userId);
            if (string.IsNullOrWhiteSpace(phc))
            {
                return Problem("User not found.", statusCode: StatusCodes.Status404NotFound);
            }
            bool isValidPassword = _passwordHasher.Verify(request.CurrentPassword, phc);
            if (!isValidPassword)
            {
                return BadRequest(new { message = "Invalid current password." });
            }
            string newPhc = _passwordHasher.Hash(request.NewPassword);
            await SetUserPasswordPhcAsync(userId, newPhc);
            await OnUserChangedPasswordAsync(userId);
            return Ok(new { message = "Password changed successfully." });
        }

        /// <summary>
        /// Finds the password hash in PHC format for the specified user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The password hash in PHC format, or null if the user is not found.</returns>
        public abstract Task<string?> FindUserPhcAsync(Guid userId);

        /// <summary>
        /// Sets the password hash in PHC format for the specified user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="phc">The password hash in PHC format.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public abstract Task SetUserPasswordPhcAsync(Guid userId, string phc);

        /// <summary>
        /// Asynchronously retrieves the collection of role names assigned to the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose roles are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
        /// strings representing the names of the roles assigned to the user. The collection is empty if the user has no
        /// roles.</returns>
        public abstract Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// Handles logic to be executed after a user has changed their password.
        /// </summary>
        /// <param name="userId">The unique identifier of the user who changed their password.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public virtual Task OnUserChangedPasswordAsync(Guid userId)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles logic to be executed after a user has changed their password.
        /// </summary>
        /// <param name="userName">The user name of the account whose password was changed. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public virtual Task OnUserChangedPasswordAsync(string userName)
        {
            return Task.CompletedTask;
        }
    }
}
