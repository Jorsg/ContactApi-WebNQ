#!/bin/bash

# Script para configurar las variables de entorno en Azure App Service
# Ejecuta este script DESPUÉS de hacer el deployment inicial

set -e

echo "=================================================="
echo "Configuración de Variables de Entorno"
echo "=================================================="
echo ""

# Variables
RESOURCE_GROUP="rg-natalia-quintero"
WEB_APP_NAME="nq-contact-api"

# Solicitar información al usuario
echo "📧 Configuración de Email"
echo ""

read -p "Proveedor de Email (SendGrid/Smtp): " EMAIL_PROVIDER
EMAIL_PROVIDER=${EMAIL_PROVIDER:-SendGrid}

read -p "Email FROM (ej: noreply@tusitio.com): " FROM_EMAIL
read -p "Nombre FROM (ej: Natalia Quintero Website): " FROM_NAME
read -p "Email TO (tu email personal): " TO_EMAIL
read -p "Nombre TO (ej: Natalia Quintero): " TO_NAME

if [ "$EMAIL_PROVIDER" == "SendGrid" ] || [ "$EMAIL_PROVIDER" == "sendgrid" ]; then
    read -p "SendGrid API Key: " SENDGRID_KEY
    
    echo ""
    echo "🌐 Configuración de CORS"
    echo ""
    read -p "Dominio principal (ej: https://tusitio.com): " DOMAIN1
    read -p "Dominio con www (ej: https://www.tusitio.com): " DOMAIN2
    
    echo ""
    echo "⚙️  Aplicando configuración..."
    
    az webapp config appsettings set \
        --resource-group $RESOURCE_GROUP \
        --name $WEB_APP_NAME \
        --settings \
            EmailSettings__Provider="SendGrid" \
            EmailSettings__SendGrid__ApiKey="$SENDGRID_KEY" \
            EmailSettings__FromEmail="$FROM_EMAIL" \
            EmailSettings__FromName="$FROM_NAME" \
            EmailSettings__ToEmail="$TO_EMAIL" \
            EmailSettings__ToName="$TO_NAME" \
            AllowedOrigins__0="$DOMAIN1" \
            AllowedOrigins__1="$DOMAIN2"
else
    read -p "SMTP Host (ej: smtp.office365.com): " SMTP_HOST
    read -p "SMTP Port (ej: 587): " SMTP_PORT
    SMTP_PORT=${SMTP_PORT:-587}
    read -p "SMTP Username: " SMTP_USER
    read -p "SMTP Password: " -s SMTP_PASS
    echo ""
    read -p "Enable SSL (true/false): " SMTP_SSL
    SMTP_SSL=${SMTP_SSL:-true}
    
    echo ""
    echo "🌐 Configuración de CORS"
    echo ""
    read -p "Dominio principal (ej: https://tusitio.com): " DOMAIN1
    read -p "Dominio con www (ej: https://www.tusitio.com): " DOMAIN2
    
    echo ""
    echo "⚙️  Aplicando configuración..."
    
    az webapp config appsettings set \
        --resource-group $RESOURCE_GROUP \
        --name $WEB_APP_NAME \
        --settings \
            EmailSettings__Provider="Smtp" \
            EmailSettings__Smtp__Host="$SMTP_HOST" \
            EmailSettings__Smtp__Port="$SMTP_PORT" \
            EmailSettings__Smtp__Username="$SMTP_USER" \
            EmailSettings__Smtp__Password="$SMTP_PASS" \
            EmailSettings__Smtp__EnableSsl="$SMTP_SSL" \
            EmailSettings__FromEmail="$FROM_EMAIL" \
            EmailSettings__FromName="$FROM_NAME" \
            EmailSettings__ToEmail="$TO_EMAIL" \
            EmailSettings__ToName="$TO_NAME" \
            AllowedOrigins__0="$DOMAIN1" \
            AllowedOrigins__1="$DOMAIN2"
fi

echo ""
echo "✅ Configuración aplicada exitosamente!"
echo ""
echo "♻️  Reiniciando aplicación..."
az webapp restart --name $WEB_APP_NAME --resource-group $RESOURCE_GROUP

echo ""
echo "=================================================="
echo "✅ CONFIGURACIÓN COMPLETADA"
echo "=================================================="
echo ""
echo "🧪 Para probar la API:"
echo ""
echo "curl -X POST https://$WEB_APP_NAME.azurewebsites.net/api/contact \\"
echo "  -H \"Content-Type: application/json\" \\"
echo "  -d '{"
echo "    \"name\": \"Test User\","
echo "    \"email\": \"test@example.com\","
echo "    \"subject\": \"Test Message\","
echo "    \"message\": \"This is a test message from the API\","
echo "    \"language\": \"en\""
echo "  }'"
echo ""
