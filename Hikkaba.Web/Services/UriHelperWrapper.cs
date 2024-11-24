using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Hikkaba.Web.Services;

public interface IUrlHelperFactoryWrapper
{
    IUrlHelper GetUrlHelper();
}
    
public class UrlHelperFactoryWrapper: IUrlHelperFactoryWrapper
{
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor;

    public UrlHelperFactoryWrapper(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
    {
        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
    }

    public IUrlHelper GetUrlHelper()
    {
        return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
    }
}