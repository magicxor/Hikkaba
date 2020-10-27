using System.Runtime.InteropServices;

namespace Hikkaba.Web.ViewModels.AdministrationViewModels
{
    public class SystemInfoDto
    {
        public string DatabaseProvider { get; set; }
        public string FrameworkDescription { get; set; }
        public Architecture OsArchitecture { get; set; }
        public string OsDescription { get; set; }
        public Architecture ProcessArchitecture { get; set; }
        public string OsPlatform { get; set; }
        public long MemoryUsage { get; set; }
    }
}
