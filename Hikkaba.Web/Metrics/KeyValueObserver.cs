using System;
using System.Collections.Generic;

namespace Hikkaba.Web.Metrics;

internal class KeyValueObserver : IObserver<KeyValuePair<string, object?>>
{
    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(KeyValuePair<string, object?> value)
    {
    }
}
