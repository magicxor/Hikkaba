using Hikkaba.Models.Dto;

namespace Hikkaba.Data.Services;

public class AuthenticatedUserService: IAuthenticatedUserService
{
    public ApplicationUserClaimsDto ApplicationUserClaims { get; set; }
}