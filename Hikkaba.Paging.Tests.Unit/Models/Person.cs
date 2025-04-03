using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Paging.Tests.Unit.Models;

public class Person
{
    [Key]
    public int PersonId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public int Age { get; set; }

    public int Salary { get; set; }

    public sbyte? Grade { get; set; }
}
