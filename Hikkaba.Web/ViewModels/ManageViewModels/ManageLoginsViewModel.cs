using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Hikkaba.Web.ViewModels.ManageViewModels
{
    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
    }
}
