using System;

namespace Application.Interfaces.Security
{
    /// <summary>
    /// Abstraction for accessing the currently authenticated user.
    /// This belongs in the Application layer so services can depend on it
    /// without referencing ASP.NET Core types.
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary>The unique identifier of the current user, or <c>null</c> if unauthenticated.</summary>
        string? UserId { get; }

        /// <summary><c>true</c> when the current user is in the "Doctor" role.</summary>
        bool IsDoctor { get; }

        /// <summary><c>true</c> when the current user is in the "Nurse" role.</summary>
        bool IsNurse { get; }
    }
}
