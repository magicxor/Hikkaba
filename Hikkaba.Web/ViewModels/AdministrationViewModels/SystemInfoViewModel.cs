using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Web.ViewModels.AdministrationViewModels;

public class SystemInfoViewModel
{
    [Display(Name = @"Database provider")]
    public string DatabaseProvider { get; set; }

    [Display(Name = @"Framework description")]
    public string FrameworkDescription { get; set; }

    [Display(Name = @"OS architecture")]
    public string OsArchitecture { get; set; }

    [Display(Name = @"OS description")]
    public string OsDescription { get; set; }

    [Display(Name = @"Process architecture")]
    public string ProcessArchitecture { get; set; }

    [Display(Name = @"OS platform")]
    public string OsPlatform { get; set; }

    [Display(Name = @"Memory usage")]
    public string MemoryUsage { get; set; }

    [Display(Name = @"Processor count")]
    public int ProcessorCount { get; set; }

    [Display(Name = @"User name")]
    public string UserName { get; set; }
}