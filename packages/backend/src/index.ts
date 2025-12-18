import { createBackend } from '@backstage/backend-defaults';

const backend = createBackend();

// App plugin
backend.add(import('@backstage/plugin-app-backend'));

// Auth plugins
backend.add(import('@backstage/plugin-auth-backend'));
backend.add(import('@backstage/plugin-auth-backend-module-guest-provider'));
backend.add(import('@backstage/plugin-auth-backend-module-microsoft-provider'));

// Catalog plugins
backend.add(import('@backstage/plugin-catalog-backend'));
backend.add(import('@backstage/plugin-catalog-backend-module-azure'));

// Azure DevOps plugin
backend.add(import('@backstage/plugin-azure-devops-backend'));

// Scaffolder plugin
backend.add(import('@backstage/plugin-scaffolder-backend'));

// Search plugins
backend.add(import('@backstage/plugin-search-backend'));
backend.add(import('@backstage/plugin-search-backend-module-catalog'));

// TechDocs plugin
backend.add(import('@backstage/plugin-techdocs-backend'));

// Proxy plugin
backend.add(import('@backstage/plugin-proxy-backend'));

// Permission plugins
backend.add(import('@backstage/plugin-permission-backend'));
backend.add(import('@backstage/plugin-permission-backend-module-allow-all-policy'));

backend.start();
