using System.ComponentModel;

namespace Hikkaba.Paging.Tests.Unit.TypeDescriptorContexts;

public class TestTypeDescriptorContext : ITypeDescriptorContext
{
    public TestTypeDescriptorContext(object instance, string propertyName)
    {
        Instance = instance;
        PropertyDescriptor = TypeDescriptor.GetProperties(instance)[propertyName]
                             ?? throw new ArgumentException(
                                 $"Property {propertyName} not found on {instance?.GetType().Name}",
                                 nameof(propertyName));
    }

    public object Instance { get; private set; }
    public PropertyDescriptor PropertyDescriptor { get; private set; }
    public IContainer Container { get; private set; } = new Container();

    public void OnComponentChanged()
    {
    }

    public bool OnComponentChanging()
    {
        return true;
    }

    public object? GetService(Type serviceType)
    {
        return null;
    }
}
