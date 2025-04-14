using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Hikkaba.Data.EfFunctions;

public static class EfFunc
{
    // SQL: COALESCE(str1, str2)
    public static string Coalesce(string? str, string reference)
        => str ?? reference;

    // SQL: NULLIF(str1, str2)
    public static string? NullIf(string str, string reference)
        => str == reference ? null : str;

    // SQL: CONCAT_WS(separator, arg1, arg2, ...)
    public static string ConcatWs(string separator, string? arg1, string? arg2)
        => string.Join(separator, new[] { arg1, arg2 }.Where(x => !string.IsNullOrEmpty(x)));
    public static string ConcatWs(string separator, string? arg1, string? arg2, string? arg3)
        => string.Join(separator, new[] { arg1, arg2, arg3 }.Where(x => !string.IsNullOrEmpty(x)));
    public static string ConcatWs(string separator, string? arg1, string? arg2, string? arg3, string? arg4)
        => string.Join(separator, new[] { arg1, arg2, arg3, arg4 }.Where(x => !string.IsNullOrEmpty(x)));
    public static string ConcatWs(string separator, string? arg1, string? arg2, string? arg3, string? arg4, string? arg5)
        => string.Join(separator, new[] { arg1, arg2, arg3, arg4, arg5 }.Where(x => !string.IsNullOrEmpty(x)));
    public static string ConcatWs(string separator, string? arg1, string? arg2, string? arg3, string? arg4, string? arg5, string? arg6)
        => string.Join(separator, new[] { arg1, arg2, arg3, arg4, arg5, arg6 }.Where(x => !string.IsNullOrEmpty(x)));
    public static string ConcatWs(string separator, string? arg1, string? arg2, string? arg3, string? arg4, string? arg5, string? arg6, string? arg7)
        => string.Join(separator, new[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 }.Where(x => !string.IsNullOrEmpty(x)));
    public static string ConcatWs(string separator, string? arg1, string? arg2, string? arg3, string? arg4, string? arg5, string? arg6, string? arg7, string? arg8)
        => string.Join(separator, new[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 }.Where(x => !string.IsNullOrEmpty(x)));

    // SQL: REVERSE(str)
    [return: NotNullIfNotNull(nameof(str))]
    public static string? Reverse(string? str)
        => str == null
            ? null
            : new string(str.Reverse().ToArray());

    // SQL: LEFT(str, length)
    [return: NotNullIfNotNull(nameof(str))]
    public static string? Left(string? str, int length)
        => str == null
            ? null
            : str.Length <= length ? str : str[..length];

    // SQL: RIGHT(str, length)
    [return: NotNullIfNotNull(nameof(str))]
    public static string? Right(string? str, int length)
        => str == null
            ? null
            : str.Length <= length ? str : str[^length..];

    // SQL: ISNUMERIC(str)
    public static int IsNumeric(string str)
        => decimal.TryParse(str, out _) ? 1 : 0;

    // SQL: TRY_CONVERT(type, str) string to int
    public static int? TryConvertStrToInt(string? str)
        => int.TryParse(str, out var result) ? result : null;
}
