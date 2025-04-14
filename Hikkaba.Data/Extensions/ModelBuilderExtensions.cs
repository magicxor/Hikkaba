using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using Hikkaba.Data.EfFunctions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hikkaba.Data.Extensions;

public static class ModelBuilderExtensions
{
    private static readonly MethodInfo CoalesceInfo = typeof(EfFunc).GetMethod(nameof(EfFunc.Coalesce), [typeof(string), typeof(string)])
                                                      ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.Coalesce));

    private static readonly MethodInfo NullIfInfo = typeof(EfFunc).GetMethod(nameof(EfFunc.NullIf), [typeof(string), typeof(string)])
                                                ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.NullIf));

    private static readonly MethodInfo ConcatWs2Info = typeof(EfFunc).GetMethod(nameof(EfFunc.ConcatWs), [typeof(string), typeof(string), typeof(string)])
                                                       ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.ConcatWs));

    private static readonly MethodInfo ConcatWs3Info = typeof(EfFunc).GetMethod(nameof(EfFunc.ConcatWs), [typeof(string), typeof(string), typeof(string), typeof(string)])
                                                       ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.ConcatWs));

    private static readonly MethodInfo ConcatWs4Info = typeof(EfFunc).GetMethod(nameof(EfFunc.ConcatWs), [typeof(string), typeof(string), typeof(string), typeof(string), typeof(string)])
                                                       ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.ConcatWs));

    private static readonly MethodInfo ConcatWs5Info = typeof(EfFunc).GetMethod(nameof(EfFunc.ConcatWs), [typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string)])
                                                       ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.ConcatWs));

    private static readonly MethodInfo ConcatWs6Info = typeof(EfFunc).GetMethod(nameof(EfFunc.ConcatWs), [typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string)])
                                                       ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.ConcatWs));

    private static readonly MethodInfo ConcatWs7Info = typeof(EfFunc).GetMethod(nameof(EfFunc.ConcatWs), [typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string)])
                                                       ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.ConcatWs));

    private static readonly MethodInfo ConcatWs8Info = typeof(EfFunc).GetMethod(nameof(EfFunc.ConcatWs), [typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string)])
                                                       ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.ConcatWs));

    private static readonly MethodInfo ReverseInfo = typeof(EfFunc).GetMethod(nameof(EfFunc.Reverse), [typeof(string)])
                                                    ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.Reverse));

    private static readonly MethodInfo LeftInfo = typeof(EfFunc).GetMethod(nameof(EfFunc.Left), [typeof(string), typeof(int)])
                                                  ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.Left));

    private static readonly MethodInfo RightInfo = typeof(EfFunc).GetMethod(nameof(EfFunc.Right), [typeof(string), typeof(int)])
                                                   ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.Right));

    private static readonly MethodInfo IsNumericInfo = typeof(EfFunc).GetMethod(nameof(EfFunc.IsNumeric), [typeof(string)])
                                                      ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.IsNumeric));

    private static readonly MethodInfo TryConvertStrToIntInfo = typeof(EfFunc).GetMethod(nameof(EfFunc.TryConvertStrToInt), [typeof(string)])
                                                  ?? throw new MissingMethodException(nameof(EfFunc), nameof(EfFunc.TryConvertStrToInt));

    private static readonly RelationalTypeMapping StringMapping = new StringTypeMapping("NVARCHAR(MAX)", DbType.String, true);

    private static readonly RelationalTypeMapping IntMapping = new IntTypeMapping("INT", DbType.Int32);

    private static readonly SqlConstantExpression EmptyStringConstant = new(string.Empty, StringMapping);

    private static readonly IEnumerable<bool> CoalesceArgumentsPropagateNullability = [false, false];

    private static SqlFunctionExpression EnsureNotNull(SqlExpression arg)
    {
        return new SqlFunctionExpression("COALESCE",
            [arg, EmptyStringConstant],
            nullable: false,
            argumentsPropagateNullability: CoalesceArgumentsPropagateNullability,
            CoalesceInfo.ReturnType,
            StringMapping);
    }

    private static readonly IEnumerable<bool> NullIfArgumentsPropagateNullability = [false, false];

    private static SqlFunctionExpression SetNullIfEmpty(SqlExpression arg)
    {
        return new SqlFunctionExpression("NULLIF",
            [EnsureNotNull(arg), EmptyStringConstant],
            nullable: true,
            argumentsPropagateNullability: NullIfArgumentsPropagateNullability,
            NullIfInfo.ReturnType,
            StringMapping);
    }

    private static ReadOnlyCollection<SqlExpression> TransformConcatWsArgs(IReadOnlyList<SqlExpression> args)
    {
        return args
            .Take(1)
            .Union(args.Skip(1).Select(SetNullIfEmpty))
            .ToList()
            .AsReadOnly();
    }

    private static readonly IEnumerable<bool> ConcatWsArgumentsPropagateNullability2 = [false, false, false];
    private static readonly IEnumerable<bool> ConcatWsArgumentsPropagateNullability3 = [false, false, false, false];
    private static readonly IEnumerable<bool> ConcatWsArgumentsPropagateNullability4 = [false, false, false, false, false];
    private static readonly IEnumerable<bool> ConcatWsArgumentsPropagateNullability5 = [false, false, false, false, false, false];
    private static readonly IEnumerable<bool> ConcatWsArgumentsPropagateNullability6 = [false, false, false, false, false, false, false];
    private static readonly IEnumerable<bool> ConcatWsArgumentsPropagateNullability7 = [false, false, false, false, false, false, false, false];
    private static readonly IEnumerable<bool> ConcatWsArgumentsPropagateNullability8 = [false, false, false, false, false, false, false, false, false];

    public static void AddEfFunctions(this ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(ConcatWs2Info)
            .HasTranslation(args => new SqlFunctionExpression("CONCAT_WS", TransformConcatWsArgs(args), nullable: true, argumentsPropagateNullability: ConcatWsArgumentsPropagateNullability2, ConcatWs2Info.ReturnType, StringMapping));
        modelBuilder.HasDbFunction(ConcatWs3Info)
            .HasTranslation(args => new SqlFunctionExpression("CONCAT_WS", TransformConcatWsArgs(args), nullable: true, argumentsPropagateNullability: ConcatWsArgumentsPropagateNullability3, ConcatWs3Info.ReturnType, StringMapping));
        modelBuilder.HasDbFunction(ConcatWs4Info)
            .HasTranslation(args => new SqlFunctionExpression("CONCAT_WS", TransformConcatWsArgs(args), nullable: true, argumentsPropagateNullability: ConcatWsArgumentsPropagateNullability4, ConcatWs4Info.ReturnType, StringMapping));
        modelBuilder.HasDbFunction(ConcatWs5Info)
            .HasTranslation(args => new SqlFunctionExpression("CONCAT_WS", TransformConcatWsArgs(args), nullable: true, argumentsPropagateNullability: ConcatWsArgumentsPropagateNullability5, ConcatWs5Info.ReturnType, StringMapping));
        modelBuilder.HasDbFunction(ConcatWs6Info)
            .HasTranslation(args => new SqlFunctionExpression("CONCAT_WS", TransformConcatWsArgs(args), nullable: true, argumentsPropagateNullability: ConcatWsArgumentsPropagateNullability6, ConcatWs6Info.ReturnType, StringMapping));
        modelBuilder.HasDbFunction(ConcatWs7Info)
            .HasTranslation(args => new SqlFunctionExpression("CONCAT_WS", TransformConcatWsArgs(args), nullable: true, argumentsPropagateNullability: ConcatWsArgumentsPropagateNullability7, ConcatWs7Info.ReturnType, StringMapping));
        modelBuilder.HasDbFunction(ConcatWs8Info)
            .HasTranslation(args => new SqlFunctionExpression("CONCAT_WS", TransformConcatWsArgs(args), nullable: true, argumentsPropagateNullability: ConcatWsArgumentsPropagateNullability8, ConcatWs8Info.ReturnType, StringMapping));

        modelBuilder.HasDbFunction(ReverseInfo)
            .HasTranslation(args => new SqlFunctionExpression("REVERSE", args, nullable: true, argumentsPropagateNullability: [true], ReverseInfo.ReturnType, StringMapping));

        modelBuilder.HasDbFunction(LeftInfo)
            .HasTranslation(args => new SqlFunctionExpression("LEFT", args, nullable: true, argumentsPropagateNullability: [true, false], LeftInfo.ReturnType, StringMapping));

        modelBuilder.HasDbFunction(RightInfo)
            .HasTranslation(args => new SqlFunctionExpression("RIGHT", args, nullable: true, argumentsPropagateNullability: [true, false], RightInfo.ReturnType, StringMapping));

        modelBuilder.HasDbFunction(IsNumericInfo)
            .HasTranslation(args => new SqlFunctionExpression("ISNUMERIC", args, nullable: false, argumentsPropagateNullability: [false], IsNumericInfo.ReturnType, IntMapping));

        modelBuilder.HasDbFunction(TryConvertStrToIntInfo)
            .HasTranslation(args =>
                new SqlFunctionExpression(
                    functionName: "TRY_CONVERT",
                    arguments: args.Prepend(new SqlFragmentExpression("int")),
                    nullable: true,
                    argumentsPropagateNullability: [false, true],
                    type: typeof(int),
                    typeMapping: IntMapping
                )
            );
    }
}
