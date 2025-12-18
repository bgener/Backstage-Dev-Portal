using OllamaSharp;
using System.Text;

string sourceCodePath = Path.GetFullPath(args[0]); // or hardcode the directory path
Console.WriteLine($"Scanning files in {sourceCodePath}...");

var projectFiles = Directory.GetFiles(sourceCodePath, "*.csproj", SearchOption.AllDirectories);

var uri = new Uri("http://localhost:11434");
var ollamaApiClient = new OllamaApiClient(uri) { SelectedModel = "llama3" };
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

Console.WriteLine($"\n\nCatalog file generated at: {rootCatalogPath}");

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
