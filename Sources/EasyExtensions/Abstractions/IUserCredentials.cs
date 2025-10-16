using System;
using System.Text;
using System.Collections.Generic;

namespace EasyExtensions.Abstractions
{
    public interface IUserIdentity<out TId>
    {
        TId Id { get; }
        string UserName { get; }
        string PasswordPhc { get; }
    }
}
