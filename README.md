# TechStore Cloud — MVP de Cadastro de Produtos (Azure)

Projeto acadêmico — Pós-graduação em Engenharia de Software (InfNet).
Código base da arquitetura descrita em [`docs/Arquitetura_Cadastro_Produtos_Azure.pdf`](docs/Arquitetura_Cadastro_Produtos_Azure.pdf).

## Arquitetura (resumo)

Usuário → **Azure Static Web Apps** (frontend) → **Application Gateway** → **VMs com API .NET 8** → **Azure SQL Database**.
Monitoramento com **Azure Monitor + Log Analytics + Application Insights**.

## Estrutura

| Pasta | Conteúdo |
|---|---|
| `docs/` | Documento de arquitetura (PDF com diagramas) |
| `frontend/` | SPA estática simples (HTML/JS) |
| `api/ProductsApi/` | API REST em .NET 8 (Minimal API + EF Core + Application Insights) |
| `infra/` | Bicep: Log Analytics, App Insights, Static Web Apps, VNet, Azure SQL |

## Executar a API localmente

```bash
cd api/ProductsApi
# ajuste ConnectionStrings:Sql (ex.: SQL Server local/Docker)
dotnet run
# Swagger: http://localhost:5000/swagger  |  Health: /health
```

## Endpoints

`POST /api/products` · `GET /api/products` · `GET /api/products/{id}` · `PUT /api/products/{id}` · `DELETE /api/products/{id}` · `GET /health`

## Provisionar a infraestrutura

```bash
az group create -n rg-techstore-produtos-prd -l brazilsouth
az deployment group create -g rg-techstore-produtos-prd -f infra/main.bicep -p sqlAdminPassword=<senha>
```

## Escopo do MVP / próximos passos

Implementado: API de produtos, frontend, IaC base. Pendente: Application Gateway, VMs, Private Endpoint, Key Vault, alertas, CI/CD e o serviço Categories API.
