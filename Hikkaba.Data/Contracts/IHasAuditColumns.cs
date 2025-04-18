namespace Hikkaba.Data.Contracts;

public interface IHasAuditColumns : IHasCreatedAt, IHasCreatedById, IHasModifiedAt, IHasModifiedById
{
}
