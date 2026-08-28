# Histórico de versões

## 0.11.0

- Tornada a classe `DomainBaseException` abstrata, impedindo sua instanciação direta.
- O código HTTP deve ser definido por cada classe derivada através de `StatusCodeValue`.
- Removido o recebimento do código HTTP pelos construtores da exceção base.

Exemplo de definição de uma exceção de domínio:

```csharp
public sealed class UserNotFoundException : DomainBaseException
{
 protected override int StatusCodeValue => StatusCodes.Status404NotFound;
}
```

## 0.10.0

- Especializado o `DomainExceptionHandler` para tratar apenas exceções do tipo `DomainBaseException`.
- Adaptado o `UnhandledExceptionHandler` para tratar exceções que não são `DomainBaseException`, retornando status HTTP 500.
- Handlers que não correspondem ao tipo da exceção retornam `false`, permitindo o processamento pelo próximo handler.

A lib agora ficou com duas possibilidades de tratamento de erros. A primeira, mais moderna, através de service, configurada como:

```csharp
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();
```

e outra é a forma antiga de fazer, através de middleware, configurada através de Extensions:

```csharp
var logger = app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(logger);
```

## 0.9.0

- Refatorado o tratamento de exceções HTTP para registrar erros 5xx como erro e respostas 4xx como aviso.

## 0.8.0

- Adicionado método `AddServicesFromAssembly` em `ServiceCollectionExtensions`
- Permite registrar serviços automaticamente de um assembly seguindo a convenção de nomenclatura (Interface: `I{NomeDaClasse}`, Implementação: `NomeDaClasse`)
- Suporta configuração de `ServiceLifetime` (padrão: Scoped)

## 0.7.0

- Migração para .NET 10.0
