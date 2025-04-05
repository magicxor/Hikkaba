using System.Collections;

namespace Hikkaba.Shared.Extensions;

public static class ArrayExtensions
{
    public static int Compare(this byte[]? b1, byte[]? b2)
    {
        if (b1 == null && b2 == null)
            return 0;
        else if (b1 == null)
            return -1;
        else if (b2 == null)
            return 1;

        return StructuralComparisons.StructuralComparer.Compare(b1, b2);
    }
}
