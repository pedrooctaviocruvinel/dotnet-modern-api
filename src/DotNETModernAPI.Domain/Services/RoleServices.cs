﻿using DotNETModernAPI.Domain.Entities;
using DotNETModernAPI.Infrastructure.CrossCutting.Core.DTOs;
using DotNETModernAPI.Infrastructure.CrossCutting.Core.Enums;
using DotNETModernAPI.Infrastructure.CrossCutting.Core.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace DotNETModernAPI.Domain.Services;

public class RoleServices
{
    public RoleServices(RoleManager<Role> roleManager, IValidator<Role> roleValidator, IOptions<PoliciesDTO> policies)
    {
        _roleManager = roleManager;
        _roleValidator = roleValidator;
        _policies = policies.Value;
    }

    private readonly RoleManager<Role> _roleManager;
    private readonly IValidator<Role> _roleValidator;
    private readonly PoliciesDTO _policies;

    public virtual async Task<ResultWrapper<Guid>> Create(string name)
    {
        if (await _roleManager.FindByNameAsync(name) != null)
            return new ResultWrapper<Guid>(EErrorCode.RoleAlreadyExists);

        var role = new Role(name);

        var validationResult = _roleValidator.Validate(role);

        if (!validationResult.IsValid)
            return new ResultWrapper<Guid>(validationResult.Errors);

        var createRoleResult = await _roleManager.CreateAsync(role);

        if (!createRoleResult.Succeeded)
            return new ResultWrapper<Guid>(createRoleResult.Errors);

        return new ResultWrapper<Guid>(role.Id);
    }

    public virtual async Task<ResultWrapper> AddClaimsToRole(string id, IEnumerable<Claim> claims)
    {
        var role = await _roleManager.FindByIdAsync(id);

        if (role == null)
            return new ResultWrapper(EErrorCode.RoleNotFound);

        var roleClaims = await _roleManager.GetClaimsAsync(role);

        foreach (var claim in claims)
        {
            if (!_policies.Users.Contains(claim.Value) && !_policies.Roles.Contains(claim.Value))
                return new ResultWrapper(EErrorCode.InvalidPolicy, new List<string>() { claim.Value });

            if (roleClaims.Any(c => c.Value == claim.Value))
                return new ResultWrapper(EErrorCode.PolicyAlreadyAssignedToRole, new List<string>() { claim.Value });
        }

        foreach (var claim in claims)
        {
            var addRoleResult = await _roleManager.AddClaimAsync(role, claim);

            if (!addRoleResult.Succeeded)
                return new ResultWrapper(addRoleResult.Errors);
        }

        return new ResultWrapper();
    }
}