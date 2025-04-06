using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Services.Contracts;

internal interface IUrlHelperFactoryWrapper
{
    IUrlHelper GetUrlHelper();
}