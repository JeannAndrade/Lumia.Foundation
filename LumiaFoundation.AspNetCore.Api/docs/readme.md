# Lumia.Foundation.AspNetCore.Api

Biblioteca com utilitários para APIs ASP.NET Core, incluindo filtros de ação, controlador base, extensão de registro por assembly, handlers de exceção, health checks e suporte a OpenAPI/Scalar.

## Instalação

```bash
dotnet add package Lumia.Foundation.AspNetCore.Api
```

## Uso rápido

```csharp
using LumiaFoundation.AspNetCore.Api.Commons.Extensions;

builder.Services.AddServicesFromAssembly(typeof(MyService).Assembly);
builder.Services.AddValidationFilters();
builder.Services.AddExceptionHandlers();
```

## Endpoints de health checks

```csharp
using LumiaFoundation.AspNetCore.Api.Commons.Extensions;

var app = builder.Build();
app.MapLumiaHealthChecks();
```

## OpenAPI e Scalar

```csharp
using LumiaFoundation.AspNetCore.Api.Commons.Extensions;

builder.Services.ConfigureOpenApi(title: "Orders API", version: "v1");
var app = builder.Build();
app.MapOpenApiDevTools();
```
