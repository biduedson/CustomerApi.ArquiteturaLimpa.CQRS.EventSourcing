# CustomerApi.Core

> 📘 Apostila técnica da camada **Core** do projeto `CustomerApi`, escrita em português do Brasil, com explicações detalhadas sobre DDD, CQRS, Clean Architecture, Event Sourcing, contratos, classes base, opções tipadas e extensões.

Este README foi pensado para estudo. A intenção não é apenas dizer "o que cada arquivo faz", mas explicar **por que ele existe**, **como ler o código**, **qual o sentido dos nomes em inglês** e **como essas peças ajudam a arquitetura do projeto**.

## 📚 Sumário

| Seção | Conteúdo |
|---|---|
| [1. Objetivo da camada Core](#1--objetivo-da-camada-core) | O que é a Core e por que ela existe |
| [2. Core em DDD, CQRS e Clean Architecture](#2--core-em-ddd-cqrs-e-clean-architecture) | Papel arquitetural da camada |
| [3. Relação com outras camadas](#3--relação-com-outras-camadas) | Como Core conversa com Domain, Application, Infrastructure e WebApi |
| [4. Estrutura de pastas](#4--estrutura-de-pastas) | Mapa da pasta `CustomerApi.Core` |
| [5. Visão rápida dos arquivos](#5--visão-rápida-dos-arquivos) | Tabela de arquivos e responsabilidades |
| [6. Explicação arquivo por arquivo](#6--explicação-arquivo-por-arquivo) | Detalhamento profundo de cada arquivo |
| [7. Fluxos principais](#7--fluxos-principais) | Exemplos práticos usando as peças da Core |
| [8. Padrões usados](#8--padrões-usados-na-core) | Repository, Unit of Work, Domain Events, Options Pattern e mais |
| [9. Glossário inglês-português](#9--glossário-inglês--português) | Tradução dos termos técnicos |
| [10. Como recriar do zero](#10--como-recriar-essa-camada-do-zero) | Roteiro para montar uma Core semelhante |

## 1. 🎯 Objetivo Da Camada Core

A camada `CustomerApi.Core` é o **núcleo técnico compartilhado** do projeto.

Ela contém estruturas que são úteis para várias partes da aplicação, mas que não pertencem exclusivamente a uma funcionalidade específica. Por exemplo: uma entidade base, um contrato de repositório, uma interface de unidade de trabalho ou uma extensão para serializar JSON.

Em outras palavras:

> A Core reúne peças pequenas, estáveis e reutilizáveis que ajudam as outras camadas a se comunicarem sem depender diretamente de detalhes externos.

### O que a Core deve conter?

| Deve conter | Por quê |
|---|---|
| Contratos | Para inverter dependências e evitar acoplamento com implementações |
| Classes base | Para padronizar entidades, eventos e comportamento comum |
| Tipos compartilhados | Para evitar duplicação em várias camadas |
| Extensões genéricas | Para reutilizar comportamentos técnicos recorrentes |
| Configurações tipadas | Para ler `appsettings.json` com segurança e clareza |

### O que a Core não deveria conter?

| Evite na Core | Motivo |
|---|---|
| Controllers | Pertencem à camada WebApi |
| Entity Framework específico | Pertence à Infrastructure |
| SQL, Mongo, Redis concretos | São detalhes externos |
| Regras específicas de endpoint | Pertencem à WebApi/Application |
| Caso de uso específico | Pertence à Application |
| Regra de negócio concreta de cliente | Pertence à Domain |

### Analogia simples

Pense na Core como uma **caixa de ferramentas arquitetural**:

```text
CustomerApi.Core
  fornece martelo, chave, régua e nível

Domain
  constrói as regras de negócio

Application
  organiza os casos de uso

Infrastructure
  conecta com banco, cache e serviços externos

WebApi
  expõe a aplicação via HTTP
```

A Core não constrói a casa sozinha. Ela fornece ferramentas confiáveis para as outras camadas construírem do jeito certo.

## 2. 🏛️ Core Em DDD, CQRS E Clean Architecture

## 2.1 DDD

DDD significa **Domain-Driven Design**, ou **Design Orientado ao Domínio**.

A ideia do DDD é colocar o domínio, ou seja, as regras importantes do negócio, no centro do software. A Core ajuda o domínio porque fornece conceitos fundamentais como entidade, agregado e evento.

### Conceitos de DDD presentes na Core

| Conceito | Tradução | Arquivo | Papel |
|---|---|---|---|
| `Entity` | Entidade | `IEntity.cs`, `BaseEntity.cs` | Objeto com identidade |
| `Aggregate Root` | Raiz de agregado | `IAggregateRoot.cs` | Entidade principal de um agregado |
| `Domain Event` | Evento de domínio | `BaseEvent.cs`, `BaseEntity.cs` | Algo importante que aconteceu no domínio |
| `Repository` | Repositório | `IWriteOnlyRepository.cs` | Contrato para persistência |
| `Shared Kernel` | Núcleo compartilhado | Pasta `SharedKernel` | Tipos comuns entre partes do sistema |

### Exemplo mental

Uma entidade de domínio pode herdar de `BaseEntity` para ganhar:

```text
Id
DomainEvents
AddDomainEvent()
ClearDomainEvents()
```

Assim, quando uma regra de negócio acontece, a entidade pode registrar um evento:

```csharp
AddDomainEvent(new CustomerCreatedEvent(...));
```

A Core não precisa saber o que é `CustomerCreatedEvent`. Ela só fornece a estrutura para que qualquer evento de domínio funcione.

## 2.2 CQRS

CQRS significa **Command Query Responsibility Segregation**, ou **Separação de Responsabilidades entre Comandos e Consultas**.

Na prática:

| Lado | Responsabilidade |
|---|---|
| Command | Escrever, alterar estado, executar ações |
| Query | Ler, consultar, montar respostas |

A Core deste projeto ajuda principalmente no lado **Command**, porque define contratos como:

| Arquivo | Papel no CQRS |
|---|---|
| `IWriteOnlyRepository.cs` | Repositório voltado para escrita |
| `IUnitOfWork.cs` | Confirma alterações feitas por comandos |
| `BaseEvent.cs` | Representa eventos após mudanças de estado |
| `EventStore.cs` | Representa evento serializado para histórico |
| `IResponse.cs` | Marca respostas de operações |

### Fluxo típico de comando

```text
Command recebido
        |
        v
Application executa caso de uso
        |
        v
Domain altera entidade
        |
        v
BaseEntity registra DomainEvents
        |
        v
Repository prepara persistência
        |
        v
IUnitOfWork.SaveChangesAsync()
```

A Core participa definindo as abstrações, não executando detalhes concretos.

## 2.3 Clean Architecture

Clean Architecture significa **Arquitetura Limpa**.

O objetivo é proteger regras importantes contra dependência de detalhes externos. Banco de dados, framework web e cache são detalhes. Eles podem mudar. As regras e contratos centrais devem mudar menos.

### Direção esperada das dependências

```text
WebApi
  -> Application
  -> Core

Infrastructure
  -> Core
  -> Domain
  -> Application

Application
  -> Core
  -> Domain

Domain
  -> Core

Core
  -> bibliotecas fundamentais
```

### Ideia principal

A Core define interfaces como:

```csharp
public interface IUnitOfWork
{
    Task SaveChangesAsync();
}
```

Ela não define:

```text
- qual banco será usado
- qual ORM será usado
- qual provider de cache será usado
- como a transação será aberta
- se o evento será salvo em SQL, NoSQL ou fila
```

Esses detalhes pertencem à `Infrastructure`.

## 3. 🔗 Relação Com Outras Camadas

Mesmo analisando somente a pasta `CustomerApi.Core`, dá para entender o papel dela pela natureza dos contratos que ela expõe.

| Camada | Como se relaciona com a Core |
|---|---|
| `Domain` | Usa tipos base como `BaseEntity`, `BaseEvent`, `IAggregateRoot`, `IEntity` e validações compartilhadas |
| `Application` | Usa contratos como `IUnitOfWork`, `IResponse`, `ICacheService` e extensões úteis |
| `Infrastructure` | Implementa interfaces da Core, como repositórios, cache e unidade de trabalho |
| `WebApi` | Pode registrar opções com `ConfigureAppSettings()` e consumir configurações tipadas |

### Por que isso é bom?

Porque as camadas dependem de **abstrações**, não de implementações.

Exemplo:

```csharp
public interface IEventStoreRepository : IDisposable
{
    Task StoreAsync(IEnumerable<EventStore> eventStores);
}
```

A Core diz:

> Deve existir algo capaz de armazenar eventos.

Mas ela não diz:

> Use SQL Server, MongoDB, Kafka, Redis ou arquivo local.

Essa separação preserva a arquitetura.

## 4. 🗂️ Estrutura De Pastas

```text
CustomerApi.Core
|-- AppSettings
|   |-- CacheOptions.cs
|   `-- ConnectionOptions.cs
|-- Extensions
|   |-- AssemblyExtensions.cs
|   |-- ConfigurationExtensions.cs
|   |-- GenericTypeExtensions.cs
|   |-- JsonExtensions.cs
|   `-- ServiceProviderExtensions.cs
|-- SharedKernel
|   |-- BaseEntity.cs
|   |-- BaseEvent.cs
|   |-- EventStore.cs
|   |-- IAggregateRoot.cs
|   |-- IAppOptions.cs
|   |-- ICacheService.cs
|   |-- IEntity.cs
|   |-- IEventStoreRepository.cs
|   |-- IResponse.cs
|   |-- IUnitOfWork.cs
|   `-- IWriteOnlyRepository.cs
|-- ConfigureServices.cs
|-- CustomerApi.Core.csproj
|-- README.md
`-- RegexPatterns.cs
```

### Responsabilidade por pasta

| Pasta | Tradução | Responsabilidade |
|---|---|---|
| `AppSettings` | Configurações da aplicação | Classes que representam seções do arquivo de configuração |
| `Extensions` | Extensões | Métodos utilitários adicionados a tipos existentes |
| `SharedKernel` | Núcleo compartilhado | Tipos arquiteturais comuns |

### Responsabilidade dos arquivos na raiz

| Arquivo | Responsabilidade |
|---|---|
| `CustomerApi.Core.csproj` | Define framework, pacotes e configuração do projeto |
| `ConfigureServices.cs` | Registra opções tipadas no container de DI |
| `RegexPatterns.cs` | Centraliza expressões regulares compartilhadas |
| `README.md` | Documenta a camada Core |

## 5. 🧭 Visão Rápida Dos Arquivos

| Arquivo | Tipo principal | Responsabilidade |
|---|---|---|
| `CustomerApi.Core.csproj` | Projeto `.NET` | Configuração de build e dependências |
| `ConfigureServices.cs` | Classe estática | Registro de opções tipadas |
| `RegexPatterns.cs` | Classe estática parcial | Regex compartilhada para e-mail |
| `AppSettings/CacheOptions.cs` | Classe selada | Configurações de cache |
| `AppSettings/ConnectionOptions.cs` | Classe selada | Configurações de conexão |
| `SharedKernel/IEntity.cs` | Interface | Contrato de entidade |
| `SharedKernel/BaseEntity.cs` | Classe abstrata | Base para entidades com eventos |
| `SharedKernel/BaseEvent.cs` | Classe abstrata | Base para eventos de domínio |
| `SharedKernel/EventStore.cs` | Classe concreta | Registro persistível de evento |
| `SharedKernel/IAggregateRoot.cs` | Interface marcadora | Marca raiz de agregado |
| `SharedKernel/IAppOptions.cs` | Interface | Contrato para options |
| `SharedKernel/ICacheService.cs` | Interface | Contrato para cache |
| `SharedKernel/IEventStoreRepository.cs` | Interface | Contrato para gravar eventos |
| `SharedKernel/IResponse.cs` | Interface marcadora | Marca respostas da aplicação |
| `SharedKernel/IUnitOfWork.cs` | Interface | Contrato para salvar alterações |
| `SharedKernel/IWriteOnlyRepository.cs` | Interface genérica | Contrato de repositório de escrita |
| `Extensions/AssemblyExtensions.cs` | Extensão | Busca tipos por interface em assembly |
| `Extensions/ConfigurationExtensions.cs` | Extensão | Lê opções tipadas de `IConfiguration` |
| `Extensions/GenericTypeExtensions.cs` | Extensão | Utilitários para tipos genéricos e valores padrão |
| `Extensions/JsonExtensions.cs` | Extensão | Serialização e desserialização JSON |
| `Extensions/ServiceProviderExtensions.cs` | Extensão | Obtém opções pelo `IServiceProvider` |

## 6. 🧩 Explicação Arquivo Por Arquivo

## 6.1 `CustomerApi.Core.csproj`

### Objetivo

Esse arquivo define a camada Core como um projeto `.NET`. Ele informa qual framework será usado, quais recursos do compilador estão ligados e quais pacotes NuGet são necessários.

### Trecho principal

```xml
<Project Sdk="Microsoft.NET.Sdk">
```

Esse trecho indica que o projeto usa o SDK padrão do .NET para bibliotecas e aplicações.

```xml
<TargetFramework>net9.0</TargetFramework>
```

Define que o projeto compila para `.NET 9.0`.

```xml
<ImplicitUsings>enable</ImplicitUsings>
```

Ativa `using` implícitos. Isso evita precisar escrever manualmente alguns namespaces comuns.

```xml
<Nullable>enable</Nullable>
```

Ativa análise de nulabilidade. Com isso, o compilador ajuda a perceber riscos de `NullReferenceException`.

### Dependências importantes

| Pacote | Para que serve |
|---|---|
| `MediatR` | Permite que eventos implementem `INotification` |
| `Microsoft.Extensions.Configuration` | Base para ler configurações |
| `Microsoft.Extensions.DependencyInjection` | Base para registrar serviços |
| `Microsoft.Extensions.Hosting` | Infraestrutura comum de hospedagem |
| `Microsoft.Extensions.Options` | Options Pattern |
| `Microsoft.Extensions.Options.ConfigurationExtensions` | Bind de configuração para options |
| `Microsoft.Extensions.Options.DataAnnotations` | Validação com `[Required]` e outros atributos |
| `System.Text.Json` | Serialização JSON |
| `Roslynator.*` | Analisadores, refatorações e sugestões de qualidade |

### Significado arquitetural

A Core depende apenas de bibliotecas transversais. Ela não referencia `Domain`, `Application`, `Infrastructure` nem `WebApi`.

Isso é importante porque a Core deve ser reutilizável e não deve conhecer as camadas que ficam ao redor dela.

## 6.2 `ConfigureServices.cs`

### Objetivo

Esse arquivo registra configurações tipadas no container de injeção de dependência.

Ele permite que a aplicação chame:

```csharp
services.ConfigureAppSettings();
```

e automaticamente registre:

```text
ConnectionOptions
CacheOptions
```

### Código completo comentado por partes

```csharp
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.SharedKernel;
using Microsoft.Extensions.DependencyInjection;
```

| `using` | Por que existe |
|---|---|
| `CustomerApi.Core.AppSettings` | Acessar `ConnectionOptions` e `CacheOptions` |
| `CustomerApi.Core.SharedKernel` | Acessar `IAppOptions` |
| `Microsoft.Extensions.DependencyInjection` | Acessar `IServiceCollection` e métodos de DI |

```csharp
namespace CustomerApi.Core;
```

Define o espaço de nomes da classe. Como o arquivo está na raiz da Core, o namespace também é `CustomerApi.Core`.

```csharp
public static class ConfigureServices
```

| Palavra | Explicação |
|---|---|
| `public` | Pode ser usada fora do projeto |
| `static` | Não precisa criar instância |
| `class` | Define uma classe |
| `ConfigureServices` | "Configurar serviços", nome comum para registros de DI |

### Método `ConfigureAppSettings`

```csharp
public static IServiceCollection ConfigureAppSettings(this IServiceCollection services) =>
    services
        .AddOptionsWithValidation<ConnectionOptions>()
        .AddOptionsWithValidation<CacheOptions>();
```

Esse método é público e pode ser chamado por outras camadas.

| Parte | Explicação |
|---|---|
| `IServiceCollection` | Coleção de serviços da aplicação |
| `this IServiceCollection services` | Torna o método uma extensão |
| `AddOptionsWithValidation<ConnectionOptions>()` | Registra opções de conexão |
| `AddOptionsWithValidation<CacheOptions>()` | Registra opções de cache |

### Por que usar método de extensão?

Sem método de extensão, o uso seria mais verboso:

```csharp
ConfigureServices.ConfigureAppSettings(services);
```

Com método de extensão, fica fluente:

```csharp
services.ConfigureAppSettings();
```

### Método privado `AddOptionsWithValidation<TOptions>`

```csharp
private static IServiceCollection AddOptionsWithValidation<TOptions>(this IServiceCollection services)
    where TOptions : class, IAppOptions
```

Esse método centraliza o padrão de registro de options. Assim, cada classe de configuração não precisa repetir o mesmo código.

| Trecho | Significado |
|---|---|
| `private` | Só pode ser usado dentro da própria classe |
| `TOptions` | Tipo genérico da opção |
| `where TOptions : class, IAppOptions` | O tipo precisa ser classe e implementar `IAppOptions` |

### Pipeline de configuração

```csharp
return services
    .AddOptions<TOptions>()
    .BindConfiguration(TOptions.ConfigSectionPath, binderOptions => binderOptions.BindNonPublicProperties = true)
    .ValidateDataAnnotations()
    .ValidateOnStart()
    .Services;
```

| Passo | Explicação |
|---|---|
| `AddOptions<TOptions>()` | Inicia o registro de options |
| `BindConfiguration(...)` | Liga uma seção do `appsettings.json` ao objeto |
| `TOptions.ConfigSectionPath` | Pega o caminho da seção definido pela classe |
| `BindNonPublicProperties = true` | Permite preencher propriedades com `private init` |
| `ValidateDataAnnotations()` | Valida atributos como `[Required]` |
| `ValidateOnStart()` | Valida quando a aplicação inicia |
| `.Services` | Retorna o `IServiceCollection` para continuar encadeando |

### Exemplo prático

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureAppSettings();
```

Depois disso, outra classe poderia receber:

```csharp
public MyService(IOptions<CacheOptions> cacheOptions)
{
    var options = cacheOptions.Value;
}
```

## 6.3 `RegexPatterns.cs`

### Objetivo

Centralizar expressões regulares compartilhadas.

Atualmente, o arquivo contém uma regex para validar e-mail:

```csharp
public static readonly Regex EmailIsValid = EmailRegexPatternAttr();
```

### Por que deixar regex na Core?

Porque validação de formato pode ser usada por mais de uma camada. Se cada camada criar sua própria regex, aparecem inconsistências.

Exemplo de problema:

```text
Domain aceita um e-mail
Application rejeita o mesmo e-mail
WebApi aceita outro formato diferente
```

Centralizar evita esse tipo de divergência.

### Classe `RegexPatterns`

```csharp
public static partial class RegexPatterns
```

| Termo | Tradução | Explicação |
|---|---|---|
| `Regex` | Expressão regular | Padrão textual para validar ou procurar texto |
| `Patterns` | Padrões | Conjunto de padrões reutilizáveis |
| `static` | Estático | Não precisa instanciar |
| `partial` | Parcial | Parte do código pode ser gerada pelo compilador |

### Campo `EmailIsValid`

```csharp
public static readonly Regex EmailIsValid = EmailRegexPatternAttr();
```

| Parte | Explicação |
|---|---|
| `public` | Pode ser usado fora da Core |
| `static` | Acessível pela classe, sem `new` |
| `readonly` | Só pode ser atribuído na inicialização |
| `Regex` | Tipo da expressão regular |
| `EmailIsValid` | Nome que significa "e-mail é válido" |

Uso:

```csharp
var isValid = RegexPatterns.EmailIsValid.IsMatch("ana@email.com");
```

### Método `EmailRegexPatternAttr`

```csharp
[GeneratedRegex(
    @"^([0-9a-zA-Z]([+\-_.][ 0-9a-zA-Z]+)*)+" +
    @"@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
private static partial Regex EmailRegexPatternAttr();
```

`GeneratedRegex` é um recurso moderno do .NET. Ele gera uma implementação otimizada da regex em tempo de compilação.

| Opção | Tradução | Efeito |
|---|---|---|
| `IgnoreCase` | Ignorar caixa | Não diferencia maiúsculas/minúsculas |
| `Compiled` | Compilada | Otimiza execução |
| `CultureInvariant` | Cultura invariável | Evita diferenças por idioma/região |

### Explicando a regex em alto nível

| Parte | Sentido |
|---|---|
| `^` | Começo da string |
| Parte antes do `@` | Nome do usuário do e-mail |
| `@` | Separador obrigatório |
| Parte depois do `@` | Domínio |
| `{2,17}` | Extensão final com 2 a 17 caracteres |
| `$` | Fim da string |

### Exemplo prático

```csharp
if (!RegexPatterns.EmailIsValid.IsMatch(email))
{
    throw new Exception("E-mail inválido.");
}
```

## 6.4 `AppSettings/CacheOptions.cs`

### Objetivo

Representar a seção de configuração relacionada a cache.

Em vez de espalhar strings como:

```csharp
configuration["CacheOptions:AbsoluteExpirationInHours"]
```

o projeto pode usar:

```csharp
cacheOptions.AbsoluteExpirationInHours
```

Isso é mais seguro, mais legível e mais fácil de testar.

### Classe `CacheOptions`

```csharp
public sealed class CacheOptions : IAppOptions
```

| Termo | Tradução | Explicação |
|---|---|---|
| `Cache` | Cache | Armazenamento temporário |
| `Options` | Opções | Configurações tipadas |
| `sealed` | Selada | Não pode ser herdada |
| `IAppOptions` | Interface de opções da aplicação | Obriga a informar a seção de configuração |

### Seção de configuração

```csharp
static string IAppOptions.ConfigSectionPath => nameof(CacheOptions);
```

Esse trecho implementa a propriedade estática exigida por `IAppOptions`.

`nameof(CacheOptions)` retorna:

```text
CacheOptions
```

Então a configuração esperada seria:

```json
{
  "CacheOptions": {
    "AbsoluteExpirationInHours": 4,
    "SlidingExpirationInSeconds": 120
  }
}
```

### Propriedades

```csharp
public int AbsoluteExpirationInHours { get; private init; }
public int SlidingExpirationInSeconds { get; private init; }
```

| Propriedade | Tradução | Significado |
|---|---|---|
| `AbsoluteExpirationInHours` | Expiração absoluta em horas | Tempo máximo que o item fica no cache |
| `SlidingExpirationInSeconds` | Expiração deslizante em segundos | Tempo renovado conforme o item é acessado |
| `private init` | Inicialização privada | Permite inicializar no bind, mas impede alteração comum |

### Diferença entre expiração absoluta e deslizante

| Tipo | Exemplo | Comportamento |
|---|---|---|
| Absoluta | 4 horas | Depois de 4 horas, expira mesmo que tenha sido acessado |
| Deslizante | 120 segundos | Se for acessado antes de 120 segundos, renova o prazo |

### Exemplo prático

```csharp
var options = serviceProvider.GetOptions<CacheOptions>();

Console.WriteLine(options.AbsoluteExpirationInHours);
Console.WriteLine(options.SlidingExpirationInSeconds);
```

## 6.5 `AppSettings/ConnectionOptions.cs`

### Objetivo

Representar a seção `ConnectionStrings`.

Essa classe agrupa strings de conexão usadas por recursos externos, como banco SQL, NoSQL e cache.

### Classe `ConnectionOptions`

```csharp
public sealed class ConnectionOptions : IAppOptions
```

| Termo | Tradução | Explicação |
|---|---|---|
| `Connection` | Conexão | Dados para acessar recurso externo |
| `Options` | Opções | Configuração tipada |
| `sealed` | Selada | Não permite herança |

### Caminho da seção

```csharp
static string IAppOptions.ConfigSectionPath => "ConnectionStrings";
```

Aqui o nome da seção não usa `nameof(ConnectionOptions)`. Ele aponta explicitamente para:

```text
ConnectionStrings
```

Exemplo esperado:

```json
{
  "ConnectionStrings": {
    "SqlConnection": "Server=localhost;Database=CustomerDb;",
    "NoSqlConnection": "mongodb://localhost:27017",
    "CacheConnection": "InMemory"
  }
}
```

### Propriedades obrigatórias

```csharp
[Required]
public string? SqlConnection { get; private init; }

[Required]
public string? NoSqlConnection { get; private init; }

[Required]
public string? CacheConnection { get; private init; }
```

| Propriedade | Tradução | Uso provável |
|---|---|---|
| `SqlConnection` | Conexão SQL | Banco relacional |
| `NoSqlConnection` | Conexão NoSQL | Banco não relacional |
| `CacheConnection` | Conexão de cache | Cache em memória, Redis ou similar |

`[Required]` vem de `System.ComponentModel.DataAnnotations`. Com `ValidateDataAnnotations()`, a aplicação valida se essas propriedades foram preenchidas.

### Por que `string?` se é obrigatório?

Porque antes do bind da configuração o valor pode estar nulo. O atributo `[Required]` valida em tempo de configuração, enquanto `string?` conversa com o compilador sobre nulabilidade.

### Método `CacheConnectionInMemory`

```csharp
public bool CacheConnectionInMemory() =>
    CacheConnection!.Equals("InMemory", StringComparison.InvariantCultureIgnoreCase);
```

Esse método responde:

> A configuração do cache indica uso em memória?

| Trecho | Explicação |
|---|---|
| `CacheConnection!` | O `!` diz ao compilador que o valor não será nulo aqui |
| `Equals("InMemory", ...)` | Compara com o texto `InMemory` |
| `InvariantCultureIgnoreCase` | Ignora maiúsculas/minúsculas de forma invariável |

### Exemplo prático

```csharp
if (connectionOptions.CacheConnectionInMemory())
{
    services.AddMemoryCache();
}
```

## 6.6 `SharedKernel/IEntity.cs`

### Objetivo

Definir o conceito mínimo de entidade.

Em DDD, uma entidade é um objeto com identidade. Duas entidades podem ter os mesmos dados, mas se o `Id` for diferente, elas representam coisas diferentes.

### Interface marcadora

```csharp
public interface IEntity;
```

Essa interface não possui membros. Ela serve como marca semântica:

> Este tipo é uma entidade.

### Interface genérica

```csharp
public interface IEntity<out TKey> : IEntity where TKey : IEquatable<TKey>
{
    TKey Id { get; }
}
```

| Parte | Explicação |
|---|---|
| `IEntity<TKey>` | Entidade com tipo de chave |
| `out TKey` | Covariância, útil para retornos genéricos |
| `where TKey : IEquatable<TKey>` | A chave precisa saber comparar igualdade |
| `TKey Id { get; }` | Toda entidade tem um identificador |

### Tradução dos nomes

| Inglês | Português | Sentido |
|---|---|---|
| `Entity` | Entidade | Objeto com identidade |
| `Key` | Chave | Tipo do identificador |
| `Id` | Identificador | Valor único da entidade |
| `Equatable` | Comparável | Pode ser comparado por igualdade |

### Exemplo prático

```csharp
public class Product : IEntity<Guid>
{
    public Guid Id { get; } = Guid.NewGuid();
}
```

## 6.7 `SharedKernel/BaseEntity.cs`

### Objetivo

Fornecer uma classe base para entidades com:

| Recurso | Papel |
|---|---|
| `Id` | Identidade da entidade |
| `_domainEvents` | Lista interna de eventos |
| `DomainEvents` | Exposição somente leitura dos eventos |
| `AddDomainEvent` | Método protegido para registrar evento |
| `ClearDomainEvents` | Método público para limpar eventos processados |

### Classe

```csharp
public abstract class BaseEntity : IEntity<Guid>
```

| Parte | Explicação |
|---|---|
| `abstract` | Não pode ser instanciada diretamente |
| `BaseEntity` | Entidade base |
| `IEntity<Guid>` | A chave padrão é `Guid` |

### Lista interna de eventos

```csharp
private readonly List<BaseEvent> _domainEvents = [];
```

Essa lista guarda eventos de domínio gerados pela entidade.

Ela é `private` para proteger a consistência. Código externo não deve adicionar eventos diretamente. A própria entidade deve decidir quando um evento acontece.

### Construtores

```csharp
protected BaseEntity() => Id = Guid.NewGuid();
protected BaseEntity(Guid id) => Id = id;
```

| Construtor | Uso |
|---|---|
| Sem parâmetro | Cria entidade nova com `Guid` automático |
| Com `Guid id` | Reconstrói entidade com ID existente |

`protected` significa que somente classes filhas podem chamar.

### Propriedade `DomainEvents`

```csharp
public IEnumerable<BaseEvent> DomainEvents =>
    _domainEvents.AsReadOnly();
```

Ela expõe os eventos como `IEnumerable<BaseEvent>`, mas usa `AsReadOnly()` para impedir alteração direta.

### Propriedade `Id`

```csharp
public Guid Id { get; private init; }
```

| Parte | Explicação |
|---|---|
| `Guid` | Identificador global único |
| `private init` | Só pode ser definido na inicialização pela própria classe |

### Método `AddDomainEvent`

```csharp
protected void AddDomainEvent(BaseEvent domainEvent) =>
    _domainEvents.Add(domainEvent);
```

Esse método registra um evento. Ele é `protected` para ser usado apenas pela entidade ou classes derivadas.

Isso é importante porque evento de domínio deve nascer de comportamento de domínio, não de qualquer código externo.

### Método `ClearDomainEvents`

```csharp
public void ClearDomainEvents() =>
    _domainEvents.Clear();
```

Limpa a lista depois que os eventos foram publicados ou armazenados.

### Exemplo prático

```csharp
public class Customer : BaseEntity, IAggregateRoot
{
    public void ChangeEmail(string newEmail)
    {
        AddDomainEvent(new CustomerEmailChangedEvent(Id, newEmail));
    }
}
```

### Leitura arquitetural

`BaseEntity` aproxima três ideias:

| Ideia | Como aparece |
|---|---|
| Entidade | Possui `Id` |
| DDD | Registra eventos de domínio |
| Event Sourcing | Eventos podem depois ser serializados e armazenados |

## 6.8 `SharedKernel/BaseEvent.cs`

### Objetivo

Fornecer uma base para eventos de domínio.

### Classe

```csharp
public abstract class BaseEvent : INotification
```

| Parte | Explicação |
|---|---|
| `abstract` | Evento genérico, não deve ser instanciado diretamente |
| `BaseEvent` | Evento base |
| `INotification` | Interface do MediatR para publicação de notificações |

### Por que implementar `INotification`?

Porque o MediatR consegue publicar qualquer evento que implemente `INotification`:

```csharp
await mediator.Publish(domainEvent);
```

Isso permite criar handlers de evento sem acoplar a entidade a quem vai reagir ao evento.

### Propriedades

```csharp
public string? MessageType { get; protected init; }
public Guid AggregateId { get; protected init; }
public DateTime OccurreedOn { get; private init; } = DateTime.Now;
```

| Propriedade | Tradução | Explicação |
|---|---|---|
| `MessageType` | Tipo da mensagem | Nome ou categoria do evento |
| `AggregateId` | ID do agregado | Identifica quem gerou o evento |
| `OccurreedOn` | Ocorreu em | Momento em que o evento foi criado |

### Observação sobre nome

`OccurreedOn` parece ter um erro de digitação em inglês. O termo mais comum seria:

```text
OccurredOn
```

O sentido arquitetural é:

> Quando esse evento ocorreu?

### Exemplo prático

```csharp
public class CustomerCreatedEvent : BaseEvent
{
    public CustomerCreatedEvent(Guid customerId)
    {
        AggregateId = customerId;
        MessageType = nameof(CustomerCreatedEvent);
    }
}
```

## 6.9 `SharedKernel/EventStore.cs`

### Objetivo

Representar um evento no formato pronto para armazenamento.

No contexto de Event Sourcing, um evento pode ser transformado em uma linha/tabela/documento contendo:

```text
Id
AggregateId
MessageType
Data
OccurreedOn
```

### Classe

```csharp
public class EventStore : BaseEvent
```

Ela herda de `BaseEvent`, então já carrega:

```text
MessageType
AggregateId
OccurreedOn
```

### Construtor principal

```csharp
public EventStore(Guid aggregateId, string messageType, string data)
{
    AggregateId = aggregateId;
    MessageType = messageType;
    Data = data;
}
```

| Parâmetro | Tradução | Explicação |
|---|---|---|
| `aggregateId` | ID do agregado | Quem gerou o evento |
| `messageType` | Tipo da mensagem | Nome do evento |
| `data` | Dados | Conteúdo serializado, normalmente JSON |

### Construtor vazio

```csharp
public EventStore()
{
}
```

Esse construtor ajuda ferramentas como ORMs e serializadores que precisam criar objetos sem passar parâmetros.

### Propriedades

```csharp
public Guid Id { get; private init; } = Guid.NewGuid();
public string? Data { get; private init; }
```

| Propriedade | Tradução | Explicação |
|---|---|---|
| `Id` | Identificador | Identidade do registro do evento |
| `Data` | Dados | Payload serializado do evento |

### Exemplo prático

```csharp
var eventStore = new EventStore(
    aggregateId: customerId,
    messageType: "CustomerCreatedEvent",
    data: "{\"firstName\":\"Ana\",\"email\":\"ana@email.com\"}");
```

### Diferença entre `BaseEvent` e `EventStore`

| Tipo | Papel |
|---|---|
| `BaseEvent` | Representa um fato de domínio em memória |
| `EventStore` | Representa esse fato no formato de armazenamento |

## 6.10 `SharedKernel/IAggregateRoot.cs`

### Objetivo

Marcar uma entidade como raiz de agregado.

```csharp
public interface IAggregateRoot;
```

### O que é Aggregate Root?

Em DDD, um agregado é um conjunto de objetos que deve ser alterado como uma unidade consistente.

A raiz do agregado é a entidade principal por onde as alterações devem passar.

| Termo | Tradução | Sentido |
|---|---|---|
| `Aggregate` | Agregado | Grupo de objetos com consistência conjunta |
| `Root` | Raiz | Entidade principal |
| `IAggregateRoot` | Interface de raiz de agregado | Marca a entrada principal do agregado |

### Por que a interface está vazia?

Porque é uma **interface marcadora**.

Ela comunica intenção:

```csharp
public class Customer : BaseEntity, IAggregateRoot
{
}
```

Isso diz:

> `Customer` é uma raiz de agregado.

## 6.11 `SharedKernel/IAppOptions.cs`

### Objetivo

Definir um contrato comum para classes de configuração.

```csharp
public interface IAppOptions
{
    static abstract string ConfigSectionPath { get; }
}
```

### Explicando `static abstract`

Esse recurso permite exigir que cada classe implemente uma propriedade estática.

Isso é útil porque o método genérico de configuração pode fazer:

```csharp
TOptions.ConfigSectionPath
```

sem saber se `TOptions` é `CacheOptions`, `ConnectionOptions` ou outra classe futura.

### Exemplo prático

```csharp
public sealed class CacheOptions : IAppOptions
{
    static string IAppOptions.ConfigSectionPath => nameof(CacheOptions);
}
```

### Tradução dos nomes

| Nome | Tradução | Sentido |
|---|---|---|
| `App` | Aplicação | O sistema em execução |
| `Options` | Opções | Configurações |
| `ConfigSectionPath` | Caminho da seção de configuração | Onde buscar no `appsettings.json` |

## 6.12 `SharedKernel/ICacheService.cs`

### Objetivo

Definir o contrato para serviço de cache.

Cache é um armazenamento temporário usado para evitar buscar ou calcular a mesma informação repetidamente.

### Interface

```csharp
public interface ICacheService
{
    Task<TItem> GetOrCreateAsync<TItem>(string cacheKey, Func<Task<TItem>> factory);
    Task<IReadOnlyList<TItem>> GetOrCreateAsync<TItem>(string cacheKey, Func<Task<IReadOnlyList<TItem>>> factory);
    Task RemoveAsync(params string[] cacheKeys);
}
```

### Método `GetOrCreateAsync<TItem>`

```csharp
Task<TItem> GetOrCreateAsync<TItem>(string cacheKey, Func<Task<TItem>> factory);
```

Esse método significa:

> Tente buscar o item no cache. Se não existir, execute a função `factory`, salve o resultado e retorne.

| Parte | Tradução | Explicação |
|---|---|---|
| `TItem` | Tipo do item | Tipo retornado pelo cache |
| `cacheKey` | Chave do cache | Nome usado para localizar o item |
| `factory` | Fábrica | Função que cria o valor caso ele não exista |
| `Task<TItem>` | Tarefa assíncrona | Operação assíncrona que retorna um item |

### Sobrecarga para lista

```csharp
Task<IReadOnlyList<TItem>> GetOrCreateAsync<TItem>(
    string cacheKey,
    Func<Task<IReadOnlyList<TItem>>> factory);
```

Essa versão faz a mesma coisa, mas para listas somente leitura.

### Método `RemoveAsync`

```csharp
Task RemoveAsync(params string[] cacheKeys);
```

Remove uma ou várias chaves.

`params` permite chamar assim:

```csharp
await cache.RemoveAsync("customer:1", "customer:list");
```

### Exemplo prático

```csharp
var customer = await cache.GetOrCreateAsync(
    cacheKey: $"customer:{id}",
    factory: () => repository.GetByIdAsync(id));
```

### Papel arquitetural

A Core define o contrato. A implementação pode usar:

```text
- cache em memória
- Redis
- banco NoSQL
- outro provider
```

## 6.13 `SharedKernel/IEventStoreRepository.cs`

### Objetivo

Definir o contrato para armazenar eventos.

```csharp
public interface IEventStoreRepository : IDisposable
{
    Task StoreAsync(IEnumerable<EventStore> eventStores);
}
```

### Método `StoreAsync`

`StoreAsync` significa **armazenar de forma assíncrona**.

Ele recebe:

```csharp
IEnumerable<EventStore> eventStores
```

Ou seja, uma coleção de eventos já preparados para armazenamento.

### Por que herda de `IDisposable`?

Porque a implementação pode usar recursos que precisam ser liberados:

```text
- DbContext
- conexão de banco
- stream
- client externo
```

### Exemplo prático

```csharp
var events = new[]
{
    new EventStore(customerId, "CustomerCreatedEvent", json)
};

await eventStoreRepository.StoreAsync(events);
```

## 6.14 `SharedKernel/IResponse.cs`

### Objetivo

Marcar classes que representam respostas da aplicação.

```csharp
public interface IResponse;
```

### Por que existe se não tem membros?

Porque é uma interface marcadora.

Ela comunica:

> Este tipo representa uma resposta de caso de uso.

### Exemplo prático

```csharp
public class CreatedCustomerResponse : IResponse
{
    public Guid Id { get; }

    public CreatedCustomerResponse(Guid id)
    {
        Id = id;
    }
}
```

### Valor arquitetural

Interfaces marcadoras ajudam a organizar intenção. Elas podem ser usadas futuramente para:

```text
- filtros genéricos
- validações
- convenções
- documentação
- constraints genéricas
```

## 6.15 `SharedKernel/IUnitOfWork.cs`

### Objetivo

Definir o contrato de Unidade de Trabalho.

```csharp
public interface IUnitOfWork
{
    Task SaveChangesAsync();
}
```

### O que é Unit of Work?

`Unit of Work` significa **Unidade de Trabalho**.

É um padrão que agrupa várias alterações e confirma tudo em um ponto único.

Em vez de cada repositório salvar sozinho:

```text
repository.Add()
repository.Save()
outroRepository.Update()
outroRepository.Save()
```

usa-se:

```text
repository.Add()
outroRepository.Update()
unitOfWork.SaveChangesAsync()
```

### Método `SaveChangesAsync`

```csharp
Task SaveChangesAsync();
```

| Parte | Explicação |
|---|---|
| `Task` | Operação assíncrona |
| `SaveChanges` | Salvar alterações |
| `Async` | Convenção de método assíncrono |

### Papel em CQRS

No lado de comandos, depois que a entidade é alterada e o repositório recebe a operação, o caso de uso chama:

```csharp
await unitOfWork.SaveChangesAsync();
```

A Core não sabe como isso será feito. A Infrastructure implementa.

## 6.16 `SharedKernel/IWriteOnlyRepository.cs`

### Objetivo

Definir um contrato genérico para repositórios de escrita.

```csharp
public interface IWriteOnlyRepository<TEntity, in TKey> : IDisposable
    where TEntity : IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<TEntity> GetByIdAsync(TKey id);
}
```

### Tradução do nome

`WriteOnlyRepository` significa **repositório somente de escrita**.

Em CQRS, ele representa o lado de comandos, não o lado de consultas.

### Parâmetros genéricos

| Genérico | Tradução | Exemplo |
|---|---|---|
| `TEntity` | Tipo da entidade | `Customer` |
| `TKey` | Tipo da chave | `Guid` |

### Constraints

```csharp
where TEntity : IEntity<TKey>
where TKey : IEquatable<TKey>
```

Essas regras dizem:

| Constraint | Significado |
|---|---|
| `TEntity : IEntity<TKey>` | A entidade precisa ter `Id` do tipo `TKey` |
| `TKey : IEquatable<TKey>` | A chave precisa ser comparável |

### Métodos

| Método | Tradução | Papel |
|---|---|---|
| `Add` | Adicionar | Coloca entidade para inserção |
| `Update` | Atualizar | Coloca entidade para alteração |
| `Remove` | Remover | Coloca entidade para exclusão |
| `GetByIdAsync` | Obter por ID assíncrono | Busca entidade antes de alterar/remover |

### Por que existe `GetByIdAsync` se é WriteOnly?

Porque comandos de escrita muitas vezes precisam carregar a entidade antes de alterar.

Exemplo:

```text
UpdateCustomerCommand
  -> buscar Customer por Id
  -> alterar email
  -> chamar Update
  -> salvar UnitOfWork
```

Essa busca não é uma query de tela. É uma busca necessária para executar uma escrita.

### Exemplo prático

```csharp
public interface ICustomerWriteOnlyRepository
    : IWriteOnlyRepository<Customer, Guid>
{
    Task<bool> ExistsByEmailAsync(string email);
}
```

## 6.17 `Extensions/AssemblyExtensions.cs`

### Objetivo

Fornecer uma extensão para buscar tipos concretos dentro de um assembly.

### Código

```csharp
public static IEnumerable<Type> GetAllTypesOf<TInterface>(this Assembly assembly)
{
    var isAssignableToInterface = typeof(TInterface).IsAssignableFrom;
    return [..assembly
        .GetTypes()
        .Where(type => type.IsClass && !type.IsAbstract && !type.IsInterface && isAssignableToInterface(type))
        ];
}
```

### Tradução dos nomes

| Nome | Tradução | Explicação |
|---|---|---|
| `Assembly` | Conjunto compilado | Normalmente uma DLL |
| `GetAllTypesOf` | Obter todos os tipos de | Busca tipos que implementam algo |
| `TInterface` | Tipo da interface | Interface usada como filtro |
| `IsAssignableFrom` | É atribuível a partir de | Verifica compatibilidade de tipos |

### Linha por linha

```csharp
var isAssignableToInterface = typeof(TInterface).IsAssignableFrom;
```

Cria uma função para testar se um tipo implementa a interface desejada.

```csharp
assembly.GetTypes()
```

Obtém todos os tipos existentes naquele assembly.

```csharp
.Where(type => type.IsClass && !type.IsAbstract && !type.IsInterface && isAssignableToInterface(type))
```

Filtra somente tipos:

| Condição | Significado |
|---|---|
| `type.IsClass` | Precisa ser classe |
| `!type.IsAbstract` | Não pode ser abstrata |
| `!type.IsInterface` | Não pode ser interface |
| `isAssignableToInterface(type)` | Precisa implementar a interface |

```csharp
return [.. assembly.GetTypes().Where(...)]
```

Cria uma coleção com os tipos encontrados.

### Exemplo prático

```csharp
var handlers = assembly.GetAllTypesOf<ICommandHandler>();
```

Esse tipo de extensão pode ser usado para registro automático de serviços.

## 6.18 `Extensions/ConfigurationExtensions.cs`

### Objetivo

Facilitar a leitura de opções tipadas diretamente a partir de `IConfiguration`.

### Código

```csharp
public static TOptions? GetOptions<TOptions>(this IConfiguration configuration)
    where TOptions : class, IAppOptions
{
    return configuration
        .GetRequiredSection(TOptions.ConfigSectionPath)
        .Get<TOptions>(options => options.BindNonPublicProperties = true);
}
```

### Explicação

| Trecho | Explicação |
|---|---|
| `this IConfiguration configuration` | Cria método de extensão para `IConfiguration` |
| `where TOptions : class, IAppOptions` | Só aceita classes de options do projeto |
| `GetRequiredSection` | Exige que a seção exista |
| `TOptions.ConfigSectionPath` | Caminho fornecido pela própria classe |
| `Get<TOptions>` | Converte configuração em objeto |
| `BindNonPublicProperties = true` | Permite preencher `private init` |

### Exemplo prático

```csharp
var cacheOptions = configuration.GetOptions<CacheOptions>();
```

### Por que isso é melhor que string solta?

Menos seguro:

```csharp
var value = configuration["CacheOptions:AbsoluteExpirationInHours"];
```

Mais seguro:

```csharp
var options = configuration.GetOptions<CacheOptions>();
var value = options.AbsoluteExpirationInHours;
```

## 6.19 `Extensions/GenericTypeExtensions.cs`

### Objetivo

Fornecer utilitários para:

```text
- verificar valor padrão
- formatar nomes de tipos genéricos
```

### Método `IsDefault<T>`

```csharp
public static bool IsDefault<T>(this T value) =>
    Equals(value, default(T));
```

Esse método verifica se um valor é igual ao valor padrão do seu tipo.

| Tipo | Valor padrão |
|---|---|
| `string` | `null` |
| `int` | `0` |
| `Guid` | `Guid.Empty` |
| `bool` | `false` |
| Classe | `null` |

### Exemplo prático

```csharp
string? text = null;
var result = text.IsDefault(); // true
```

### Método `GetGenericTypeName`

```csharp
public static string GetGenericTypeName(this object @object)
```

Esse método retorna um nome legível para tipos genéricos.

### Por que isso existe?

O .NET representa tipos genéricos internamente com crase:

```text
Result`1
Dictionary`2
```

Para logs, isso é feio e pouco didático.

Esse método transforma em:

```text
Result<CreatedCustomerResponse>
Dictionary<String,Int32>
```

### Código principal

```csharp
var type = @object.GetType();
if (!type.IsGenericType)
    return type.Name;
```

Se o objeto não for genérico, retorna o nome normal.

```csharp
var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
```

Obtém os nomes dos tipos internos.

```csharp
return $"{type.Name[..type.Name.IndexOf('`')]}<{genericTypes}>";
```

Remove a parte com crase e monta um nome mais amigável.

### Exemplo prático

```csharp
var commandName = request.GetGenericTypeName();
```

Muito útil para logs de comandos e handlers.

## 6.20 `Extensions/JsonExtensions.cs`

### Objetivo

Centralizar serialização e desserialização JSON com opções padronizadas.

### Por que centralizar JSON?

Porque se cada lugar configurar `JsonSerializerOptions` de um jeito, o sistema pode serializar o mesmo objeto de formas diferentes.

Centralizando, você garante:

```text
- camelCase
- enum como string
- ignorar null
- suporte a construtores privados
- comportamento consistente
```

### Campo `LazyOptions`

```csharp
private static readonly Lazy<JsonSerializerOptions> LazyOptions =
    new(() => new JsonSerializerOptions().Configure(), isThreadSafe: true);
```

| Parte | Explicação |
|---|---|
| `Lazy<T>` | Cria o objeto apenas quando for usado |
| `JsonSerializerOptions` | Configurações do serializador JSON |
| `Configure()` | Método da própria Core que aplica padrões |
| `isThreadSafe: true` | Seguro para múltiplas threads |

### Método `FromJson<T>`

```csharp
public static T? FromJson<T>(this string value) =>
    value != null ? JsonSerializer.Deserialize<T>(value, LazyOptions.Value) : default;
```

Converte string JSON em objeto.

| Caso | Resultado |
|---|---|
| `value != null` | Desserializa |
| `value == null` | Retorna `default` |

Exemplo:

```csharp
var user = json.FromJson<User>();
```

### Método `ToJson<T>`

```csharp
public static string? ToJson<T>(this T value) =>
   !value.IsDefault() ? JsonSerializer.Serialize(value, LazyOptions.Value) : default;
```

Converte objeto em JSON.

Antes de serializar, verifica se o valor é default.

Exemplo:

```csharp
var json = user.ToJson();
```

### Método `Configure`

```csharp
public static JsonSerializerOptions Configure(this JsonSerializerOptions jsonSettings)
```

Esse método aplica as configurações padrão.

| Configuração | Valor | Efeito |
|---|---|---|
| `WriteIndented` | `false` | JSON compacto |
| `DefaultIgnoreCondition` | `WhenWritingNull` | Ignora propriedades nulas |
| `ReadCommentHandling` | `Skip` | Ignora comentários ao ler JSON |
| `PropertyNamingPolicy` | `CamelCase` | Usa `firstName`, `dateOfBirth` |
| `TypeInfoResolver` | `PrivateConstructorContractResolver` | Permite criar objetos com construtor privado |
| `JsonStringEnumConverter` | `CamelCase` | Enum vira texto em camelCase |

### Classe interna `PrivateConstructorContractResolver`

```csharp
internal sealed class PrivateConstructorContractResolver : DefaultJsonTypeInfoResolver
```

Essa classe customiza como o `System.Text.Json` cria objetos.

### Método `GetTypeInfo`

```csharp
public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
```

Ele obtém metadados do tipo e ajusta a criação do objeto quando necessário.

### Trecho mais importante

```csharp
if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object
    && jsonTypeInfo.CreateObject is null
    && jsonTypeInfo.Type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length == 0)
{
    jsonTypeInfo.CreateObject = () => Activator.CreateInstance(jsonTypeInfo.Type, true)!;
}
```

| Condição | Explicação |
|---|---|
| `Kind == Object` | O tipo é um objeto |
| `CreateObject is null` | O serializador não sabe criar sozinho |
| Sem construtor público | O tipo protege sua criação |
| `Activator.CreateInstance(..., true)` | Cria mesmo com construtor privado |

### Por que isso importa em DDD?

Em DDD é comum proteger construtores para forçar criação por métodos de fábrica:

```csharp
public sealed class Email
{
    private Email(string value)
    {
        Address = value;
    }

    public static Email Create(string value)
    {
        return new Email(value);
    }
}
```

Mas serializadores podem precisar reconstruir objetos. Essa extensão ajuda nesse cenário.

## 6.21 `Extensions/ServiceProviderExtensions.cs`

### Objetivo

Obter options tipadas diretamente de `IServiceProvider`.

### Código

```csharp
public static TOptions GetOptions<TOptions>(this IServiceProvider serviceProvider)
    where TOptions : class, IAppOptions =>
    serviceProvider.GetService<IOptions<TOptions>>()?.Value!;
```

### Tradução dos nomes

| Nome | Tradução | Explicação |
|---|---|---|
| `ServiceProvider` | Provedor de serviços | Resolve dependências registradas |
| `GetOptions` | Obter opções | Retorna configurações tipadas |
| `IOptions<TOptions>` | Opções tipadas | Padrão oficial do .NET para configuração |

### Explicação por partes

```csharp
serviceProvider.GetService<IOptions<TOptions>>()
```

Busca no container um serviço do tipo `IOptions<TOptions>`.

```csharp
?.Value
```

Acessa o valor das options, caso o serviço exista.

```csharp
!
```

Diz ao compilador que o retorno não será nulo.

### Exemplo prático

```csharp
var cacheOptions = serviceProvider.GetOptions<CacheOptions>();
```

## 7. 🚀 Fluxos Principais

## 7.1 Fluxo de configuração tipada

```text
Classe implementa IAppOptions
        |
        v
Define ConfigSectionPath
        |
        v
ConfigureAppSettings registra no IServiceCollection
        |
        v
IOptions<TOptions> fica disponível
        |
        v
ServiceProviderExtensions.GetOptions<TOptions>() pode recuperar
```

### Exemplo

```csharp
services.ConfigureAppSettings();

var cacheOptions = serviceProvider.GetOptions<CacheOptions>();
```

### O que cada peça faz?

| Peça | Função |
|---|---|
| `IAppOptions` | Obriga a informar seção de configuração |
| `CacheOptions` | Representa valores da seção `CacheOptions` |
| `ConfigureServices` | Registra e valida options |
| `ServiceProviderExtensions` | Facilita recuperar options |

## 7.2 Fluxo de entidade e evento de domínio

```text
Entidade herda BaseEntity
        |
        v
Método de negócio executa mudança
        |
        v
Entidade chama AddDomainEvent()
        |
        v
Evento herda BaseEvent
        |
        v
Evento fica em DomainEvents
        |
        v
UnitOfWork ou infraestrutura processa depois
```

### Exemplo

```csharp
public class Customer : BaseEntity, IAggregateRoot
{
    public void ChangeEmail(string email)
    {
        AddDomainEvent(new CustomerEmailChangedEvent(Id, email));
    }
}
```

## 7.3 Fluxo de Event Store

```text
BaseEvent nasce no domínio
        |
        v
Evento é serializado com ToJson()
        |
        v
EventStore recebe AggregateId, MessageType e Data
        |
        v
IEventStoreRepository.StoreAsync grava o evento
```

### Exemplo

```csharp
var json = domainEvent.ToJson();

var eventStore = new EventStore(
    aggregateId: domainEvent.AggregateId,
    messageType: domainEvent.MessageType!,
    data: json!);

await eventStoreRepository.StoreAsync([eventStore]);
```

## 7.4 Fluxo de JSON

```text
Objeto
  -> ToJson()
  -> JsonSerializer com opções da Core
  -> string JSON

string JSON
  -> FromJson<T>()
  -> objeto tipado
```

### Exemplo

```csharp
var json = customer.ToJson();
var restoredCustomer = json.FromJson<Customer>();
```

## 7.5 Fluxo de repositório de escrita

```text
Application recebe comando
        |
        v
Busca entidade com GetByIdAsync quando necessário
        |
        v
Executa método de domínio
        |
        v
Chama Add, Update ou Remove
        |
        v
Confirma com IUnitOfWork.SaveChangesAsync()
```

### Exemplo

```csharp
var customer = await repository.GetByIdAsync(id);

customer.ChangeEmail(newEmail);

repository.Update(customer);

await unitOfWork.SaveChangesAsync();
```

## 8. 🧱 Padrões Usados Na Core

| Padrão | Arquivos | Explicação |
|---|---|---|
| Shared Kernel | `SharedKernel/*` | Núcleo compartilhado de conceitos |
| Entity | `IEntity.cs`, `BaseEntity.cs` | Objeto com identidade |
| Aggregate Root | `IAggregateRoot.cs` | Marca entidade principal do agregado |
| Domain Events | `BaseEvent.cs`, `BaseEntity.cs` | Registra fatos relevantes do domínio |
| Event Store | `EventStore.cs`, `IEventStoreRepository.cs` | Armazena histórico de eventos |
| Repository | `IWriteOnlyRepository.cs` | Abstrai persistência |
| Unit of Work | `IUnitOfWork.cs` | Coordena salvamento |
| Options Pattern | `IAppOptions.cs`, `CacheOptions.cs`, `ConnectionOptions.cs` | Configuração tipada |
| Extension Methods | `Extensions/*` | Adiciona métodos utilitários a tipos existentes |
| Marker Interface | `IEntity`, `IResponse`, `IAggregateRoot` | Comunica intenção arquitetural |
| Generics | `IEntity<TKey>`, `IWriteOnlyRepository<TEntity, TKey>` | Reutiliza contratos para vários tipos |

## 9. 🧠 Glossário Inglês → Português

| Inglês | Português | Sentido no projeto |
|---|---|---|
| `Core` | Núcleo | Camada central e reutilizável |
| `SharedKernel` | Núcleo compartilhado | Tipos comuns entre camadas |
| `AppSettings` | Configurações da aplicação | Classes que representam configurações |
| `Extensions` | Extensões | Métodos adicionados a tipos existentes |
| `Entity` | Entidade | Objeto com identidade própria |
| `BaseEntity` | Entidade base | Classe mãe para entidades |
| `Event` | Evento | Algo que aconteceu |
| `BaseEvent` | Evento base | Classe mãe para eventos |
| `DomainEvent` | Evento de domínio | Fato relevante para o negócio |
| `EventStore` | Armazenamento de eventos | Registro persistível de evento |
| `Aggregate` | Agregado | Grupo de objetos consistentes juntos |
| `AggregateRoot` | Raiz de agregado | Entidade principal do agregado |
| `Repository` | Repositório | Abstração de persistência |
| `WriteOnly` | Somente escrita | Voltado ao lado Command do CQRS |
| `UnitOfWork` | Unidade de trabalho | Coordena transação/salvamento |
| `Options` | Opções | Configurações tipadas |
| `Cache` | Cache | Armazenamento temporário |
| `Connection` | Conexão | Dados para acessar recurso externo |
| `Assembly` | Conjunto compilado | DLL ou executável .NET |
| `Generic` | Genérico | Código parametrizado por tipo |
| `Response` | Resposta | Objeto de retorno |
| `ServiceProvider` | Provedor de serviços | Resolve dependências |
| `Configuration` | Configuração | Fonte de valores da aplicação |
| `Regex` | Expressão regular | Padrão textual |

## 10. 🛠️ Como Recriar Essa Camada Do Zero

## 10.1 Criar o projeto

```bash
dotnet new classlib -n CustomerApi.Core
```

## 10.2 Configurar o `.csproj`

```xml
<TargetFramework>net9.0</TargetFramework>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
```

Adicionar pacotes conforme necessidade:

```text
MediatR
Microsoft.Extensions.Configuration
Microsoft.Extensions.DependencyInjection
Microsoft.Extensions.Options
System.Text.Json
```

## 10.3 Criar `SharedKernel`

Comece pelos contratos mais básicos:

```text
IEntity
BaseEntity
BaseEvent
IAggregateRoot
```

Depois adicione contratos de aplicação e persistência:

```text
IUnitOfWork
IWriteOnlyRepository
IEventStoreRepository
IResponse
ICacheService
```

## 10.4 Criar `AppSettings`

Crie:

```text
IAppOptions
CacheOptions
ConnectionOptions
```

Use `IAppOptions` para obrigar cada classe a informar seu caminho de configuração.

## 10.5 Criar `Extensions`

Adicione extensões transversais:

```text
AssemblyExtensions
ConfigurationExtensions
GenericTypeExtensions
JsonExtensions
ServiceProviderExtensions
```

## 10.6 Criar registro de serviços

Crie um arquivo como `ConfigureServices.cs`:

```csharp
public static IServiceCollection ConfigureAppSettings(this IServiceCollection services) =>
    services
        .AddOptionsWithValidation<ConnectionOptions>()
        .AddOptionsWithValidation<CacheOptions>();
```

## 10.7 Manter a Core limpa

Use esta regra:

| Pode entrar na Core | Melhor deixar fora |
|---|---|
| Interfaces | Implementações de banco |
| Classes base | Controllers |
| Eventos base | Endpoints HTTP |
| Opções tipadas | Entity Framework específico |
| Métodos de extensão genéricos | Redis concreto |
| Contratos arquiteturais | SQL, Mongo ou provider externo |

## ✅ Resumo Final

A `CustomerApi.Core` é a camada que fornece o vocabulário técnico compartilhado do projeto.

Ela não é uma camada de regra de negócio específica, nem uma camada de infraestrutura. Ela é a base que permite que as outras camadas trabalhem com conceitos comuns:

```text
Entidade
Evento
Agregado
Repositório
Unidade de Trabalho
Event Store
Options
JSON
Cache
Extensões
```

Em DDD, ela fornece base para entidades, agregados e eventos. Em CQRS, ela ajuda o lado de comandos com repositórios de escrita e unidade de trabalho. Em Clean Architecture, ela mantém as dependências apontando para abstrações, não para detalhes.

Essa é a força da Core: ela é pequena, mas sustenta a arquitetura inteira.
