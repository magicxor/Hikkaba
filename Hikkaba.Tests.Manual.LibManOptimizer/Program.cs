using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Hikkaba.Tests.Manual.LibManOptimizer;

[SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "This is a utility application.")]
[SuppressMessage("CodeSmell", "EPC12:Suspicious exception handling: only the \'Message\' property is observed in the catch block", Justification = "This is a utility application.")]
internal static class Program
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        // Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true, // Make the output readable (pretty-print)
        // Ensure characters like '/' in paths are not escaped
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static void Main(string[] args)
    {
        // Specify the input and output file paths
        string inputFilePath = "libman.json"; // Assume the file is in the same directory as the executable
        string outputFilePath = "libman_modified.json"; // Save changes to a new file

        try
        {
            // --- 1. Read the JSON file ---
            string jsonString = File.ReadAllText(inputFilePath);

            // --- 2. Deserialize the JSON into C# objects ---
            // Allow case-insensitive property matching
            LibManConfig? config = JsonSerializer.Deserialize<LibManConfig>(jsonString, JsonSerializerOptions);

            if (config?.Libraries == null)
            {
                Console.WriteLine(@"Error: Could not parse libraries from the JSON file.");
                return;
            }

            // --- 3. Process each library's files ---
            foreach (var library in config.Libraries)
            {
                if (library.Files == null || !library.Files.Any())
                {
                    // Skip libraries with no files
                    continue;
                }

                // Create a HashSet for efficient lookup of existing files
                var fileSet = new HashSet<string>(library.Files);
                var filesToKeep = new List<string>();

                foreach (var currentFile in library.Files)
                {
                    bool keepFile = true;
                    string? minifiedVersion = null;

                    // --- Check for non-minified .js files ---
                    if (currentFile.EndsWith(".js", StringComparison.OrdinalIgnoreCase) && !currentFile.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase))
                    {
                        minifiedVersion = currentFile.Replace(".js", ".min.js", StringComparison.OrdinalIgnoreCase);
                    }
                    // --- Check for non-minified .css files ---
                    else if (currentFile.EndsWith(".css", StringComparison.OrdinalIgnoreCase) && !currentFile.EndsWith(".min.css", StringComparison.OrdinalIgnoreCase))
                    {
                        minifiedVersion = currentFile.Replace(".css", ".min.css", StringComparison.OrdinalIgnoreCase);
                    }
                     // --- Check for non-minified .js.map files ---
                    else if (currentFile.EndsWith(".js.map", StringComparison.OrdinalIgnoreCase) && !currentFile.EndsWith(".min.js.map", StringComparison.OrdinalIgnoreCase))
                    {
                        minifiedVersion = currentFile.Replace(".js.map", ".min.js.map", StringComparison.OrdinalIgnoreCase);
                    }
                    // --- Check for non-minified .css.map files ---
                     else if (currentFile.EndsWith(".css.map", StringComparison.OrdinalIgnoreCase) && !currentFile.EndsWith(".min.css.map", StringComparison.OrdinalIgnoreCase))
                    {
                        minifiedVersion = currentFile.Replace(".css.map", ".min.css.map", StringComparison.OrdinalIgnoreCase);
                    }

                    // If we identified a potential minified version, check if it actually exists
                    if (minifiedVersion != null && fileSet.Contains(minifiedVersion))
                    {
                        // If the minified version exists, mark the non-minified one for removal (i.e., don't keep it)
                        keepFile = false;
                        // Console.WriteLine($"Removing '{currentFile}' because '{minifiedVersion}' exists."); // Optional: Log removals
                    }

                    if (keepFile)
                    {
                        filesToKeep.Add(currentFile);
                    }
                }

                // Replace the old file list with the filtered list
                library.Files = filesToKeep;
            }

            // --- 4. Serialize the modified C# object back to JSON ---
            string modifiedJsonString = JsonSerializer.Serialize(config, WriteOptions);

            // --- 5. Save the modified JSON to the output file ---
            File.WriteAllText(outputFilePath, modifiedJsonString);

            Console.WriteLine($"Successfully processed the file. Modified JSON saved to '{outputFilePath}'");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Error: Input file not found at '{inputFilePath}'");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error parsing JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
}
