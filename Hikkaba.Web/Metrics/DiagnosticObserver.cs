using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Web.Metrics;

internal class DiagnosticObserver : IObserver<DiagnosticListener>
{
    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(DiagnosticListener value)
    {
        if (value.Name == DbLoggerCategory.Name)
        {
            value.Subscribe(new KeyValueObserver());
        }
    }
}
