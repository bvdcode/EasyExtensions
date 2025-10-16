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

namespace EasyExtensions.AspNetCore.Authorization.Controllers
{
    /// <summary>
    /// Base controller for authentication-related operations.
    /// </summary>
    public abstract class BaseAuthController(
        IPasswordHashService _passwordHasher,
        ITokenProvider _tokenProvider) : ControllerBase
    {
        /// <summary>
        /// Gets the IP address from which the current request originated.
        /// </summary>
        public IPAddress RequestIP => IPAddress.Parse(Request.GetRemoteAddress());

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
                return this.ApiNotFound("User not found");
            }
            bool isValidPassword = _passwordHasher.Verify(request.CurrentPassword, phc);
            if (!isValidPassword)
            {
                return this.ApiBadRequest("Invalid current password");
            }
            string newPhc = _passwordHasher.Hash(request.NewPassword);
            await SetUserPasswordPhcAsync(userId, newPhc);
            await OnUserChangedPasswordAsync(userId);
            return Ok("Password changed successfully");
        }

        /// <summary>
        /// Handles a request to refresh an access token using a valid refresh token.
        /// </summary>
        /// <remarks>If the provided refresh token has been revoked or is not found, the method returns a 404 Not Found response. </remarks>
        /// <param name="request">The refresh token request containing the refresh token to validate and exchange for a new access token.
        /// Cannot be null.</param>
        /// <returns>An <see cref="IActionResult"/> containing the new access token if the refresh token is valid; otherwise, a
        /// 404 Not Found result if the token is invalid or revoked.</returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            Guid? userId = await FindUserByRefreshTokenAsync(request.RefreshToken);
            if (!userId.HasValue || userId == Guid.Empty)
            {
                return this.ApiNotFound("Refresh token was not found");
            }

            string newRefreshToken = StringHelpers.CreateRandomString(64);
            var roles = await GetUserRolesAsync(userId.Value);
            string accessToken = CreateAccessToken(userId.Value, roles);
            await SaveAndRevokeRefreshTokenAsync(userId.Value, request.RefreshToken, newRefreshToken);
            await OnTokenRefreshedAsync(userId.Value, newRefreshToken);
            return Ok(new TokenPairDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            });
        }

        private string CreateAccessToken(Guid userId, IEnumerable<string> roles)
        {
            return _tokenProvider.CreateToken(cb =>
            {
                cb.Add(JwtRegisteredClaimNames.Sub, userId.ToString());
                foreach (string role in roles)
                {
                    cb.Add(ClaimTypes.Role, role);
                }
                return cb;
            });
        }

        /// <summary>
        /// Saves a new refresh token for the specified user and revokes the previous refresh token asynchronously.
        /// </summary>
        /// <remarks>This method should be used to securely rotate refresh tokens, ensuring that the old
        /// token is invalidated and cannot be reused. Implementations must guarantee atomicity to prevent race
        /// conditions where both tokens could remain valid.</remarks>
        /// <param name="userId">The unique identifier of the user for whom the refresh token is being updated.</param>
        /// <param name="oldRefreshToken">The refresh token to be revoked. Cannot be null or empty.</param>
        /// <param name="newRefreshToken">The new refresh token to be saved for the user. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task SaveAndRevokeRefreshTokenAsync(Guid userId, string oldRefreshToken, string newRefreshToken);

        /// <summary>
        /// Asynchronously retrieves the unique identifier of a user associated with the specified refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token to search for. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user's unique identifier if
        /// a matching refresh token is found; otherwise, null.</returns>
        public abstract Task<Guid?> FindUserByRefreshTokenAsync(string refreshToken);

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
        /// Handles actions to perform when a user's refresh token has been updated.
        /// </summary>
        /// <remarks>Override this method to implement custom logic when a user's refresh token is
        /// refreshed, such as updating persistent storage or notifying other services.</remarks>
        /// <param name="userId">The unique identifier of the user whose refresh token was refreshed.</param>
        /// <param name="newRefreshToken">The new refresh token value assigned to the user.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public virtual Task OnTokenRefreshedAsync(Guid userId, string newRefreshToken)
        {
            return Task.CompletedTask;
        }
    }
}
