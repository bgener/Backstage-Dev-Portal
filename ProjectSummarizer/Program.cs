using OllamaSharp;
using System.Text;

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run --project ProjectSummarizer -- <path-to-scan>");
    Console.WriteLine("Example: dotnet run --project ProjectSummarizer -- .");
    return;
}

string sourceCodePath = Path.GetFullPath(args[0]);
Console.WriteLine($"Scanning files in {sourceCodePath}...");

var projectFiles = Directory.GetFiles(sourceCodePath, "*.csproj", SearchOption.AllDirectories)
    .Where(p => !p.Contains("ProjectSummarizer")) // Exclude the CLI tool itself
    .ToArray();

if (projectFiles.Length == 0)
{
    Console.WriteLine("No .NET projects found to scan.");
    return;
}

Console.WriteLine($"Found {projectFiles.Length} project(s) to analyze.\n");

var uri = new Uri("http://localhost:11434");
var ollamaApiClient = new OllamaApiClient(uri) { SelectedModel = "llama3" };

// Check if Ollama is running
try
{
    Console.WriteLine("Checking Ollama connection...");
    var models = await ollamaApiClient.ListLocalModelsAsync();
    if (!models.Any(m => m.Name.Contains("llama3")))
    {
        Console.WriteLine("Warning: llama3 model not found. Please run: ollama pull llama3");
        return;
    }
    Console.WriteLine("✓ Connected to Ollama successfully!\n");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: Cannot connect to Ollama at {uri}");
    Console.WriteLine("Please ensure Ollama is running:");
    Console.WriteLine("  1. Start Ollama: ollama serve (or start the Ollama application)");
    Console.WriteLine("  2. Pull the model: ollama pull llama3");
    Console.WriteLine($"\nDetails: {ex.Message}");
    return;
}

var chat = new Chat(ollamaApiClient);

var catalogEntries = new List<string>();

foreach (var csprojPath in projectFiles)
{
    var sb = new StringBuilder();
    var projectDir = Path.GetDirectoryName(csprojPath)!;
    var projectName = Path.GetFileNameWithoutExtension(csprojPath);

    sb.AppendLine($"Project: {projectName}");
    sb.AppendLine("Folder structure:");
    AppendFolderStructure(projectDir, sb, "");

    sb.AppendLine("\n.csproj content:\n");
    sb.AppendLine(File.ReadAllText(csprojPath));

    var programPath = Directory.GetFiles(projectDir, "Program.cs", SearchOption.AllDirectories).FirstOrDefault();
    if (programPath != null)
    {
        sb.AppendLine("\nProgram.cs content:\n");
        sb.AppendLine(File.ReadAllText(programPath));
    }

    var prompt = "Summarize the project in 1-2 sentences based on the files provided, keep it concise. " +
                 "Do not output anything else. " +
                 "I expect an output similar to the below: " +
                 "- This system exposes REST APIs to provide weather forecasts with temperatures in range 0-100.\n\n" +
                 "- This system represents asp.net mvc with reactjs and bootstrap css framework, allowing to manage todo items.\n\n"
                 + sb.ToString();

    Console.WriteLine($"\n\n==== Summary for {projectName} ====");

    var summaryBuilder = new StringBuilder();
    await foreach (var token in chat.SendAsync(prompt))
    {
        Console.Write(token);
        summaryBuilder.Append(token);
    }

    var summary = summaryBuilder.ToString().Trim();
    Console.WriteLine("\n===============================");

    var yamlEntry = $@"
apiVersion: backstage.io/v1alpha1
kind: Component
metadata:
  name: {projectName.ToLowerInvariant()}
  description: {summary}
spec:
  type: service
  lifecycle: production
  owner: your-team";

    catalogEntries.Add(yamlEntry);
}

string multiDocYaml = string.Join("\n---\n", catalogEntries);
var rootCatalogPath = Path.Combine(sourceCodePath, "catalog-info.yaml");
File.WriteAllText(rootCatalogPath, multiDocYaml);

Console.WriteLine($"\n\n✓ Catalog file generated successfully!");
Console.WriteLine($"  Location: {rootCatalogPath}");
Console.WriteLine($"  Services cataloged: {catalogEntries.Count}");
Console.WriteLine("\nNext steps:");
Console.WriteLine("  1. Review the generated catalog-info.yaml file");
Console.WriteLine("  2. Set up Backstage: npx @backstage/create-app");
Console.WriteLine("  3. Register this catalog file in your Backstage instance");

static void AppendFolderStructure(string rootPath, StringBuilder sb, string indent)
{
    foreach (var dir in Directory.GetDirectories(rootPath))
    {
        var dirName = Path.GetFileName(dir);
        sb.AppendLine($"{indent}- {dirName}/");
        AppendFolderStructure(dir, sb, indent + "  ");
    }

    foreach (var file in Directory.GetFiles(rootPath))
    {
        var fileName = Path.GetFileName(file);
        sb.AppendLine($"{indent}- {fileName}");
    }
}
