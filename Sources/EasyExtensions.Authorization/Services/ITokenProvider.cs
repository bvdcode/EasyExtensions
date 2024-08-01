using System;
using EasyExtensions.Authorization.Builders;

namespace EasyExtensions.Authorization.Services
{
    internal interface ITokenProvider
    {
        string CreateToken(Func<ClaimBuilder, ClaimBuilder>? claimBuilder = null);
    }
}