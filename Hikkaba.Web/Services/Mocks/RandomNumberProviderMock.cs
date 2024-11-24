using DNTCaptcha.Core.Contracts;

namespace Hikkaba.Web.Services.Mocks;

public class RandomNumberProviderMock : IRandomNumberProvider
{
    public int Next()
    {
        return 0;
    }

    public int Next(int max)
    {
        return 0;
    }

    public int Next(int min, int max)
    {
        return min;
    }
}