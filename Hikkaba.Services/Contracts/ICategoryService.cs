using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Implementations.Generic;

namespace Hikkaba.Services.Contracts;

public interface ICategoryService
{
    Task<IList<CategoryDto>> ListAsync<TOrderKey>(
        Expression<Func<Category, bool>> where = null,
        Expression<Func<Category, TOrderKey>> orderBy = null,
        bool isDescending = false);

    Task<BasePagedList<CategoryDto>> PagedListAsync<TOrderKey>(
        Expression<Func<Category, bool>> where = null,
        Expression<Func<Category, TOrderKey>> orderBy = null, bool isDescending = false,
        PageDto page = null);

    Task<CategoryDto> GetAsync(TPrimaryKey id);

    Task<CategoryDto> GetAsync(string alias);

    Task<TPrimaryKey> CreateAsync(CategoryDto dto);

    Task EditAsync(CategoryDto dto);

    Task SetIsDeletedAsync(TPrimaryKey id, bool newValue);
}