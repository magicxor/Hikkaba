using System;
using Hikkaba.Models.Dto.Attachments;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments
{
    public class DocumentViewModel : DocumentDto
    {
        public Guid ThreadId { get; set; }
    }
}