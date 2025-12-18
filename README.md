# Backstage Developer Portal Demo

This is a demo project for the blog article: [Platform Engineering with AI](https://blog.bgener.nl) - Building an AI-powered internal developer portal using Backstage, .NET Core, and Ollama.

## What's Included

This project demonstrates:

- **ServiceA**: A .NET Web API project (scaffolded example)
- **ServiceB**: A .NET MVC project (scaffolded example)
- **ProjectSummarizer**: A .NET CLI tool that uses AI (via Ollama) to generate service summaries and create a Backstage catalog file

## Prerequisites

Before you begin, install the following:

- [.NET SDK 8+](https://dotnet.microsoft.com/en-us/download)
- [Node.js](https://nodejs.org/) (includes `npx`)
- [Ollama](https://ollama.com/)

## Setup Instructions

### 1. Verify Your Setup

```bash
dotnet --version
node --version
npx --version
ollama --version
```

### 2. Download and Run the Ollama Model

```bash
ollama pull llama3
ollama run llama3
```

Make sure Ollama is running on `http://localhost:11434` before proceeding.

### 3. Clone This Repository

```bash
git clone https://github.com/bgener/Backstage-Dev-Portal.git
cd Backstage-Dev-Portal
```

### 4. Build the Solution

```bash
dotnet build
```

### 5. Run the ProjectSummarizer CLI Tool

This will scan the .NET projects, generate AI summaries, and create a `catalog-info.yaml` file:

```bash
dotnet run --project ProjectSummarizer -- .
```

You should see AI-generated summaries for ServiceA and ServiceB in the console output, and a `catalog-info.yaml` file will be created in the root directory.

### 6. Setup Backstage (Optional)

To view the generated catalog in a Backstage portal:

```bash
npx @backstage/create-app
# Follow prompts and name it 'dev-portal'
```

After creating the Backstage app:

1. Copy the generated `catalog-info.yaml` to the Backstage app directory
2. Register the catalog file in Backstage
3. Run the portal:

```bash
cd dev-portal
yarn dev
```

Navigate to `http://localhost:3000` to view your services in the Backstage portal.

## How It Works

1. **ProjectSummarizer** scans all `.csproj` files in the specified directory
2. For each project, it collects:
   - Project folder structure
   - `.csproj` file content
   - `Program.cs` file content
3. Sends this information to Ollama (running locally) to generate a concise summary
4. Generates a `catalog-info.yaml` file with all services in Backstage format

## Customization

- **Change the AI Model**: Edit `Program.cs` in ProjectSummarizer and change `llama3` to another model
- **Modify the Prompt**: Adjust the prompt in `Program.cs` to get different style summaries
- **Add More Services**: Simply add more .NET projects to the solution and run the tool again

## Next Steps

- Add more metadata to the catalog (owners, tags, links)
- Integrate with your CI/CD pipeline
- Deploy Backstage to Netlify (see the blog article for details)
- Add API documentation to the catalog
- Customize Backstage with plugins

## Learn More

Read the full tutorial at: [blog.bgener.nl](https://blog.bgener.nl)

## License

MIT
