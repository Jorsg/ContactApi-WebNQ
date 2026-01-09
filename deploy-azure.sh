#!/bin/bash

# Natalia Quintero Contact API - Azure Deployment Script
# Este script automatiza el deployment a Azure App Service

set -e  # Exit on error

echo "=================================================="
echo "Natalia Quintero Contact API - Azure Deployment"
echo "=================================================="
echo ""

# Variables - PERSONALIZA ESTAS
RESOURCE_GROUP="rg-natalia-quintero"
LOCATION="eastus"
ACR_NAME="nqcontactapi"
APP_SERVICE_PLAN="plan-contact-api"
WEB_APP_NAME="nq-contact-api"
IMAGE_NAME="contact-api"

echo "📋 Configuración:"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  Location: $LOCATION"
echo "  ACR Name: $ACR_NAME"
echo "  Web App: $WEB_APP_NAME"
echo ""

read -p "¿Continuar con el deployment? (y/n) " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Yy]$ ]]
then
    echo "Deployment cancelado."
    exit 1
fi

# 1. Crear Resource Group (si no existe)
echo "📦 Verificando Resource Group..."
if ! az group show --name $RESOURCE_GROUP &> /dev/null; then
    echo "  ➕ Creando Resource Group..."
    az group create --name $RESOURCE_GROUP --location $LOCATION
else
    echo "  ✅ Resource Group ya existe"
fi

# 2. Crear Azure Container Registry (si no existe)
echo ""
echo "🐳 Verificando Azure Container Registry..."
if ! az acr show --name $ACR_NAME --resource-group $RESOURCE_GROUP &> /dev/null; then
    echo "  ➕ Creando ACR..."
    az acr create --resource-group $RESOURCE_GROUP \
        --name $ACR_NAME --sku Basic
else
    echo "  ✅ ACR ya existe"
fi

# 3. Build y Push de la imagen
echo ""
echo "🔨 Construyendo imagen Docker en ACR..."
az acr build --registry $ACR_NAME \
    --image $IMAGE_NAME:latest \
    --image $IMAGE_NAME:$(date +%Y%m%d-%H%M%S) \
    --file Dockerfile .

echo "  ✅ Imagen construida y pusheada exitosamente"

# 4. Crear App Service Plan (si no existe)
echo ""
echo "📊 Verificando App Service Plan..."
if ! az appservice plan show --name $APP_SERVICE_PLAN --resource-group $RESOURCE_GROUP &> /dev/null; then
    echo "  ➕ Creando App Service Plan..."
    az appservice plan create \
        --name $APP_SERVICE_PLAN \
        --resource-group $RESOURCE_GROUP \
        --is-linux --sku B1
else
    echo "  ✅ App Service Plan ya existe"
fi

# 5. Crear o actualizar Web App
echo ""
echo "🌐 Configurando Web App..."
if ! az webapp show --name $WEB_APP_NAME --resource-group $RESOURCE_GROUP &> /dev/null; then
    echo "  ➕ Creando Web App..."
    az webapp create \
        --resource-group $RESOURCE_GROUP \
        --plan $APP_SERVICE_PLAN \
        --name $WEB_APP_NAME \
        --deployment-container-image-name $ACR_NAME.azurecr.io/$IMAGE_NAME:latest
else
    echo "  ✅ Web App ya existe"
fi

# 6. Configurar ACR credentials
echo ""
echo "🔐 Configurando credenciales ACR..."
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query passwords[0].value -o tsv)

az webapp config container set \
    --name $WEB_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --docker-custom-image-name $ACR_NAME.azurecr.io/$IMAGE_NAME:latest \
    --docker-registry-server-url https://$ACR_NAME.azurecr.io \
    --docker-registry-server-user $ACR_NAME \
    --docker-registry-server-password $ACR_PASSWORD

# 7. Habilitar logs
echo ""
echo "📝 Habilitando logs..."
az webapp log config \
    --name $WEB_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --docker-container-logging filesystem

# 8. Configurar continuous deployment
echo ""
echo "🔄 Habilitando continuous deployment..."
az webapp deployment container config \
    --name $WEB_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --enable-cd true

# 9. Restart app
echo ""
echo "♻️  Reiniciando aplicación..."
az webapp restart --name $WEB_APP_NAME --resource-group $RESOURCE_GROUP

# Esperar un momento
echo ""
echo "⏳ Esperando que la aplicación inicie..."
sleep 10

# 10. Health check
echo ""
echo "🏥 Verificando health check..."
WEB_APP_URL="https://$WEB_APP_NAME.azurewebsites.net"
HEALTH_URL="$WEB_APP_URL/health"

if curl -f -s $HEALTH_URL > /dev/null; then
    echo "  ✅ Health check exitoso!"
else
    echo "  ⚠️  Health check falló. Revisa los logs."
fi

# Mostrar información final
echo ""
echo "=================================================="
echo "✅ DEPLOYMENT COMPLETADO"
echo "=================================================="
echo ""
echo "📍 URL de la API: $WEB_APP_URL"
echo "📍 Health Check: $HEALTH_URL"
echo "📍 Swagger UI: $WEB_APP_URL/swagger"
echo ""
echo "🔧 Próximos pasos:"
echo "  1. Configurar variables de entorno (EmailSettings)"
echo "  2. Configurar CORS (AllowedOrigins)"
echo "  3. Probar el endpoint /api/contact"
echo ""
echo "📝 Ver logs:"
echo "  az webapp log tail --name $WEB_APP_NAME --resource-group $RESOURCE_GROUP"
echo ""
echo "🔐 Configurar App Settings:"
echo "  az webapp config appsettings set --name $WEB_APP_NAME --resource-group $RESOURCE_GROUP --settings Key=Value"
echo ""
echo "=================================================="
