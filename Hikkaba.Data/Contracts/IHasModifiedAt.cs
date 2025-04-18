using System;

namespace Hikkaba.Data.Contracts;

public interface IHasModifiedAt
{
    DateTime? ModifiedAt { get; set; }
}
