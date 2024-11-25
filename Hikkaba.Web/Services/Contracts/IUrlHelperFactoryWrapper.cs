using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Services.Contracts;

public interface IUrlHelperFactoryWrapper
{
    IUrlHelper GetUrlHelper();
}