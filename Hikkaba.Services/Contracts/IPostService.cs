using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Models.Dto.Post;
using Hikkaba.Models.Enums;
using Hikkaba.Services.Implementations.Generic;

namespace Hikkaba.Services.Contracts;

public interface IPostService
{
    Task<PostDto> GetAsync(TPrimaryKey id);

    Task<IList<PostDto>> ListAsync<TOrderKey>(
        Expression<Func<Post, bool>> where = null,
        Expression<Func<Post, TOrderKey>> orderBy = null,
        bool isDescending = false);

    Task<BasePagedList<PostDto>> PagedListAsync<TOrderKey>(
        Expression<Func<Post, bool>> where = null,
        Expression<Func<Post, TOrderKey>> orderBy = null,
        AdditionalRecordType additionalRecordType = AdditionalRecordType.None,
        bool isDescending = false,
        PageDto page = null);

    Task EditAsync(PostDto dto);

    Task SetIsDeletedAsync(TPrimaryKey id, bool newValue);
}