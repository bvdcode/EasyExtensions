using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using EasyExtensions.Helpers;
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
            bool canSetIfNeverHad = await CanSetPasswordIfNeverHadAsync(userId);
            if (string.IsNullOrWhiteSpace(phc) && !canSetIfNeverHad)
            {
                return this.ApiNotFound("User or password does not exist");
            }
            if (!string.IsNullOrWhiteSpace(phc))
            {
                bool isValidPassword = _passwordHasher.Verify(request.CurrentPassword, phc);
                if (!isValidPassword)
                {
                    return this.ApiUnauthorized("Current password is incorrect");
                }
            }
            string newPhc = _passwordHasher.Hash(request.NewPassword);
            await SetUserPasswordPhcAsync(userId, newPhc);
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
            await SaveAndRevokeRefreshTokenAsync(userId.Value, request.RefreshToken, newRefreshToken, AuthType.Unknown);
            return Ok(new TokenPairDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            });
        }

        /// <summary>
        /// Authenticates a user using the provided credentials and issues a new access and refresh token pair.
        /// </summary>
        /// <remarks>This endpoint is typically used as part of a username and password authentication
        /// flow. The returned tokens can be used to access protected resources and to refresh authentication without
        /// re-entering credentials. Repeated failed login attempts may be subject to rate limiting or account lockout
        /// policies, depending on system configuration.</remarks>
        /// <param name="request">The login request containing the user's username and password. Cannot be null.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="TokenPairDto"/> with access and refresh tokens if
        /// authentication is successful; otherwise, an unauthorized response if the credentials are invalid.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            Guid? userId = await FindUserByUsernameAsync(request.Username);
            if (!userId.HasValue || userId == Guid.Empty)
            {
                await OnUserLoggingInAsync(userId.Value, AuthType.Credentials, AuthRejectionType.UserNotFound);
                return this.ApiUnauthorized("Invalid username or password");
            }
            bool canLogin = await CanUserLoginAsync(userId.Value);
            if (!canLogin)
            {
                await OnUserLoggingInAsync(userId.Value, AuthType.Credentials, AuthRejectionType.CannotLoginExternal);
                return this.ApiUnauthorized("Invalid username or password");
            }
            string? phc = await FindUserPhcAsync(userId.Value);
            if (string.IsNullOrWhiteSpace(phc))
            {
                await OnUserLoggingInAsync(userId.Value, AuthType.Credentials, AuthRejectionType.NoPassword);
                return this.ApiUnauthorized("Invalid username or password");
            }
            bool isValidPassword = _passwordHasher.Verify(request.Password, phc);
            if (!isValidPassword)
            {
                await OnUserLoggingInAsync(userId.Value, AuthType.Credentials, AuthRejectionType.WrongPassword);
                return this.ApiUnauthorized("Invalid username or password");
            }
            var roles = await GetUserRolesAsync(userId.Value);
            string accessToken = CreateAccessToken(userId.Value, roles);
            string refreshToken = StringHelpers.CreateRandomString(64);
            await SaveAndRevokeRefreshTokenAsync(userId.Value, string.Empty, refreshToken, AuthType.Credentials);
            await OnUserLoggingInAsync(userId.Value, AuthType.Credentials, AuthRejectionType.None);
            return Ok(new TokenPairDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        /// <summary>
        /// Authenticates a user using a Google OAuth token and issues a new access and refresh token pair if
        /// authentication succeeds.
        /// </summary>
        /// <remarks>The user's email must be verified if email verification is required by the
        /// application. If the user does not exist, a new user account is created using the information from Google.
        /// The method issues new tokens and revokes any previous refresh tokens for the user.</remarks>
        /// <param name="token">The Google OAuth access token to use for retrieving user information. Must be a valid token issued by
        /// Google.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="TokenPairDto"/> with access and refresh tokens if
        /// authentication is successful; otherwise, an unauthorized response if authentication fails or the user's
        /// email is not verified.</returns>
        /// <exception cref="InvalidOperationException">Thrown if user information cannot be retrieved from Google using the provided token.</exception>
        [HttpPost("login/google")]
        public async Task<IActionResult> LoginWithGoogle([FromQuery] string token)
        {
            const string url = "https://openidconnect.googleapis.com/v1/userinfo";
            using HttpClient http = new();
            http.DefaultRequestHeaders.Authorization = new("Bearer", token);
            GoogleOpenIdResponseDto response = await http.GetFromJsonAsync<GoogleOpenIdResponseDto>(url)
                ?? throw new InvalidOperationException("Failed to get user info from Google.");
            bool mustVerifyEmail = IsEmailVerificationRequired();
            if (mustVerifyEmail && !response.IsEmailVerified)
            {
                return this.ApiUnauthorized("Email is not verified");
            }
            Guid? userId = await CreateOrUpdateUserFromGoogleAsync(response);
            if (!userId.HasValue || userId == Guid.Empty)
            {
                return this.ApiUnauthorized("User not found");
            }
            bool canLogin = await CanUserLoginAsync(userId.Value);
            if (!canLogin)
            {
                await OnUserLoggingInAsync(userId.Value, AuthType.Credentials, AuthRejectionType.CannotLoginExternal);
                return this.ApiUnauthorized("Invalid username or password");
            }
            var roles = await GetUserRolesAsync(userId.Value);
            string accessToken = CreateAccessToken(userId.Value, roles);
            string refreshToken = StringHelpers.CreateRandomString(64);
            await SaveAndRevokeRefreshTokenAsync(userId.Value, string.Empty, refreshToken, AuthType.Google);
            await OnUserLoggingInAsync(userId.Value, AuthType.Google, AuthRejectionType.None);
            return Ok(new TokenPairDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        /// <summary>
        /// Asynchronously searches for a user by username and returns the user's unique identifier if found.
        /// </summary>
        /// <param name="username">The username of the user to locate. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the unique identifier of the
        /// user if found; otherwise, null.</returns>
        public abstract Task<Guid?> FindUserByUsernameAsync(string username);

        /// <summary>
        /// Saves a new refresh token for the specified user and revokes the previous refresh token asynchronously.
        /// </summary>
        /// <remarks>This method should be used to securely rotate refresh tokens, ensuring that the old
        /// token is invalidated and cannot be reused. Implementations must guarantee atomicity to prevent race
        /// conditions where both tokens could remain valid.</remarks>
        /// <param name="userId">The unique identifier of the user for whom the refresh token is being updated.</param>
        /// <param name="oldRefreshToken">The refresh token to be revoked. Cannot be null or empty.</param>
        /// <param name="newRefreshToken">The new refresh token to be saved for the user. Cannot be null or empty.</param>
        /// <param name="authType">The type of authentication used for the user when logged in for the first time.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task SaveAndRevokeRefreshTokenAsync(Guid userId, string oldRefreshToken, string newRefreshToken, AuthType authType);

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
        /// Determines whether a password can be set for the specified user if the user has never previously set a
        /// password.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to evaluate. This value must correspond to an existing user.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user is
        /// eligible to set a password for the first time; otherwise, <see langword="false"/>.</returns>
        public abstract Task<bool> CanSetPasswordIfNeverHadAsync(Guid userId);

        /// <summary>
        /// Determines asynchronously whether the specified user is permitted to log in.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose login permission is being checked.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the
        /// user is allowed to log in; otherwise, <see langword="false"/>.</returns>
        public virtual Task<bool> CanUserLoginAsync(Guid userId)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Handles logic to be performed when a user has successfully logged in.
        /// </summary>
        /// <param name="userId">The unique identifier of the user who has logged in.</param>
        /// <param name="authType">The type of authentication used for login.</param>
        /// <param name="authRejectionType">The type of authentication rejection, if any.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public virtual Task OnUserLoggingInAsync(Guid userId, AuthType authType, AuthRejectionType authRejectionType)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Determines whether email verification is required for user accounts.
        /// </summary>
        /// <returns><see langword="true"/> if email verification is required; otherwise, <see langword="false"/>.</returns>
        public virtual bool IsEmailVerificationRequired()
        {
            return true;
        }

        /// <summary>
        /// Creates a new user account based on the information provided by a Google OpenID response.
        /// If a user with the same email already exists, their information may be updated instead.
        /// </summary>
        /// <param name="dto">The Google OpenID response data containing user information to create the account. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the unique identifier of the
        /// newly created user if successful; otherwise, null.</returns>
        public virtual Task<Guid?> CreateOrUpdateUserFromGoogleAsync(GoogleOpenIdResponseDto dto)
        {
            return Task.FromResult<Guid?>(null);
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
    }
}
