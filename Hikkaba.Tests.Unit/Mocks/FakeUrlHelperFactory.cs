using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Hikkaba.Tests.Unit.Mocks;

internal sealed class FakeUrlHelperFactory : IUrlHelperFactory
{
    private readonly FakeUrlHelperParams _fakeUrlHelperParams;

    public FakeUrlHelperFactory(FakeUrlHelperParams fakeUrlHelperParams)
    {
        _fakeUrlHelperParams = fakeUrlHelperParams;
    }

    public IUrlHelper GetUrlHelper(ActionContext context)
    {
        return new FakeUrlHelper(_fakeUrlHelperParams);
    }
}
