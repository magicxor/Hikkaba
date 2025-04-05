using System;
using System.IO;

namespace Hikkaba.Data.Utils;

public static class MigrationUtility
{
    /// <summary>
    /// Read a SQL script that is embedded into a resource.
    /// </summary>
    /// <param name="migrationType">The migration type the SQL file script is attached to.</param>
    /// <param name="sqlFileName">The embedded SQL file name.</param>
    /// <returns>The content of the SQL file.</returns>
    public static string ReadSql(Type migrationType, string sqlFileName)
    {
        var assembly = migrationType.Assembly;
        var resourceName = $"{migrationType.Namespace}.{sqlFileName}";
        using var manifestResourceStream = assembly.GetManifestResourceStream(resourceName)
                                     ?? throw new FileNotFoundException("Unable to find the SQL file from an embedded resource", resourceName);
        using var reader = new StreamReader(manifestResourceStream);
        var content = reader.ReadToEnd();
        return content;
    }
}
