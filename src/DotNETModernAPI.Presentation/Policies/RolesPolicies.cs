﻿using Microsoft.AspNetCore.Authorization;

namespace DotNETModernAPI.Presentation.Policies;

internal static class RolesPolicies
{
    public static void AddRolesOptions(this AuthorizationOptions options) =>
        options.AddPolicy("RolesCreate", apb => apb.RequireAssertion(ahc => ahc.User.HasClaim(c => c.Value == "roles.create")));
}
