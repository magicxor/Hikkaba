using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.AdministrationViewModels;

public sealed class SystemInfoViewModel
{
    [Display(Name = @"Database provider")]
    public required string DatabaseProvider { get; set; }

    [Display(Name = @"Framework description")]
    public required string FrameworkDescription { get; set; }

    [Display(Name = @"OS architecture")]
    public required string OsArchitecture { get; set; }

    [Display(Name = @"OS description")]
    public required string OsDescription { get; set; }

    [Display(Name = @"Process architecture")]
    public required string ProcessArchitecture { get; set; }

    [Display(Name = @"OS platform")]
    public required string OsPlatform { get; set; }

    [Display(Name = @"Memory usage")]
    public required string MemoryUsage { get; set; }

    [Display(Name = @"Processor count")]
    public required int ProcessorCount { get; set; }

    [Display(Name = @"User name")]
    public required string UserName { get; set; }
}
