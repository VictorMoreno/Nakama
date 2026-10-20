# Infraestructura de Azure (entorno `nakama-dev`)

Terraform crea el grupo de recursos `nakama-dev`, un App Service Windows (plan B1, .NET 10) y un PostgreSQL flexible
server (Burstable B1ms, 32 GB, sin alta disponibilidad). El estado de Terraform es **local** (`terraform.tfstate`, excluido
de git): hay que conservar una copia, porque sin él Terraform no conoce los recursos creados.

## Requisitos

- Terraform >= 1.9 y Azure CLI.
- `az login` y la suscripción elegida (`az account set --subscription <id>`).

## Crear el entorno

```powershell
cd infra
$env:TF_VAR_subscription_id = "<id de la suscripción>"
$env:TF_VAR_postgres_admin_password = "<contraseña de administrador>"
terraform init
terraform plan -var-file=dev.tfvars
terraform apply -var-file=dev.tfvars
```

## Variables de entorno del App Service (a mano, en el portal)

Terraform solo define la configuración no sensible (`ASPNETCORE_ENVIRONMENT` y las rutas `Storage__*` bajo
`D:\home\site\external_resources`) y no sobrescribe las app settings posteriores (`ignore_changes`). Hay que añadir en
*Configuración → Variables de entorno*:

| Nombre | Valor |
|---|---|
| `ConnectionStrings__Nakama` | el output `connection_string_template` con la contraseña real |
| `Jwt__SigningKey` | una clave aleatoria larga |

## Publicar la aplicación

```powershell
dotnet publish apps/backend/api/Api.csproj -c Release -o publish
Compress-Archive -Path publish\* -DestinationPath publish.zip -Force
az webapp deploy --resource-group nakama-dev --name <output app_name> --src-path publish.zip --type zip
```

`-c Release` compila el frontend en modo `release` (carga `apps/frontend/nakama.web/.env.release`). No activar
`WEBSITE_RUN_FROM_PACKAGE`: dejaría `wwwroot` de solo lectura. La API aplica las migraciones al arrancar.

## Destruir el entorno

```powershell
terraform destroy -var-file=dev.tfvars
```
