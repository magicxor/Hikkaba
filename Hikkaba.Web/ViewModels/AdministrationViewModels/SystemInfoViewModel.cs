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

    [Display(Name = @"Working set")]
    public required long WorkingSet64 { get; set; }

    [Display(Name = @"Private memory size")]
    public required long PrivateMemorySize64 { get; set; }

    [Display(Name = @"Paged memory size")]
    public required long PagedMemorySize64 { get; set; }

    [Display(Name = @"Peak working set")]
    public required long PeakWorkingSet64 { get; set; }

    [Display(Name = @"Peak paged memory size")]
    public required long PeakPagedMemorySize64 { get; set; }

    [Display(Name = @"Processor count")]
    public required int ProcessorCount { get; set; }

    [Display(Name = @"User name")]
    public required string UserName { get; set; }
}
