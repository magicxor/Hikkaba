namespace Hikkaba.Common.Constants;

public enum EventIdentifiers
{
    // Http
    HttpNotFound       = Defaults.HikkabaStartEventId + 404,
    HttpInternalError  = Defaults.HikkabaStartEventId + 500,

    // System
    SystemAccessDenied     = Defaults.HikkabaStartEventId + 1001,
    SystemBanned           = Defaults.HikkabaStartEventId + 1002,
    SystemEntityNotFound   = Defaults.HikkabaStartEventId + 1003,
    SystemTooManyRequests  = Defaults.HikkabaStartEventId + 1004,
    SystemFailedDependency = Defaults.HikkabaStartEventId + 1005,
    SystemUnavailableForLegalReasons = Defaults.HikkabaStartEventId + 1006,
    SystemNotImplemented = Defaults.HikkabaStartEventId + 1007,
    SystemMaintenance    = Defaults.HikkabaStartEventId + 1008,

    // Thread
    ThreadCreateError = Defaults.HikkabaStartEventId + 1101,
    ThreadEditError = Defaults.HikkabaStartEventId + 1102,
    ThreadDeleteError = Defaults.HikkabaStartEventId + 1103,

    // Post
    PostCreateError = Defaults.HikkabaStartEventId + 1201,
    PostEditError = Defaults.HikkabaStartEventId + 1202,
    PostDeleteError = Defaults.HikkabaStartEventId + 1203,
}
