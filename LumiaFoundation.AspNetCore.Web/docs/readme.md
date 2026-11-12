# Lumia.Foundation.AspNetCore.Web

Biblioteca com utilitários para aplicações web ASP.NET Core, incluindo autenticação de cliente, sessão e paginação via tag helper.

## Instalação

```bash
dotnet add package Lumia.Foundation.AspNetCore.Web
```

## Uso rápido

```csharp
using LumiaFoundation.AspNetCore.Web.Commons.Extensions;

builder.Services.AddApiSignInService();
builder.Services.AddApiRegistrationService();
```

```cshtml
@addTagHelper *, LumiaFoundation.AspNetCore.Web
```

```cshtml
<lumia-pagination page="1" total-pages="10" />
```
