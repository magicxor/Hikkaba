using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Tests.Integration.Utils;

public static class DbUtils
{
    public static async Task WaitForFulltextIndexAsync(
        ILogger logger,
        DbContext dbContext,
        string tableName,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var delayBeforeCheck = TimeSpan.FromMilliseconds(200);
        timeout ??= TimeSpan.FromSeconds(30);
        var stopwatch = Stopwatch.StartNew();
        int? tableFulltextPopulateStatus = -999;

        while (!cancellationToken.IsCancellationRequested
               && tableFulltextPopulateStatus != 0)
        {
            // Wait for a short period to allow the full-text index to start populating
            await Task.Delay(delayBeforeCheck, cancellationToken);

            tableFulltextPopulateStatus = await dbContext.Database
                .SqlQuery<int?>($"SELECT OBJECTPROPERTYEX(OBJECT_ID({tableName}), 'TableFulltextPopulateStatus') AS Value")
                .FirstOrDefaultAsync(cancellationToken);

            logger.LogInformation(
                "TableFulltextPopulateStatus: {Status}",
                tableFulltextPopulateStatus);

            if (stopwatch.Elapsed >= timeout)
            {
                logger.LogError("Timeout ({Timeout}) waiting for FTS index on table '{TableName}'. Last status: {Status}", timeout, tableName, tableFulltextPopulateStatus?.ToString(CultureInfo.InvariantCulture) ?? "NULL");
                throw new TimeoutException($"Timeout waiting for Full-Text index on table '{tableName}'. Last status: {tableFulltextPopulateStatus?.ToString(CultureInfo.InvariantCulture) ?? "NULL"}");
            }
        }
    }

    public static async Task WaitForFulltextIndexAsync(
        ILogger logger,
        DbContext dbContext,
        string[] tableNames,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var tableName in tableNames)
        {
            await WaitForFulltextIndexAsync(
                logger,
                dbContext,
                tableName,
                timeout,
                cancellationToken);
        }
    }
}
