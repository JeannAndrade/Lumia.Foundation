# Usa da lib

Lib com interface de log

## Como usar

Instale o pacote com o comando:

`dotnet add package Lumia.Foundation.Logger`

Na classe program configure o log com o comando:

``` csharp
using Microsoft.AspNetCore.HttpOverrides;
using webapi.Extensions;
using LumiaFoundation.Logger.LoggerService;

var builder = WebApplication.CreateBuilder(args);

LoggerManager.LoadConfigurationFromFile(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

// Add services to the container.
builder.Services.ConfigureCors();
```

## Histórico de versões

* 0.1.0 - Versão inicial com interface básica, sem arquivo de configuração presente no pacote.
* 0.1.1 - Expondo método de configuração do serviço
