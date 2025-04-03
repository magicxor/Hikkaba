namespace Hikkaba.Paging.Tests.Unit.Models;

public sealed class PersonDto
{
    public int PersonId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public int PersonAge { get; set; }

    public int GrossSalary { get; set; }

    public int NameLength { get; set; }

    public int SalaryDigits { get; set; }
}
