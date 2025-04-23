using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Thinktecture;

namespace Hikkaba.Data.Utils;

public static class ContextConfiguration
{
    public static readonly Action<SqlServerDbContextOptionsBuilder> SqlServerOptionsAction = sql
        => sql
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
            .AddWindowFunctionsSupport()
            .AddTableHintSupport();
}
