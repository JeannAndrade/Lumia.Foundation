# Lumia.Foundation.Core

Conjunto de classes utilitárias para projetos C#, com abstrações de domínio, provedores de data e hora e extensões de uso comum.

## Instalação

```bash
dotnet add package Lumia.Foundation.Core
```

## Recursos

* `ValueObject`: classe base para objetos de valor com comparação por componentes.
* `GenericSingleValueObject<T>`: implementação genérica para value objects compostos por um único valor.
* `IDateTimeProvider`: abstração para obter data e hora.
* `LocalDateTimeProvider`: implementação baseada em `DateTime.Now`.
* `UtcDateTimeProvider`: implementação baseada em `DateTime.UtcNow`.
* `EnumExtensions.GetDescription()`: retorna o valor de `DescriptionAttribute` de um enum, quando existir.
* `ParseHelper.ToIntOrDefault()`: converte uma string para `int` e retorna um valor padrão quando a conversão falha.
* `DomainBaseException`: classe base para exceções específicas do domínio, sem dependência de HTTP.
* `CommandValidationException`: exceção lançada quando um comando falha nas validações.
* `CommandValidator.Validate()`: valida objetos usando `DataAnnotations`.

## Alterações recentes

* Adicionadas `DomainBaseException` e `CommandValidationException` ao projeto Core.
* Adicionado `CommandValidator` para validação de comandos usando DataAnnotations.
* Removida a dependência de conceitos HTTP das exceções de domínio.
* Adicionada a classe utilitária `ParseHelper` para conversão segura de valores string para inteiros.

## Histórico de versões

* 0.5.0 - Migração para .NET 10.0.
* 0.2.0 - Adicionado `GenericSingleValueObject`, uma classe genérica para ser base de value objects primitivos.
* 0.1.0 - Inclusão de repository pattern e DateTimeService.
