﻿namespace Hikkaba.Infrastructure.Models.Ban;

public enum BanCreationPrerequisiteStatus
{
    Success,
    PostNotFound,
    IpAddressIsLocalOrPrivate,
    IpAddressNull,
    ActiveBanFound,
}
