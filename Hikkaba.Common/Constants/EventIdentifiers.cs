namespace Hikkaba.Common.Constants;

public enum EventIdentifiers
{
    // Http
    HttpNotFound       = 404,
    HttpInternalError  = 500,

    // System
    SystemAccessDenied     = 10_403_0,
    SystemBanned           = 10_403_1,        
    SystemEntityNotFound   = 10_404_0,
    SystemTooManyRequests  = 10_429_0,
    SystemFailedDependency = 10_424_0,
    SystemUnavailableForLegalReasons = 10_451_0,
    SystemNotImplemented = 10_501_0,
    SystemMaintenance    = 10_503_0,
        
    // Thread
    ThreadCreateError = 11_000_1,
    ThreadEditError = 11_000_2,
    ThreadDeleteError = 11_000_2,
        
    // Post
    PostCreateError = 12_000_1,
    PostEditError = 12_000_2,
    PostDeleteError = 12_000_2,
}