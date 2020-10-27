namespace Hikkaba.Web.ViewModels.AdministrationViewModels
{
    public class SystemInfoViewModel
    {
        public string DatabaseProvider { get; set; }
        public string FrameworkDescription { get; set; }
        public string OsArchitecture { get; set; }
        public string OsDescription { get; set; }
        public string ProcessArchitecture { get; set; }
        public string OsPlatform { get; set; }
        public string MemoryUsage { get; set; }
    }
}
