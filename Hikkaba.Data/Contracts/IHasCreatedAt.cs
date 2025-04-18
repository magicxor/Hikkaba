using System;

namespace Hikkaba.Data.Contracts;

public interface IHasCreatedAt
{
    DateTime CreatedAt { get; set; }
}
