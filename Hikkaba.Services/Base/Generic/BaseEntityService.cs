using TPrimaryKey = System.Guid;
using System.Collections.Generic;
using AutoMapper;
using Hikkaba.Data.Entities.Base.Generic;
using Hikkaba.Models.Dto.Base.Generic;

namespace Hikkaba.Services.Base.Generic;

public abstract class BaseEntityService
{
    private readonly IMapper _mapper;

    protected BaseEntityService(IMapper mapper)
    {
        _mapper = mapper;
    }

    protected TDto MapEntityToDto<TDto, TEntity>(TEntity entity)
        where TDto : class, IBaseDto<TPrimaryKey>
        where TEntity : class, IBaseEntity<TPrimaryKey>
    {
        var dto = _mapper.Map<TDto>(entity);
        return dto;
    }

    protected IList<TDto> MapEntityListToDtoList<TDto, TEntity>(IList<TEntity> entityList)
        where TDto : class, IBaseDto<TPrimaryKey>
        where TEntity : class, IBaseEntity<TPrimaryKey>
    {
        var dtoList = _mapper.Map<List<TDto>>(entityList);
        return dtoList;
    }

    protected TEntity MapDtoToNewEntity<TDto, TEntity>(TDto dto)
        where TDto : class, IBaseDto<TPrimaryKey>
        where TEntity : class, IBaseEntity<TPrimaryKey>
    {
        var entity = _mapper.Map<TEntity>(dto);
        entity.Id = entity.GenerateNewId();
        return entity;
    }

    protected void MapDtoToExistingEntity<TDto, TEntity>(TDto dto, TEntity entity)
        where TDto : class, IBaseDto<TPrimaryKey>
        where TEntity : class, IBaseEntity<TPrimaryKey>
    {
        _mapper.Map(dto, entity);
    }
}
