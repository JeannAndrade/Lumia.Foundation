# Lumia.Foundation.Abstractions

Biblioteca com abstrações compartilhadas pela Lumia Foundation. Atualmente, disponibiliza o modelo `ErrorDetails` para padronizar respostas de erro entre APIs e clientes HTTP.

## Instalação

Instale o pacote no projeto:

```bash
dotnet add package Lumia.Foundation.Abstractions
```

O pacote não possui dependências externas.

## Modelo de erro

`ErrorDetails` representa uma resposta de erro com o código HTTP, a mensagem e o tipo da exceção que a originou:

```csharp
using LumiaFoundation.Abstractions.ErrorModel;

var error = new ErrorDetails
{
    StatusCode = 404,
    Message = "Order not found.",
    ExceptionType = "EntityNotFoundException"
};
```

O método `ToString` serializa o modelo em JSON, sendo útil para gravar ou devolver a resposta:

```csharp
var content = error.ToString();
// {"StatusCode":404,"Message":"Order not found.","ExceptionType":"EntityNotFoundException"}
```

`ExceptionType` é obrigatório. `Message` é opcional para acomodar cenários em que a mensagem não deve ser exposta ao cliente.

## Histórico de versões

### 0.1.0

- Adicionado o modelo `ErrorDetails` para padronizar respostas de erro.
