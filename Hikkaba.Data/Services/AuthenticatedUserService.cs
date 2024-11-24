using Hikkaba.Models.Dto;

namespace Hikkaba.Data.Services;

public interface IAuthenticatedUserService
{
    ApplicationUserClaimsDto ApplicationUserClaims { get; set; }
}
    
public class AuthenticatedUserService: IAuthenticatedUserService
{
    public ApplicationUserClaimsDto ApplicationUserClaims { get; set; }
}