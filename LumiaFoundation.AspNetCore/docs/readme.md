# Histórico de versões

## 0.9.0

- Refatorado o tratamento de exceções HTTP para registrar erros 5xx como erro e respostas 4xx como aviso.

## 0.8.0

- Adicionado método `AddServicesFromAssembly` em `ServiceCollectionExtensions`
- Permite registrar serviços automaticamente de um assembly seguindo a convenção de nomenclatura (Interface: `I{NomeDaClasse}`, Implementação: `NomeDaClasse`)
- Suporta configuração de `ServiceLifetime` (padrão: Scoped)

## 0.7.0

- Migração para .NET 10.0
