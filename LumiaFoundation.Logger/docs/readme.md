# Usa da lib

Biblioteca com interface para logging, baseada em NLog.

## Instalação

Instale o pacote com o comando:

```bash
dotnet add package Lumia.Foundation.Logger
```

## Como usar

Ao instalar o pacote, o arquivo `nlog.config` da biblioteca é copiado automaticamente para a raiz do projeto consumidor durante o build. Isso facilita a configuração inicial do NLog sem necessidade de copiar manualmente o arquivo.

Na classe `Program.cs`, configure o arquivo de configuração do NLog e registre a interface de log na injeção de dependência:

```csharp
using LumiaFoundation.Logger.Contracts;
using LumiaFoundation.Logger.LoggerService;

var builder = WebApplication.CreateBuilder(args);

LoggerManager.LoadConfigurationFromFile(
    Path.Combine(builder.Environment.ContentRootPath, "nlog.config"));

builder.Services.AddSingleton<ILoggerManager, LoggerManager>();

var app = builder.Build();
```

Após isso, você pode injetar `ILoggerManager` em serviços, controllers ou handlers e utilizar os métodos de log:

```csharp
public class OrderService
{
    private readonly ILoggerManager _logger;

    public OrderService(ILoggerManager logger)
    {
        _logger = logger;
    }

    public void CreateOrder()
    {
        _logger.LogInfo("Criando pedido");
        _logger.LogWarn("Pedido sem cliente associado");
    }
}
```

### Exemplo de `nlog.config`

O pacote copia o arquivo de configuração para a raiz do projeto consumidor automaticamente, então você pode começar com algo como:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogLevel="Trace"
      internalLogFile=".\internal_logs\internallog.txt">

  <targets>
    <target
      xsi:type="File"
      name="logfile"
      fileName=".\logs\current.log"
      layout="${longdate} ${level:uppercase=true} ${message}"
      archiveFileName=".\logs\archive\txt\${shortdate}_logfile.{#}.log"
      archiveAboveSize="10485760"
      maxArchiveFiles="30"
      archiveNumbering="Rolling"
      archiveEvery="Day"
    />

    <target
      xsi:type="File"
      name="jsonFile"
      fileName=".\logs\current.json"
      archiveFileName=".\logs\archive\json\${shortdate}_logfile.{#}.json"
      archiveAboveSize="10485760"
      maxArchiveFiles="30"
      archiveNumbering="Rolling"
      createDirs="true">
      <layout xsi:type="JsonLayout" includeEventProperties="true">
        <attribute name="time" layout="${longdate}" />
        <attribute name="level" layout="${level}" />
        <attribute name="message" layout="${message}" />
        <!-- O atributo 'properties' conterá todas as suas propriedades estruturadas -->
        <attribute name="properties" layout="${all-event-properties}" />
      </layout>
    </target>
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="jsonFile" />
  </rules>
</nlog>
```

## O que é Log Estruturado e Por que Usá-lo?

Em vez de gerar apenas uma mensagem de texto como `"Usuário Kenny logou do IP 127.0.0.1"`, o log estruturado preserva os dados que compõem a mensagem, como `Usuário` e `IP`, como propriedades separadas.

### Vantagens

* Consultas poderosas: você pode pesquisar facilmente logs por um determinado usuário (`user = "Kenny"`).
* Agrupamento inteligente: agrupe eventos semelhantes, como todos os logs de `Logon`.
* Menos fragilidade: se a mensagem mudar, as propriedades continuam as mesmas, evitando quebrar consultas e filtros.

## Usando o Log Estruturado no NLog (Sintaxe de Templates)

A sintaxe usa placeholders nomeados entre chaves `{...}` na mensagem, em vez de índices numéricos (`{0}`, `{1}`).

### Exemplo básico

```csharp
// Exemplo com placeholders nomeados. Os valores "Kenny" e "127.0.0.1"
// serão capturados como propriedades "user" e "ip_address".
_logger.LogInfo("Logon by {user} from {ip_address}", "Kenny", "127.0.0.1");

// Log de um objeto complexo. A propriedade "shopitem" capturará o objeto inteiro.
_logger.LogDebug("{shopitem} added to basket by {user}", new { Id = 6, Name = "Jacket", Color = "Orange" }, "Kenny");
```

## Formatando a Mensagem (Como os Dados São Exibidos)

O NLog formata os parâmetros de forma inteligente, dependendo do tipo:

* `null`: exibe `NULL`.
* Listas/arrays: exibem `item1, item2, ...`.
* Dicionários: exibem `chave1=valor1, chave2=valor2, ...`.
* Objetos: por padrão, chamam o método `ToString()`.

### Controlando a Formatação com `@` e `$`

Você pode prefixar o nome do placeholder com caracteres especiais para controlar a serialização:

* `@` (formatação avançada): serializa o objeto em JSON. É excelente para capturar o estado completo de um objeto.
* `$` (formatação simples): força o uso do método `ToString()` do objeto, ignorando a serialização de suas propriedades.

### Exemplo de formatação

```csharp
var order = new Order { OrderId = 2, Status = "Processing" };

// 1. Formatação padrão: chama order.ToString()
// Resultado: Test MyApp.Order
_logger.LogInfo("Test {order}", order);

// 2. Com '@': serializa o objeto para JSON
// Resultado: Test {"OrderId":2, "Status":"Processing"}
_logger.LogInfo("Test {@order}", order);

// 3. Com '$': força o uso do ToString()
// Resultado: Test MyApp.Order
_logger.LogInfo("Test {$order}", order);
```

## Capturando e Usando as Propriedades nos Targets (Destinos)

A grande vantagem do log estruturado é que os targets do NLog, como arquivo, banco de dados e console, podem capturar essas propriedades automaticamente.

### Exemplo de configuração do `nlog.config` para arquivo JSON

```xml
<target xsi:type="File" name="jsonFile" fileName="log.json">
  <layout xsi:type="JsonLayout" includeEventProperties="true">
    <attribute name="time" layout="${longdate}" />
    <attribute name="level" layout="${level}" />
    <attribute name="message" layout="${message}" />
    <attribute name="properties" layout="${all-event-properties}" />
  </layout>
</target>
```

Com essa configuração, o log pode ficar assim:

```json
{
  "time": "2024-05-20 10:00:00.1234",
  "level": "Info",
  "message": "Logon by Kenny from 127.0.0.1",
  "properties": {
    "user": "Kenny",
    "ip_address": "127.0.0.1"
  }
}
```

## Extraindo Propriedades Específicas

Você pode acessar uma propriedade individual usando `${event-properties:item=user}`.

```xml
<target ...>
  <layout>
    User: ${event-properties:item=user}
  </layout>
</target>
```

## Conceitos Adicionais Importantes

* `${message:raw=true}`: use este layout para renderizar o template da mensagem (por exemplo, `"Logon by {user} from {ip_address}"`) em vez da mensagem formatada.
* Transformação de objetos (NLog 4.7+): você pode registrar uma função para transformar objetos grandes ou perigosos em um objeto mais seguro e reduzido antes da serialização.
* Desabilitar o log estruturado: se precisar reduzir o custo mínimo de performance, use `<nlog parseMessageTemplates="false">` na configuração.

## Resumo Final

| Funcionalidade | Descrição | Exemplo |
| :--- | :--- | :--- |
| Placeholders nomeados | Use nomes descritivos `{user}` em vez de índices `{0}`. | `logger.Info("User {user} logged in", "Kenny")` |
| Serialização com `@` | Converte objetos complexos em JSON para análise detalhada. | `logger.Info("Order {@order}", new { Id = 1 })` |
| Captura de propriedades | Targets como `File` e `Database` capturam as propriedades automaticamente. | Configurar `includeEventProperties="true"`. |
| Extração de propriedades | Use o layout `${event-properties}` para acessar um valor específico. | `${event-properties:item=user}` |

## Histórico de versões

* 0.1.0 - Versão inicial com interface básica, sem arquivo de configuração presente no pacote.
* 0.1.1 - Expondo método de configuração do serviço.
* 0.2.0 - Ajustes na documentação e uso do pacote com interface de log estruturado por NLog.
* 0.2.1 - Empacotamento automático do `nlog.config` para a raiz do projeto consumidor durante o build.
* 0.2.2 - Atualização do `nlog.config` com arquivos atuais de log, arquivamento por tamanho e organização dos logs em texto e JSON.
