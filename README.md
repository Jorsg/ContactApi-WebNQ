# Natalia Quintero - Contact API Backend

API REST en ASP.NET Core 8 para manejar el formulario de contacto del sitio web.

## 🚀 Características

- ✅ API REST con ASP.NET Core 8
- ✅ Validación de datos con FluentValidation
- ✅ Envío de emails con SendGrid o SMTP
- ✅ CORS configurado
- ✅ Docker optimizado para producción
- ✅ Listo para Azure App Service
- ✅ Logs estructurados
- ✅ Health checks
- ✅ Swagger/OpenAPI documentation
- ✅ Respuestas multilingües (ES/EN/FR)

## 📁 Estructura del Proyecto

```
backend-api/
├── Controllers/
│   └── ContactController.cs      # API endpoint
├── Models/
│   └── ContactRequest.cs         # Request/Response models
├── Services/
│   ├── IEmailService.cs          # Service interface
│   └── EmailService.cs           # Email implementation
├── Validators/
│   └── ContactRequestValidator.cs # FluentValidation rules
├── appsettings.json              # Configuration
├── appsettings.Development.json
├── Program.cs                    # App startup
├── ContactApi.csproj             # Project file
├── Dockerfile                    # Docker image
└── .dockerignore
```

## 🛠️ Requisitos Previos

- .NET 8 SDK
- Docker Desktop (para desarrollo local)
- Cuenta de Azure (para deployment)
- SendGrid API Key O cuenta SMTP (Office365, Gmail, etc.)

## ⚙️ Configuración

### 1. Configurar Email Settings

Edita `appsettings.json`:

#### Opción A: SendGrid (Recomendado)

1. Crea cuenta en [SendGrid](https://sendgrid.com)
2. Genera API Key
3. Configura:

```json
{
  "EmailSettings": {
    "Provider": "SendGrid",
    "FromEmail": "noreply@tusitio.com",
    "FromName": "Natalia Quintero",
    "ToEmail": "tu-email@gmail.com",
    "ToName": "Natalia Quintero",
    "SendGrid": {
      "ApiKey": "SG.xxxxxxxxxxxxxxxxxxxxxxxx"
    }
  }
}
```

#### Opción B: SMTP (Office365, Gmail, etc.)

```json
{
  "EmailSettings": {
    "Provider": "Smtp",
    "FromEmail": "tu-email@outlook.com",
    "FromName": "Natalia Quintero",
    "ToEmail": "tu-email@outlook.com",
    "ToName": "Natalia Quintero",
    "Smtp": {
      "Host": "smtp.office365.com",
      "Port": "587",
      "Username": "tu-email@outlook.com",
      "Password": "tu-password",
      "EnableSsl": "true"
    }
  }
}
```

**SMTP Hosts Comunes:**
- Office365: `smtp.office365.com:587`
- Gmail: `smtp.gmail.com:587` (requiere App Password)
- Outlook: `smtp-mail.outlook.com:587`

### 2. Configurar CORS

Actualiza tus dominios en `appsettings.json`:

```json
{
  "AllowedOrigins": [
    "https://tusitio.com",
    "https://www.tusitio.com"
  ]
}
```

## 🏃 Ejecutar Localmente

### Método 1: .NET CLI

```bash
# Restaurar dependencias
dotnet restore

# Ejecutar en modo desarrollo
dotnet run

# La API estará disponible en:
# https://localhost:7001
# Swagger UI: https://localhost:7001/swagger
```

### Método 2: Docker

```bash
# Construir imagen
docker build -t contact-api .

# Ejecutar contenedor
docker run -p 8080:8080 \
  -e EmailSettings__SendGrid__ApiKey="TU_API_KEY" \
  -e EmailSettings__ToEmail="tu-email@gmail.com" \
  contact-api

# La API estará en: http://localhost:8080
```

## 🐳 Deployment a Azure App Service con Docker

### Paso 1: Preparar Azure Container Registry (ACR)

```bash
# 1. Crear Resource Group (si no existe)
az group create --name rg-natalia-quintero --location eastus

# 2. Crear Azure Container Registry
az acr create --resource-group rg-natalia-quintero \
  --name nqcontactapi --sku Basic

# 3. Login al registry
az acr login --name nqcontactapi
```

### Paso 2: Construir y Pushear Imagen

```bash
# 1. Tag de la imagen
docker tag contact-api nqcontactapi.azurecr.io/contact-api:latest

# 2. Push a ACR
docker push nqcontactapi.azurecr.io/contact-api:latest

# O construir directamente en ACR (recomendado)
az acr build --registry nqcontactapi \
  --image contact-api:latest \
  --file Dockerfile .
```

### Paso 3: Crear App Service

```bash
# 1. Crear App Service Plan (Linux)
az appservice plan create \
  --name plan-contact-api \
  --resource-group rg-natalia-quintero \
  --is-linux --sku B1

# 2. Crear Web App con Container
az webapp create \
  --resource-group rg-natalia-quintero \
  --plan plan-contact-api \
  --name nq-contact-api \
  --deployment-container-image-name nqcontactapi.azurecr.io/contact-api:latest

# 3. Configurar ACR credentials
az webapp config container set \
  --name nq-contact-api \
  --resource-group rg-natalia-quintero \
  --docker-custom-image-name nqcontactapi.azurecr.io/contact-api:latest \
  --docker-registry-server-url https://nqcontactapi.azurecr.io \
  --docker-registry-server-user nqcontactapi \
  --docker-registry-server-password $(az acr credential show --name nqcontactapi --query passwords[0].value -o tsv)

# 4. Habilitar logs
az webapp log config \
  --name nq-contact-api \
  --resource-group rg-natalia-quintero \
  --docker-container-logging filesystem
```

### Paso 4: Configurar Variables de Entorno

**Opción A: Azure CLI**

```bash
az webapp config appsettings set \
  --resource-group rg-natalia-quintero \
  --name nq-contact-api \
  --settings \
    EmailSettings__Provider="SendGrid" \
    EmailSettings__SendGrid__ApiKey="TU_SENDGRID_API_KEY" \
    EmailSettings__FromEmail="noreply@tusitio.com" \
    EmailSettings__ToEmail="tu-email@gmail.com" \
    EmailSettings__FromName="Natalia Quintero Website" \
    EmailSettings__ToName="Natalia Quintero" \
    AllowedOrigins__0="https://tusitio.com" \
    AllowedOrigins__1="https://www.tusitio.com"
```

**Opción B: Portal de Azure**

1. Ve a tu App Service en Azure Portal
2. Settings → Configuration → Application settings
3. Agrega las siguientes variables:

```
EmailSettings__Provider = SendGrid
EmailSettings__SendGrid__ApiKey = SG.xxxxxxx
EmailSettings__FromEmail = noreply@tusitio.com
EmailSettings__ToEmail = tu-email@gmail.com
EmailSettings__FromName = Natalia Quintero Website
EmailSettings__ToName = Natalia Quintero
AllowedOrigins__0 = https://tusitio.com
AllowedOrigins__1 = https://www.tusitio.com
```

4. Save y Restart

### Paso 5: Verificar Deployment

```bash
# Ver logs
az webapp log tail --name nq-contact-api --resource-group rg-natalia-quintero

# Health check
curl https://nq-contact-api.azurewebsites.net/health

# Test del endpoint
curl -X POST https://nq-contact-api.azurewebsites.net/api/contact \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "subject": "Test",
    "message": "This is a test message",
    "language": "en"
  }'
```

## 🔄 Actualizar la Aplicación

```bash
# 1. Rebuild y push nueva imagen
az acr build --registry nqcontactapi \
  --image contact-api:latest \
  --file Dockerfile .

# 2. Restart app service
az webapp restart --name nq-contact-api --resource-group rg-natalia-quintero

# O con webhook para auto-deploy
az webapp deployment container config \
  --name nq-contact-api \
  --resource-group rg-natalia-quintero \
  --enable-cd true
```

## 🌐 Integrar con tu Website

Actualiza el formulario en tu website para usar la API:

```javascript
// En tu archivo script.js o dentro del HTML
const contactForm = document.getElementById('contactForm');

contactForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const formData = {
        name: document.getElementById('name').value,
        email: document.getElementById('email').value,
        subject: document.getElementById('subject').value,
        message: document.getElementById('message').value,
        language: document.documentElement.lang // 'es', 'en', 'fr'
    };
    
    try {
        const response = await fetch('https://nq-contact-api.azurewebsites.net/api/contact', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert(result.message);
            contactForm.reset();
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Hubo un error al enviar el mensaje. Por favor, intenta nuevamente.');
    }
});
```

## 📊 Monitoreo

### Ver Logs en Azure

```bash
# Stream logs en tiempo real
az webapp log tail --name nq-contact-api --resource-group rg-natalia-quintero

# Descargar logs
az webapp log download --name nq-contact-api --resource-group rg-natalia-quintero
```

### Configurar Application Insights (Recomendado)

```bash
# 1. Crear Application Insights
az monitor app-insights component create \
  --app nq-contact-api-insights \
  --location eastus \
  --resource-group rg-natalia-quintero

# 2. Obtener instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app nq-contact-api-insights \
  --resource-group rg-natalia-quintero \
  --query instrumentationKey -o tsv)

# 3. Configurar en App Service
az webapp config appsettings set \
  --resource-group rg-natalia-quintero \
  --name nq-contact-api \
  --settings APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATION_KEY
```

## 🔒 Seguridad

### Mejores Prácticas Implementadas

✅ **Secrets Management**: Usar Azure Key Vault para API keys
✅ **HTTPS Only**: Forzar HTTPS en producción
✅ **CORS**: Solo dominios permitidos
✅ **Input Validation**: FluentValidation en todos los inputs
✅ **Rate Limiting**: Considerar agregar rate limiting
✅ **Logs**: No loguear información sensible

### Agregar Azure Key Vault (Opcional pero Recomendado)

```bash
# 1. Crear Key Vault
az keyvault create \
  --name nq-keyvault \
  --resource-group rg-natalia-quintero \
  --location eastus

# 2. Agregar secret
az keyvault secret set \
  --vault-name nq-keyvault \
  --name SendGridApiKey \
  --value "TU_API_KEY"

# 3. Dar permisos al App Service
az webapp identity assign \
  --resource-group rg-natalia-quintero \
  --name nq-contact-api

# Obtener el principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --resource-group rg-natalia-quintero \
  --name nq-contact-api \
  --query principalId -o tsv)

# Dar acceso al Key Vault
az keyvault set-policy \
  --name nq-keyvault \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list

# 4. Referenciar en App Settings
az webapp config appsettings set \
  --resource-group rg-natalia-quintero \
  --name nq-contact-api \
  --settings \
    EmailSettings__SendGrid__ApiKey="@Microsoft.KeyVault(SecretUri=https://nq-keyvault.vault.azure.net/secrets/SendGridApiKey/)"
```

## 🧪 Testing

### Test Local

```bash
# POST request de prueba
curl -X POST http://localhost:8080/api/contact \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Juan Pérez",
    "email": "juan@example.com",
    "subject": "Consulta sobre servicios",
    "message": "Hola, me gustaría más información sobre sus servicios de traducción.",
    "language": "es"
  }'
```

### Respuestas Esperadas

**Success (200):**
```json
{
  "success": true,
  "message": "¡Gracias por tu mensaje! Te responderé lo antes posible."
}
```

**Validation Error (400):**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": {
    "Email": "Invalid email format",
    "Message": "Message must be at least 10 characters"
  }
}
```

## 💰 Costos Estimados Azure

- **App Service B1**: ~$13/mes
- **Container Registry Basic**: ~$5/mes
- **Application Insights**: ~$2/mes (primeros 5GB gratis)

**Total estimado: ~$20/mes**

## 🆘 Troubleshooting

### El contenedor no inicia

```bash
# Ver logs del contenedor
az webapp log tail --name nq-contact-api --resource-group rg-natalia-quintero

# Verificar que el puerto esté correcto (8080)
# Verificar variables de entorno
az webapp config appsettings list --name nq-contact-api --resource-group rg-natalia-quintero
```

### Emails no se envían

1. Verificar API Key de SendGrid
2. Verificar que el email "From" esté verificado en SendGrid
3. Revisar logs de la aplicación
4. Test con SMTP como alternativa

### CORS Errors

1. Verificar que tu dominio esté en `AllowedOrigins`
2. Verificar que incluyas `https://` en la URL
3. Check browser console para error específico

## 📚 Recursos Adicionales

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [SendGrid .NET Documentation](https://docs.sendgrid.com/for-developers/sending-email/v3-csharp-code-example)
- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)

## 📞 Soporte

Si tienes problemas con el deployment:
1. Revisa los logs
2. Verifica las variables de entorno
3. Test localmente primero
4. Contacta a Azure Support si es problema de infraestructura

---

**¡Éxito con tu API!** 🚀
