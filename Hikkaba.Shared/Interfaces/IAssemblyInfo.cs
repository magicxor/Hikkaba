namespace Hikkaba.Shared.Interfaces;

public interface IAssemblyInfo
{
    /// <summary>
    /// Assembly hierarchy level in this solution.
    /// From lowest (0 = shared code and libraries) to highest (presentation layer).
    /// </summary>
    public int HierarchyLevel { get; }
}
