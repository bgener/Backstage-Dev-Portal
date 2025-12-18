# Azure DevOps Integration Setup Guide

This guide will walk you through setting up Azure DevOps integration for Backstage using a Service Principal for authentication.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Create Azure AD Service Principal](#create-azure-ad-service-principal)
3. [Configure Azure DevOps Permissions](#configure-azure-devops-permissions)
4. [Generate Client Secret](#generate-client-secret)
5. [Configure Backstage](#configure-backstage)
6. [Test the Integration](#test-the-integration)
7. [Troubleshooting](#troubleshooting)

## Prerequisites

Before you begin, ensure you have:

- Access to an Azure AD tenant
- Permissions to create App Registrations (Service Principals) in Azure AD
- Administrator access to your Azure DevOps organization
- The Azure CLI installed (optional, but recommended)

## Create Azure AD Service Principal

### Option 1: Using Azure Portal

1. **Navigate to Azure Portal**
   - Go to https://portal.azure.com
   - Sign in with your Azure AD account

2. **Create App Registration**
   - Navigate to `Azure Active Directory` > `App registrations`
   - Click `+ New registration`
   - Fill in the details:
     - **Name**: `Backstage-AzureDevOps-Integration`
     - **Supported account types**: Select "Accounts in this organizational directory only"
     - **Redirect URI**: Leave blank for now
   - Click `Register`

3. **Note the Application Details**
   - After creation, you'll see the Overview page
   - **Copy and save these values**:
     - `Application (client) ID` - This is your `AZURE_CLIENT_ID`
     - `Directory (tenant) ID` - This is your `AZURE_TENANT_ID`

### Option 2: Using Azure CLI

```bash
# Login to Azure
az login

# Create the service principal
az ad sp create-for-rbac --name "Backstage-AzureDevOps-Integration" --skip-assignment

# Output will contain:
# {
#   "appId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",        # This is AZURE_CLIENT_ID
#   "displayName": "Backstage-AzureDevOps-Integration",
#   "password": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",         # This is AZURE_CLIENT_SECRET
#   "tenant": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"        # This is AZURE_TENANT_ID
# }
```

## Generate Client Secret

### Using Azure Portal

1. **Navigate to your App Registration**
   - Go to `Azure Active Directory` > `App registrations`
   - Click on `Backstage-AzureDevOps-Integration`

2. **Create a Client Secret**
   - In the left menu, click `Certificates & secrets`
   - Click `+ New client secret`
   - Add a description: `Backstage Integration Secret`
   - Choose an expiration period (recommended: 12-24 months)
   - Click `Add`

3. **Copy the Secret Value**
   - **IMPORTANT**: Copy the secret `Value` immediately
   - This is your `AZURE_CLIENT_SECRET`
   - You won't be able to see it again after leaving this page
   - Store it securely

## Configure Azure DevOps Permissions

### Add Service Principal to Azure DevOps

1. **Access Organization Settings**
   - Go to https://dev.azure.com/{YOUR_ORGANIZATION}
   - Click on `Organization settings` (bottom left)

2. **Add Service Principal as User**
   - Navigate to `Users` in the left sidebar
   - Click `+ Add users`
   - In the "Users or Service Principals" field, paste your Service Principal's `Application (client) ID`
   - Select access level: `Basic` (or `Stakeholder` for read-only)
   - Click `Add`

3. **Grant Project Permissions**
   - Navigate to each project you want to integrate
   - Go to `Project Settings` > `Permissions`
   - Add the Service Principal to the appropriate groups:
     - **Readers**: Minimum required for viewing builds and repositories
     - **Contributors**: Required if you want Backstage to trigger builds

   Or use the Azure DevOps CLI:
   ```bash
   # Install Azure DevOps extension
   az extension add --name azure-devops

   # Set default organization
   az devops configure --defaults organization=https://dev.azure.com/{YOUR_ORGANIZATION}

   # Add service principal to project
   az devops user add --email-id {CLIENT_ID}@{TENANT_ID} \
     --license-type basic \
     --user-type service-principal
   ```

### Grant API Permissions

1. **Navigate to Project Settings**
   - Go to your Azure DevOps project
   - Click `Project Settings` > `Service connections`

2. **Verify Permissions**
   Ensure the Service Principal has at least these permissions:
   - **Code**: Read
   - **Build**: Read
   - **Release**: Read
   - **Graph**: Read (for user information)

### Using Personal Access Token (Alternative)

If you prefer using a Personal Access Token instead of Service Principal:

1. **Create PAT in Azure DevOps**
   - Click on your user icon (top right) > `Personal access tokens`
   - Click `+ New Token`
   - Set the following scopes:
     - **Code**: Read
     - **Build**: Read
     - **Release**: Read
     - **Graph**: Read
     - **Project and Team**: Read
   - Copy the token value

2. **Use in Configuration**
   - Set `AZURE_TOKEN` environment variable with the PAT value
   - You can use this alongside or instead of Service Principal auth

## Configure Backstage

### 1. Set Environment Variables

Create a `.env` file in the root of your Backstage project:

```bash
# Service Principal Credentials
AZURE_CLIENT_ID=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
AZURE_CLIENT_SECRET=your-secret-value-here
AZURE_TENANT_ID=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx

# Azure DevOps Organization
AZURE_ORG=your-organization-name

# Optional: Personal Access Token
AZURE_TOKEN=your-pat-token-here
```

### 2. Verify app-config.yaml

The configuration should already be set up in `app-config.yaml`:

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
        project: '*'  # or specify specific projects: ['project1', 'project2']
```

### 3. Configure Catalog Entities

Update your component's `catalog-info.yaml` with Azure DevOps annotations:

```yaml
apiVersion: backstage.io/v1alpha1
kind: Component
metadata:
  name: my-service
  description: My service component
  annotations:
    # Required: Project and Repository
    dev.azure.com/project-repo: MyProject/MyRepo

    # Optional: Build Definition
    dev.azure.com/build-definition: MyBuildPipeline

    # Alternative format using repo ID
    # dev.azure.com/project: MyProject
    # dev.azure.com/repo-id: abc123-repo-id
spec:
  type: service
  lifecycle: production
  owner: team-name
```

## Test the Integration

### 1. Start Backstage

```bash
# Install dependencies if not already done
yarn install

# Start the development server
yarn dev
```

### 2. Verify Authentication

Check the backend logs for successful authentication:

```bash
# Look for messages like:
[info] Successfully authenticated with Azure DevOps
[info] Azure DevOps provider initialized for organization: your-org
```

### 3. Test in the UI

1. **Navigate to a Component**
   - Go to http://localhost:3000
   - Navigate to the Catalog
   - Select a component with Azure DevOps annotations

2. **Check Azure DevOps Tabs**
   - You should see these tabs (if configured):
     - **CI/CD**: Shows Azure Pipeline builds
     - **Pull Requests**: Shows active and recent PRs
     - **Git Tags**: Shows repository tags

3. **Verify Data Display**
   - Pipeline runs should display with status
   - Pull requests should show with details
   - Links should direct to Azure DevOps

### 4. Test Catalog Discovery

If you configured the Azure DevOps catalog provider:

```bash
# Check backend logs for catalog discovery
[info] Azure DevOps: Discovered 15 repositories
[info] Azure DevOps: Processing repository: MyProject/MyRepo
```

## Troubleshooting

### Common Issues

#### 1. Authentication Failed

**Error**: `Failed to authenticate with Azure DevOps`

**Solutions**:
- Verify `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`, and `AZURE_TENANT_ID` are correct
- Check if the Service Principal secret has expired
- Ensure the Service Principal is not disabled
- Verify the tenant ID matches your organization

#### 2. Permission Denied

**Error**: `Access denied` or `Unauthorized`

**Solutions**:
- Verify the Service Principal is added to your Azure DevOps organization
- Check project-level permissions for the Service Principal
- Ensure the Service Principal has at least "Reader" access
- Verify API permissions are granted

#### 3. No Data Displayed

**Symptoms**: Tabs appear but show no data

**Solutions**:
- Check component annotations are correctly formatted
- Verify project and repository names match exactly (case-sensitive)
- Check backend logs for specific API errors
- Ensure the repository has builds/PRs to display
- Verify the Service Principal has access to the specific project

#### 4. Organization Not Found

**Error**: `Organization 'xxx' not found`

**Solutions**:
- Verify `AZURE_ORG` environment variable is set correctly
- Check the organization name spelling (no URL, just the name)
- Ensure the Service Principal has access to the organization

### Debugging Tips

1. **Enable Debug Logging**

   Add to `app-config.yaml`:
   ```yaml
   backend:
     baseUrl: http://localhost:7007
     listen:
       port: 7007
     # Add this:
     debug: true
   ```

2. **Check Backend Logs**
   ```bash
   yarn start-backend
   # Look for Azure DevOps-related log messages
   ```

3. **Test API Directly**

   Use curl to test Azure DevOps API access:
   ```bash
   # Get access token first (using Service Principal)
   curl -X POST "https://login.microsoftonline.com/${AZURE_TENANT_ID}/oauth2/v2.0/token" \
     -d "client_id=${AZURE_CLIENT_ID}" \
     -d "client_secret=${AZURE_CLIENT_SECRET}" \
     -d "scope=499b84ac-1321-427f-aa17-267ca6975798/.default" \
     -d "grant_type=client_credentials"

   # Use the token to call Azure DevOps API
   curl "https://dev.azure.com/${AZURE_ORG}/_apis/projects?api-version=7.0" \
     -H "Authorization: Bearer ${ACCESS_TOKEN}"
   ```

4. **Verify Service Principal**
   ```bash
   # List service principal details
   az ad sp show --id ${AZURE_CLIENT_ID}

   # Check if service principal is active
   az ad sp list --display-name "Backstage-AzureDevOps-Integration"
   ```

## Security Best Practices

1. **Rotate Secrets Regularly**
   - Set calendar reminders for secret expiration
   - Create new secrets before old ones expire
   - Update environment variables when rotating

2. **Use Minimum Required Permissions**
   - Grant only "Reader" access unless write operations are needed
   - Limit to specific projects instead of organization-wide access
   - Review permissions quarterly

3. **Secure Secret Storage**
   - Never commit `.env` files to version control
   - Use secret management systems (Azure Key Vault, HashiCorp Vault, etc.)
   - Implement proper secret rotation policies

4. **Monitor Access**
   - Regularly review Service Principal usage in Azure AD logs
   - Monitor API calls in Azure DevOps
   - Set up alerts for unusual activity

5. **Environment Separation**
   - Use different Service Principals for dev/staging/production
   - Maintain separate Azure DevOps organizations if needed
   - Implement proper access controls per environment

## Advanced Configuration

### Multiple Azure DevOps Organizations

To integrate multiple organizations:

```yaml
integrations:
  azure:
    - host: dev.azure.com
      credentials:
        - organizations: [org1, org2]
          clientId: ${AZURE_CLIENT_ID}
          clientSecret: ${AZURE_CLIENT_SECRET}
          tenantId: ${AZURE_TENANT_ID}

catalog:
  providers:
    azureDevOps:
      org1Provider:
        host: dev.azure.com
        organization: org1
        project: '*'
      org2Provider:
        host: dev.azure.com
        organization: org2
        project: 'SpecificProject'
```

### Filtering Projects

To limit catalog discovery to specific projects:

```yaml
catalog:
  providers:
    azureDevOps:
      yourProviderId:
        host: dev.azure.com
        organization: ${AZURE_ORG}
        project:
          - ProjectA
          - ProjectB
          - ProjectC
```

### Custom Entity Mapping

You can customize how Azure DevOps repositories are mapped to Backstage entities by extending the catalog processor.

## Next Steps

After successful setup:

1. **Import Repositories**: Use the Catalog Import page to bulk-import repositories
2. **Create Templates**: Set up scaffolder templates for creating new Azure DevOps projects
3. **Configure CI/CD**: Set up automated catalog updates via CI/CD pipelines
4. **Customize UI**: Modify entity pages to show additional Azure DevOps data
5. **Add Monitoring**: Set up alerts and monitoring for the integration

## Additional Resources

- [Backstage Azure DevOps Plugin Documentation](https://github.com/backstage/backstage/tree/master/plugins/azure-devops)
- [Azure AD Service Principal Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/app-objects-and-service-principals)
- [Azure DevOps REST API Documentation](https://docs.microsoft.com/en-us/rest/api/azure/devops/)
- [Backstage Integrations Documentation](https://backstage.io/docs/integrations/)

## Support

For issues or questions:
- Check the Troubleshooting section above
- Review backend logs for detailed error messages
- Contact your platform team or Backstage administrators
