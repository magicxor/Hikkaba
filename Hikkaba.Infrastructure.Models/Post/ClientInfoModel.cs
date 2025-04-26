namespace Hikkaba.Infrastructure.Models.Post;

public class ClientInfoModel
{
    public required string? CountryIsoCode { get; set; }

    public required string? BrowserType { get; set; }

    public required string? OsType { get; set; }
}
