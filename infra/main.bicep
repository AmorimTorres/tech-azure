// Infraestrutura base do MVP - TechStore Cloud
// Deploy: az deployment group create -g rg-techstore-produtos-prd -f main.bicep -p sqlAdminPassword=<senha>
param location string = resourceGroup().location
param prefix string = 'techstore'
@secure()
param sqlAdminPassword string

resource law 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'law-${prefix}-prd'
  location: location
  properties: { sku: { name: 'PerGB2018' }, retentionInDays: 30 }
}

resource appi 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-${prefix}-prd'
  location: location
  kind: 'web'
  properties: { Application_Type: 'web', WorkspaceResourceId: law.id }
}

resource swa 'Microsoft.Web/staticSites@2022-09-01' = {
  name: 'swa-${prefix}-prd'
  location: 'eastus2'               // Static Web Apps não está disponível em todas as regiões
  sku: { name: 'Standard', tier: 'Standard' }
  properties: {}
}

resource vnet 'Microsoft.Network/virtualNetworks@2023-04-01' = {
  name: 'vnet-${prefix}-prd'
  location: location
  properties: {
    addressSpace: { addressPrefixes: ['10.0.0.0/16'] }
    subnets: [
      { name: 'snet-agw',  properties: { addressPrefix: '10.0.1.0/24' } }
      { name: 'snet-app',  properties: { addressPrefix: '10.0.2.0/24' } }
      { name: 'snet-data', properties: { addressPrefix: '10.0.3.0/24' } }
    ]
  }
}

resource sql 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: 'sql-${prefix}-prd'
  location: location
  properties: {
    administratorLogin: 'sqladmin'
    administratorLoginPassword: sqlAdminPassword
    publicNetworkAccess: 'Disabled'   // acesso somente via Private Endpoint
    minimalTlsVersion: '1.2'
  }
}

resource db 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: sql
  name: 'productsdb'
  location: location
  sku: { name: 'GP_S_Gen5_1', tier: 'GeneralPurpose' }
}

// TODO (próximas etapas): Application Gateway WAF v2, 2 VMs Linux (zonas 1 e 2),
// Private Endpoint do SQL, Key Vault e alertas do Azure Monitor.
