using Hikkaba.Web.ViewModels.PostsViewModels;

namespace Hikkaba.Web.ViewModels.BansViewModels;

public sealed class BanCreateDataViewModel
{
    public required PostDetailsViewModel PostDetails { get; set; }
    public required IpAddressDetailsViewModel IpAddressDetails { get; set; }
}
