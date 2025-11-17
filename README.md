# Backstage Developer Portal with Azure DevOps Integration

This is a Backstage Developer Portal configured with Azure DevOps integration using a Service Principal for authentication.

## Features

- **Azure DevOps Integration**: View pipelines, pull requests, and git tags directly in Backstage
- **Service Principal Authentication**: Secure authentication using Azure AD Service Principal
- **Microsoft Authentication Provider**: Sign in with your Microsoft/Azure AD account
- **Catalog Discovery**: Automatically discover and catalog Azure DevOps repositories
- **CI/CD Visibility**: Monitor build pipelines and deployment status
- **Pull Request Tracking**: View and track pull requests from Azure DevOps

## Prerequisites

- Node.js 18, 20, or 22
- Yarn package manager
- Azure DevOps organization and projects
- Azure AD tenant with permissions to create Service Principals

## Quick Start

### 1. Install Dependencies

```bash
yarn install
```

### 2. Configure Azure DevOps Integration

Follow the detailed setup guide in [docs/azure-devops-setup.md](docs/azure-devops-setup.md) to:
- Create an Azure AD Service Principal
- Grant necessary permissions in Azure DevOps
- Configure environment variables

### 3. Set Up Environment Variables

Copy the example environment file and fill in your values:

```bash
cp .env.example .env
```

Edit `.env` and provide:
- `AZURE_CLIENT_ID`: Your Service Principal's Application (client) ID
- `AZURE_CLIENT_SECRET`: Your Service Principal's client secret
- `AZURE_TENANT_ID`: Your Azure AD Tenant ID
- `AZURE_ORG`: Your Azure DevOps organization name

### 4. Start the Application

Development mode (runs both frontend and backend):
```bash
yarn dev
```

Or start them separately:
```bash
# Terminal 1 - Backend
yarn start-backend

# Terminal 2 - Frontend
yarn start
```

The application will be available at:
- Frontend: http://localhost:3000
- Backend: http://localhost:7007

## Project Structure

```
.
├── app-config.yaml              # Main configuration file
├── packages/
│   ├── app/                     # Frontend React application
│   │   ├── src/
│   │   │   ├── App.tsx
│   │   │   ├── components/
│   │   │   │   ├── catalog/    # Catalog entity pages with Azure DevOps integration
│   │   │   │   ├── Root/        # Navigation and layout
│   │   │   │   └── search/      # Search functionality
│   │   │   └── apis.ts
│   │   └── package.json
│   └── backend/                 # Backend Node.js application
│       ├── src/
│       │   └── index.ts         # Backend plugin configuration
│       └── package.json
├── examples/
│   └── entities.yaml            # Example catalog entities
└── docs/
    └── azure-devops-setup.md    # Detailed Azure DevOps setup guide
```

## Azure DevOps Integration Details

### Enabled Features

1. **Azure Pipelines**: View build and release pipelines
2. **Pull Requests**: Track PR status and activity
3. **Git Tags**: View repository tags
4. **Repository Information**: Display repo metadata and links

### Required Annotations

To enable Azure DevOps features for a component, add these annotations to your `catalog-info.yaml`:

```yaml
apiVersion: backstage.io/v1alpha1
kind: Component
metadata:
  name: my-component
  annotations:
    dev.azure.com/project-repo: PROJECT_NAME/REPO_NAME
    dev.azure.com/build-definition: BUILD_DEFINITION_NAME
spec:
  type: service
  lifecycle: production
  owner: team-name
```

### Supported Annotations

- `dev.azure.com/project-repo`: Format `PROJECT/REPO` - Links to Azure DevOps repository
- `dev.azure.com/build-definition`: Build pipeline name or ID
- `dev.azure.com/project`: Azure DevOps project name
- `dev.azure.com/repo-id`: Repository ID (alternative to project-repo)

## Configuration

### Main Configuration File

The `app-config.yaml` file contains the main configuration:

```yaml
integrations:
  azure:
    - host: dev.azure.com
      credentials:
        - clientId: ${AZURE_CLIENT_ID}
          clientSecret: ${AZURE_CLIENT_SECRET}
          tenantId: ${AZURE_TENANT_ID}

azureDevOps:
  host: dev.azure.com
  token: ${AZURE_TOKEN}
  organization: ${AZURE_ORG}

catalog:
  providers:
    azureDevOps:
      yourProviderId:
        host: dev.azure.com
        organization: ${AZURE_ORG}
        project: '*'
```

### Local Development

For local development overrides, copy and customize `app-config.local.yaml`:

```bash
cp app-config.local.yaml app-config.local.dev.yaml
```

This file is gitignored and safe for local credentials.

## Building for Production

### Build Backend

```bash
yarn build:backend
```

### Build Docker Image

```bash
yarn build-image
```

This creates a Docker image named `backstage` that you can deploy.

## Security Considerations

- **Never commit secrets**: The `.gitignore` file excludes `.env` and `app-config.local.yaml`
- **Use environment variables**: All sensitive data should be provided via environment variables
- **Service Principal permissions**: Grant only the minimum required permissions
- **Rotate secrets regularly**: Update Service Principal secrets periodically

## Troubleshooting

### Authentication Issues

If you encounter authentication errors:

1. Verify your Service Principal credentials are correct
2. Ensure the Service Principal has the necessary permissions in Azure DevOps
3. Check that the Azure AD tenant ID matches your organization
4. Verify the Service Principal is added to your Azure DevOps organization

### Azure DevOps Integration Not Working

1. Check the annotations on your catalog entities
2. Verify the project and repository names are correct
3. Ensure the Service Principal has read access to the repositories
4. Check backend logs for specific error messages

### General Issues

```bash
# Clear node modules and reinstall
rm -rf node_modules packages/*/node_modules
yarn install

# Check for TypeScript errors
yarn tsc:full

# Run linter
yarn lint:all
```

## Additional Resources

- [Backstage Documentation](https://backstage.io/docs)
- [Azure DevOps Plugin Documentation](https://github.com/backstage/backstage/tree/master/plugins/azure-devops)
- [Azure AD Service Principal Guide](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal)
- [Azure DevOps REST API](https://docs.microsoft.com/en-us/rest/api/azure/devops/)

## Contributing

This is an internal developer portal. For questions or issues, contact the platform team.

## License

Copyright © 2025 Your Organization
