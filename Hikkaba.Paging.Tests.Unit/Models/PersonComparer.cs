namespace Hikkaba.Paging.Tests.Unit.Models;

internal sealed class PersonComparer : IComparer<Person>
{
    public int Compare(Person? x, Person? y)
    {
        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null)
        {
            return -1;
        }

        if (y == null)
        {
            return 1;
        }

        if (x == y)
        {
            return 0;
        }

        if (x.PersonId == y.PersonId
            && x.Name == y.Name
            && x.Age == y.Age
            && x.Salary == y.Salary)
        {
            return 0;
        }

        return -1;
    }
}
