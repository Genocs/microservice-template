// Get the resource group location
param location string = 'East US'

@description('The name of you Web Site.')
param webSiteName string = 'gnx-website'

param uniqueString string = '{uniqueString(resourceGroup().id)}'

// Generate a unique storage account name
param storageAccountName string = 'helloworldstorage${uniqueString}'

// Create a storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}

// Create an App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'asp-${uniqueString}'
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
}

// Create a web app
resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webSiteName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'STORAGE_ACCOUNT_NAME'
          value: storageAccount.name
        }
      ]
    }
  }
}
