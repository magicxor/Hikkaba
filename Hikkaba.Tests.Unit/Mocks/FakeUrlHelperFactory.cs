using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Hikkaba.Tests.Unit.Mocks;

public class FakeUrlHelperFactory : IUrlHelperFactory
{
    private readonly string _action;

    public FakeUrlHelperFactory(string action)
    {
        _action = action;
    }

    public IUrlHelper GetUrlHelper(ActionContext context)
    {
        return new FakeUrlHelper(_action);
    }
}
