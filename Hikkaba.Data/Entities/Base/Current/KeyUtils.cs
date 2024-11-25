using System;

namespace Hikkaba.Data.Entities.Base.Current;

public static class KeyUtils
{
    public static TPrimaryKey GenerateNew()
    {
        return Guid.NewGuid();
    }
}