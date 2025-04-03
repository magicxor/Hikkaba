using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Paging.Tests.Unit.Models;

public class PersonWithEmploymentDate
{
    [Key]
    public int PersonId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly? EmployedOn { get; set; }
}
