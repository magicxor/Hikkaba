using System.Linq;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Models.Dto.Attachments;
using Hikkaba.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service.Attachments
{
    public interface IDocumentService : IBaseImmutableEntityService<DocumentDto, Document> { }

    public class DocumentService : BaseImmutableEntityService<DocumentDto, Document>, IDocumentService
    {
        public DocumentService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }

        protected override DbSet<Document> GetDbSet(ApplicationDbContext context)
        {
            return context.Documents;
        }

        protected override IQueryable<Document> GetDbSetWithReferences(ApplicationDbContext context)
        {
            return context.Documents.Include(document => document.Post);
        }
    }
}