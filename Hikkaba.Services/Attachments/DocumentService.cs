using System.Linq;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Models.Dto.Attachments;
using Hikkaba.Services.Base.Concrete;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Services.Attachments
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
    }
}