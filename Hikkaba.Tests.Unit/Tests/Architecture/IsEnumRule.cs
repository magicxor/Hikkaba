using Mono.Cecil;
using NetArchTest.Rules;

namespace Hikkaba.Tests.Unit.Tests.Architecture;

public class IsEnumRule : ICustomRule
{
    public bool MeetsRule(TypeDefinition type)
    {
        return type.IsEnum;
    }
}
