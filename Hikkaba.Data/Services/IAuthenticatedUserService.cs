using Hikkaba.Models.Dto;

namespace Hikkaba.Data.Services;

public interface IAuthenticatedUserService
{
    ApplicationUserClaimsDto ApplicationUserClaims { get; set; }
}