﻿using System;

namespace Hikkaba.Shared.Exceptions;

public class HikkabaDomainException : Exception
{
    public HikkabaDomainException()
    {
    }

    public HikkabaDomainException(string message)
        : base(message)
    {
    }

    public HikkabaDomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
