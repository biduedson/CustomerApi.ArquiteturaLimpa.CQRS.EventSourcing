# CustomerApi.BlazorUI - MudBlazor Guide

Este README explica como a camada `CustomerApi.BlazorUI` usa MudBlazor. A ideia e servir como guia de estudo para voce entender quais componentes foram usados, onde foram usados e por que eles fazem sentido em cada tela.

## Objetivo Da UI

Esta camada e uma aplicacao Blazor Server que consome a `CustomerApi.WebApi`.

Ela continua separada assim:

- `Components/Pages`: paginas navegaveis.
- `Components/Pages/*/Components`: componentes especificos de cada area.
- `Components/Shared`: componentes reutilizaveis.
- `Services/ApiClients`: clientes HTTP que chamam a API.
- `Services/Authentication`: autenticacao, cookies e refresh.
- `Services/Notifications`: servico de notificacao usado pela UI.
- `Models`: modelos usados pelos formularios e respostas da API.

MudBlazor foi usado somente para melhorar a camada visual. A regra de negocio, chamadas HTTP, autenticacao e separacao de responsabilidades continuam iguais.

## Configuracao Do MudBlazor

### Pacote

O pacote foi adicionado no projeto:

```xml
<PackageReference Include="MudBlazor" />
```

Como o projeto usa gerenciamento central de pacotes, a versao fica em `Directory.Packages.props`:

```xml
<PackageVersion Include="MudBlazor" Version="9.5.0" />
```

### Program.cs

O MudBlazor precisa registrar seus servicos:

```csharp
using MudBlazor.Services;

builder.Services.AddMudServices();
```

Isso habilita servicos internos do MudBlazor, como snackbar, dialog, popover, resize listener e outros recursos usados pelos componentes.

### _Imports.razor

Foi adicionado:

```razor
@using MudBlazor
```

Assim os componentes podem ser usados sem precisar importar `MudBlazor` em cada arquivo.

### App.razor

Foram adicionados os arquivos CSS e JS do MudBlazor:

```razor
<link rel="stylesheet" href="@Assets["_content/MudBlazor/MudBlazor.min.css"]" />
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

Tambem foram adicionados os providers:

```razor
<MudThemeProvider Theme="_theme" IsDarkMode="true" DefaultScrollbar="true" />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

Esses providers sao importantes:

- `MudThemeProvider`: aplica o tema global.
- `MudPopoverProvider`: usado por menus, selects, date pickers e popovers.
- `MudDialogProvider`: base para dialogs do MudBlazor.
- `MudSnackbarProvider`: habilita notificacoes no canto da tela.

## Tema Escuro

O tema escuro foi configurado em `Components/App.razor` com `MudTheme`.

Principais propriedades usadas:

- `PaletteDark.Primary`: cor principal da UI.
- `PaletteDark.Secondary`: cor secundaria.
- `PaletteDark.Background`: fundo geral.
- `PaletteDark.Surface`: fundo de cards, tabelas e paineis.
- `PaletteDark.AppbarBackground`: fundo da barra superior.
- `PaletteDark.DrawerBackground`: fundo do menu lateral.
- `PaletteDark.TextPrimary`: texto principal.
- `PaletteDark.TextSecondary`: texto secundario.
- `LayoutProperties.DefaultBorderRadius`: arredondamento padrao.
- `Typography.Default.FontFamily`: fonte padrao.

Exemplo:

```csharp
private readonly MudTheme _theme = new()
{
    PaletteDark = new PaletteDark
    {
        Primary = "#8b5cf6",
        Secondary = "#22d3ee",
        Background = "#070b18",
        Surface = "#111827"
    }
};
```

## Componentes MudBlazor Mais Usados

### MudLayout

Usado para criar a estrutura principal da aplicacao.

Arquivo:

```text
Components/Layout/MainLayout.razor
```

Uso:

```razor
<MudLayout>
    <MudDrawer />
    <MudMainContent />
</MudLayout>
```

Ele organiza a tela em menu lateral, topo e conteudo.

### MudDrawer

Usado como menu lateral fixo.

Arquivo:

```text
Components/Layout/MainLayout.razor
```

Ele substitui o antigo `aside` com Bootstrap/CSS manual.

### MudAppBar

Usado na barra superior da aplicacao.

Arquivo:

```text
Components/Layout/MainLayout.razor
```

Serve para exibir o titulo do painel e contexto da aplicacao.

### MudNavMenu E MudNavLink

Usados no menu lateral.

Arquivo:

```text
Components/Layout/NavMenu.razor
```

Exemplo:

```razor
<MudNavMenu>
    <MudNavLink Href="" Icon="@Icons.Material.Filled.Dashboard">
        Dashboard
    </MudNavLink>
</MudNavMenu>
```

`MudNavLink` funciona parecido com `NavLink` do Blazor, mas ja vem com visual do MudBlazor e suporte facil a icones.

### MudPaper

Usado como base visual para cards, paineis, formularios e modais.

Arquivos onde aparece:

- `Home.razor`
- `Login.razor`
- `CustomerTable.razor`
- `UserTable.razor`
- `AccountIdentityCard.razor`
- `ConfirmDeleteModal.razor`
- varios outros componentes.

Pense nele como uma "caixa visual" reutilizavel.

Exemplo:

```razor
<MudPaper Class="work-card" Elevation="0">
    Conteudo aqui
</MudPaper>
```

### MudStack

Usado para alinhar elementos em coluna ou linha sem precisar escrever muito CSS.

Exemplo em coluna:

```razor
<MudStack Spacing="3">
    <MudText>Texto 1</MudText>
    <MudText>Texto 2</MudText>
</MudStack>
```

Exemplo em linha:

```razor
<MudStack Row AlignItems="AlignItems.Center" Spacing="2">
    <MudIcon />
    <MudText>Texto</MudText>
</MudStack>
```

Foi muito usado para evitar `div` demais.

### MudGrid E MudItem

Usados para layouts responsivos em formularios e detalhes.

Arquivos:

- `CustomerForm.razor`
- `CustomerCreateModal.razor`
- `UserCreateModal.razor`
- `UserManagementModal.razor`
- `UserDetails.razor`
- `AccountPasswordForm.razor`

Exemplo:

```razor
<MudGrid>
    <MudItem xs="12" md="6">
        <MudTextField Label="Nome" />
    </MudItem>
</MudGrid>
```

Significa:

- `xs="12"`: no celular ocupa a linha inteira.
- `md="6"`: em tela media ocupa metade da linha.

### MudText

Usado para textos com tipografia padronizada.

Exemplo:

```razor
<MudText Typo="Typo.h5"><b>Clientes</b></MudText>
<MudText Color="Color.Secondary">Descricao</MudText>
```

Evita ficar misturando `h1`, `p`, `span` e classes manuais.

### MudButton

Usado para botoes principais, secundarios, cancelar, excluir e navegar.

Exemplo:

```razor
<MudButton Variant="Variant.Filled"
           Color="Color.Primary"
           StartIcon="@Icons.Material.Filled.Add">
    Novo cliente
</MudButton>
```

Propriedades importantes:

- `Variant.Filled`: botao preenchido.
- `Variant.Outlined`: botao com borda.
- `Color.Primary`: cor principal.
- `Color.Error`: cor de perigo.
- `ButtonType="ButtonType.Submit"`: usado dentro de formularios.
- `Href`: transforma o botao em link.
- `OnClick`: executa metodo C#.

### MudIcon E Icons.Material.Filled

Usados para icones.

Exemplo:

```razor
<MudIcon Icon="@Icons.Material.Filled.Lock" />
```

Os icones ajudam a deixar a UI mais clara sem criar imagens manuais.

### MudIconButton

Usado principalmente em botoes de fechar modal.

Exemplo:

```razor
<MudIconButton Icon="@Icons.Material.Filled.Close"
               Color="Color.Secondary"
               OnClick="Close" />
```

### MudAvatar

Usado na marca do sistema, com a letra `C`.

Arquivos:

- `MainLayout.razor`
- `Login.razor`

### MudTable

Usado para tabelas de clientes e usuarios.

Arquivos:

```text
Components/Pages/Customers/Components/CustomerTable.razor
Components/Pages/Users/Components/UserTable.razor
```

Exemplo:

```razor
<MudTable Items="Customers" Hover="true">
    <HeaderContent>
        <MudTh>Nome</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.Email</MudTd>
    </RowTemplate>
</MudTable>
```

Pontos importantes:

- `Items`: lista exibida.
- `HeaderContent`: cabecalho.
- `RowTemplate`: linha da tabela.
- `context`: item atual da linha.
- `MudTh`: coluna do cabecalho.
- `MudTd`: celula da linha.

### MudChip

Usado para badges/status.

Arquivos:

- `CustomerTable.razor`
- `CustomerCardList.razor`
- `UserTable.razor`
- `UserCardList.razor`
- `UserDetails.razor`

Exemplo:

```razor
<MudChip T="string" Color="Color.Success" Variant="Variant.Filled">
    Ativo
</MudChip>
```

Foi usado para:

- genero do cliente;
- role do usuario;
- status ativo/inativo.

### MudTabs E MudTabPanel

Usados nos modais de gerenciamento.

Arquivos:

```text
CustomerManagementModal.razor
UserManagementModal.razor
```

Exemplo:

```razor
<MudTabs>
    <MudTabPanel Text="Detalhes">
        Conteudo
    </MudTabPanel>
</MudTabs>
```

Isso substitui o controle manual de abas com botoes e variavel string.

No projeto, ainda existe estado:

```csharp
private int _tabIndex;
```

Ele guarda qual aba esta ativa.

### MudAlert

Usado para mensagens de erro, avisos e confirmacoes perigosas.

Arquivos:

- `Login.razor`
- `CustomerCreateModal.razor`
- `CustomerManagementModal.razor`
- `UserCreateModal.razor`
- `UserManagementModal.razor`
- `ConfirmDeleteModal.razor`
- `Routes.razor`

Exemplo:

```razor
<MudAlert Severity="Severity.Error" Variant="Variant.Outlined">
    Erro ao salvar.
</MudAlert>
```

### MudTextField

Usado em formularios com `EditForm`.

Arquivos:

- `CustomerForm.razor`
- `CustomerCreateModal.razor`
- `CustomerManagementModal.razor`
- `UserCreateModal.razor`
- `UserManagementModal.razor`
- `AccountEmailForm.razor`

Exemplo:

```razor
<MudTextField @bind-Value="Model.Email"
              Label="Email"
              Variant="Variant.Outlined"
              For="() => Model.Email" />
```

Pontos importantes:

- `@bind-Value`: liga o campo ao model.
- `Label`: texto exibido no input.
- `Variant.Outlined`: visual com borda.
- `For`: conecta o campo a validacao do Blazor.

### MudSelect E MudSelectItem

Usados para escolher genero e role.

Arquivos:

- `CustomerForm.razor`
- `CustomerCreateModal.razor`
- `UserCreateModal.razor`
- `UserManagementModal.razor`

Exemplo:

```razor
<MudSelect @bind-Value="Model.Gender" Label="Genero">
    <MudSelectItem Value="@("Male")">Masculino</MudSelectItem>
    <MudSelectItem Value="@("Female")">Feminino</MudSelectItem>
</MudSelect>
```

### MudDatePicker

Usado para datas.

Arquivos:

- `CustomerForm.razor`
- `CustomerCreateModal.razor`
- `UserCreateModal.razor`
- `UserManagementModal.razor`

Exemplo:

```razor
<MudDatePicker @bind-Date="Model.DateOfBirth"
               Label="Nascimento"
               Variant="Variant.Outlined" />
```

Ele substitui o `InputDate` visualmente, mas continua ligado ao model.

### MudSnackbarProvider E ISnackbar

Usados para notificacoes.

Arquivos:

```text
Components/App.razor
Components/Shared/ToastHost.razor
```

O projeto ja tinha `ToastService`. Ele foi mantido. O que mudou foi a exibicao visual.

Antes:

```razor
<div class="alert alert-success">Mensagem</div>
```

Agora:

```csharp
Snackbar.Add(message, Severity.Success);
```

Isso permite que o resto da aplicacao continue chamando:

```csharp
Toast.Success("Cliente cadastrado com sucesso.");
Toast.Error("Erro ao salvar.");
```

Sem precisar trocar as pages.

## Como Cada Tela Usa MudBlazor

## Layout Principal

Arquivo:

```text
Components/Layout/MainLayout.razor
```

Componentes usados:

- `MudLayout`
- `MudDrawer`
- `MudMainContent`
- `MudAppBar`
- `MudContainer`
- `MudStack`
- `MudAvatar`
- `MudText`

Responsabilidade:

Define a casca da aplicacao: menu lateral, barra superior e area de conteudo.

## Menu Lateral

Arquivo:

```text
Components/Layout/NavMenu.razor
```

Componentes usados:

- `MudNavMenu`
- `MudNavLink`
- `MudSpacer`
- `MudPaper`
- `MudText`
- `MudButton`

Responsabilidade:

Mostra as rotas principais:

- Dashboard
- Clientes
- Usuarios
- Meu perfil

Tambem exibe usuario logado e botao de sair.

## Dashboard

Arquivo:

```text
Components/Pages/Home.razor
```

Componentes usados:

- `MudPaper`
- `MudStack`
- `MudText`
- `MudButton`
- `MudGrid`
- `MudItem`

Responsabilidade:

Mostra a tela inicial com card principal e cards de metricas.

## Login

Arquivo:

```text
Components/Pages/Login.razor
```

Componentes usados:

- `MudPaper`
- `MudStack`
- `MudAvatar`
- `MudText`
- `MudAlert`
- `MudButton`

Observacao importante:

Os campos de email e senha continuam sendo `<input>` HTML normal porque o form faz `POST` direto para:

```text
/auth/login
```

Isso preserva o fluxo atual de autenticacao.

## Clientes

Arquivo:

```text
Components/Pages/Customers/Customers.razor
```

Componentes usados diretamente:

- `ManagementPageHeader`
- `SkeletonCard`
- `EmptyState`
- `MudButton`
- `CustomerTable`
- `CustomerCardList`
- `CustomerManagementModal`
- `CustomerCreateModal`
- `ConfirmDeleteModal`

Responsabilidade:

Orquestra estado, carrega clientes, abre modais e chama `CustomerApi`.

Ela nao deve concentrar HTML gigante. A visualizacao fica nos componentes filhos.

## CustomerTable

Arquivo:

```text
Components/Pages/Customers/Components/CustomerTable.razor
```

Componentes usados:

- `MudPaper`
- `MudTable`
- `MudTh`
- `MudTd`
- `MudText`
- `MudChip`
- `MudButton`

Responsabilidade:

Exibe clientes em tabela desktop.

## CustomerCardList

Arquivo:

```text
Components/Pages/Customers/Components/CustomerCardList.razor
```

Componentes usados:

- `MudPaper`
- `MudStack`
- `MudText`
- `MudChip`
- `MudDivider`
- `MudButton`

Responsabilidade:

Exibe clientes em cards para telas menores.

## CustomerForm

Arquivo:

```text
Components/Pages/Customers/Components/CustomerForm.razor
```

Componentes usados:

- `ManagementPageHeader`
- `MudPaper`
- `MudGrid`
- `MudItem`
- `MudTextField`
- `MudSelect`
- `MudSelectItem`
- `MudDatePicker`
- `MudButton`

Responsabilidade:

Formulario reutilizavel para criar/editar cliente por rota.

## CustomerCreateModal

Arquivo:

```text
Components/Pages/Customers/Components/CustomerCreateModal.razor
```

Componentes usados:

- `MudPaper`
- `MudText`
- `MudIconButton`
- `MudAlert`
- `MudGrid`
- `MudItem`
- `MudTextField`
- `MudSelect`
- `MudSelectItem`
- `MudDatePicker`
- `MudStack`
- `MudButton`

Responsabilidade:

Modal para cadastrar cliente sem sair da listagem.

## CustomerManagementModal

Arquivo:

```text
Components/Pages/Customers/Components/CustomerManagementModal.razor
```

Componentes usados:

- `MudPaper`
- `MudText`
- `MudIconButton`
- `MudTabs`
- `MudTabPanel`
- `MudTextField`
- `MudAlert`
- `MudStack`
- `MudButton`

Responsabilidade:

Gerencia detalhes, atualizacao de email e exclusao de cliente.

## Usuarios

Arquivo:

```text
Components/Pages/Users/Users.razor
```

Componentes usados diretamente:

- `ManagementPageHeader`
- `SkeletonCard`
- `EmptyState`
- `UserTable`
- `UserCardList`
- `UserManagementModal`
- `UserCreateModal`
- `ConfirmDeleteModal`

Responsabilidade:

Orquestra carregamento, criacao, edicao, role, delete e permissoes.

## UserTable

Arquivo:

```text
Components/Pages/Users/Components/UserTable.razor
```

Componentes usados:

- `MudPaper`
- `MudTable`
- `MudTh`
- `MudTd`
- `MudText`
- `MudChip`
- `MudButton`

Responsabilidade:

Exibe usuarios em tabela desktop.

## UserCardList

Arquivo:

```text
Components/Pages/Users/Components/UserCardList.razor
```

Componentes usados:

- `MudPaper`
- `MudStack`
- `MudText`
- `MudChip`
- `MudDivider`
- `MudButton`

Responsabilidade:

Exibe usuarios em cards para telas menores.

## UserCreateModal

Arquivo:

```text
Components/Pages/Users/Components/UserCreateModal.razor
```

Componentes usados:

- `MudPaper`
- `MudText`
- `MudIconButton`
- `MudAlert`
- `MudGrid`
- `MudItem`
- `MudTextField`
- `MudSelect`
- `MudSelectItem`
- `MudDatePicker`
- `MudButton`

Responsabilidade:

Cria usuario novo.

## UserManagementModal

Arquivo:

```text
Components/Pages/Users/Components/UserManagementModal.razor
```

Componentes usados:

- `MudPaper`
- `MudText`
- `MudIconButton`
- `MudTabs`
- `MudTabPanel`
- `MudGrid`
- `MudItem`
- `MudTextField`
- `MudSelect`
- `MudSelectItem`
- `MudDatePicker`
- `MudAlert`
- `MudButton`

Responsabilidade:

Gerencia perfil, role e exclusao de usuario.

## UserDetails

Arquivo:

```text
Components/Pages/Users/UserDetails.razor
```

Componentes usados:

- `MudStack`
- `MudText`
- `MudButton`
- `MudPaper`
- `MudGrid`
- `MudItem`
- `MudChip`
- `ConfirmDeleteModal`

Responsabilidade:

Mostra detalhes de um usuario especifico.

## Meu Perfil

Arquivo:

```text
Components/Pages/Account/AccountProfile.razor
```

Componentes usados diretamente:

- `ManagementPageHeader`
- `SkeletonCard`
- `AccountIdentityCard`
- `AccountEmailForm`
- `AccountPasswordForm`

Responsabilidade:

Orquestra dados do usuario autenticado, troca de email e troca de senha.

## AccountIdentityCard

Arquivo:

```text
Components/Pages/Account/Components/AccountIdentityCard.razor
```

Componentes usados:

- `MudPaper`
- `MudStack`
- `MudText`

Responsabilidade:

Mostra dados principais da identidade do usuario logado.

## AccountEmailForm

Arquivo:

```text
Components/Pages/Account/Components/AccountEmailForm.razor
```

Componentes usados:

- `MudPaper`
- `MudStack`
- `MudText`
- `MudAlert`
- `MudTextField`
- `MudButton`

Responsabilidade:

Formulario Blazor para trocar email.

## AccountPasswordForm

Arquivo:

```text
Components/Pages/Account/Components/AccountPasswordForm.razor
```

Componentes usados:

- `MudPaper`
- `MudStack`
- `MudText`
- `MudAlert`
- `MudGrid`
- `MudItem`
- `MudButton`

Observacao importante:

Os campos continuam sendo `<input>` HTML porque o formulario faz `POST` direto para:

```text
/account/changepassword
```

Isso preserva o fluxo atual da aplicacao.

## Componentes Compartilhados

## ManagementPageHeader

Arquivo:

```text
Components/Shared/ManagementPageHeader.razor
```

Componentes usados:

- `MudStack`
- `MudText`
- `MudButton`

Responsabilidade:

Padroniza cabecalho de paginas de gestao.

Recebe:

- `Eyebrow`
- `Title`
- `Description`
- `ButtonText`
- `OnButtonClick`

## EmptyState

Arquivo:

```text
Components/Shared/EmptyState.razor
```

Componentes usados:

- `MudPaper`
- `MudStack`
- `MudIcon`
- `MudText`

Responsabilidade:

Mostra estado vazio quando nao ha registros.

## SkeletonCard

Arquivo:

```text
Components/Shared/SkeletonCard.razor
```

Componentes usados:

- `MudPaper`
- `MudSkeleton`

Responsabilidade:

Mostra carregamento visual.

## ConfirmDeleteModal

Arquivo:

```text
Components/Shared/ConfirmDeleteModal.razor
```

Componentes usados:

- `MudPaper`
- `MudText`
- `MudIconButton`
- `MudAlert`
- `MudStack`
- `MudButton`

Responsabilidade:

Modal reutilizavel para confirmar exclusoes.

## ToastHost

Arquivo:

```text
Components/Shared/ToastHost.razor
```

Componentes/servicos usados:

- `ISnackbar`
- `Severity`

Responsabilidade:

Traduz o `ToastService` antigo para notificacoes do MudBlazor.

## Quando Usar Cada Componente

Use `MudPaper` quando precisar de:

- card;
- container;
- painel;
- modal customizado;
- area destacada.

Use `MudStack` quando precisar:

- alinhar elementos em coluna;
- alinhar elementos em linha;
- controlar espacamento sem CSS demais.

Use `MudGrid` e `MudItem` quando precisar:

- formulario responsivo;
- duas ou tres colunas;
- layout que muda entre desktop e celular.

Use `MudTable` quando precisar:

- listar dados tabulares;
- ter cabecalho e linhas;
- exibir colecao de objetos.

Use `MudChip` quando precisar:

- status;
- role;
- categoria;
- badge visual.

Use `MudAlert` quando precisar:

- erro;
- aviso;
- confirmacao perigosa;
- mensagem destacada dentro da pagina.

Use `MudTabs` quando precisar:

- separar conteudo dentro do mesmo modal;
- evitar modais gigantes;
- separar "detalhes", "editar", "excluir".

Use `MudTextField`, `MudSelect` e `MudDatePicker` dentro de `EditForm` quando quiser:

- manter validacao do Blazor;
- melhorar o visual dos inputs;
- usar `For` para conectar validacao.

## Padrao Que Este Projeto Segue

O padrao mais importante e:

```text
Page = estado + chamadas + orquestracao
Component = visual + eventos
ApiClient = HTTP
Model = dados da tela
```

Exemplo:

`Customers.razor`:

- carrega clientes;
- abre modal;
- chama `CustomerApi`;
- mostra sucesso/erro.

`CustomerTable.razor`:

- recebe lista;
- renderiza tabela;
- dispara eventos `OnManage` e `OnDelete`.

Esse padrao evita page gigante.

## Exemplo Mental Para Aprender MudBlazor

Antes, com Bootstrap:

```razor
<button class="btn btn-primary">Salvar</button>
```

Com MudBlazor:

```razor
<MudButton Variant="Variant.Filled" Color="Color.Primary">
    Salvar
</MudButton>
```

Antes:

```razor
<div class="card">...</div>
```

Com MudBlazor:

```razor
<MudPaper Class="work-card" Elevation="0">
    ...
</MudPaper>
```

Antes:

```razor
<table class="table">...</table>
```

Com MudBlazor:

```razor
<MudTable Items="items">
    ...
</MudTable>
```

## Observacoes Importantes

- MudBlazor nao substitui Blazor.
- `@code`, `@inject`, `@bind`, `EventCallback` e `EditForm` continuam iguais.
- MudBlazor substitui principalmente a camada visual.
- Forms que fazem POST direto podem continuar usando `<form>` e `<input>` HTML.
- Nem tudo precisa virar componente MudBlazor se isso quebrar um fluxo existente.
- O projeto manteve componentes pequenos para facilitar manutencao.

## Build

Para validar a UI:

```bash
dotnet build src/CustomerApi.BlazorUI/CustomerApi.BlazorUI.csproj
```

Ou, se os pacotes ja estiverem restaurados:

```bash
dotnet build src/CustomerApi.BlazorUI/CustomerApi.BlazorUI.csproj --no-restore
```

