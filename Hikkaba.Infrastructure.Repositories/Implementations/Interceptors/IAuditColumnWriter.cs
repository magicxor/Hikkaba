using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Hikkaba.Infrastructure.Repositories.Implementations.Interceptors;

public interface IAuditColumnWriter : ISaveChangesInterceptor
{
}
