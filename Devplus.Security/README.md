# Devplus.Security 🔐

[![NuGet](https://img.shields.io/nuget/v/Devplus.Security.svg)](https://www.nuget.org/packages/Devplus.Security/)
[![Downloads](https://img.shields.io/nuget/dt/Devplus.Security.svg)](https://www.nuget.org/packages/Devplus.Security/)

**Devplus.Security** é uma biblioteca .NET que fornece funcionalidades completas de autenticação e autorização OAuth com endpoints pré-configurados, integração JWT e gerenciamento de usuários, eliminando a necessidade de criar repetitivamente controllers de autenticação em suas aplicações.

## ✨ **Características**

- 🔐 **OAuth Service integrado com Refit**
- 🎛️ **Controller de segurança pré-configurado com 9 endpoints**
- 🔑 **Autenticação JWT Bearer (Legacy)**
- 👥 **Gerenciamento de usuários da aplicação cliente**
- 🏢 **Suporte multi-tenant com informações de contexto**
- ⚙️ **Configuração via appsettings.json**
- 🚀 **Integração automática com DI**
- 📦 **HttpClient com Polly para retry automático**
- 🔒 **Endpoints completos: login, logout, refresh, recuperação, reset, exchange code, CRUD usuários**

## 📦 **Instalação**

```bash
dotnet add package Devplus.Security
```

### 🎯 Compatibilidade

A partir da versão **1.9.0** o pacote é **multi-target** e suporta:

| Target Framework | Suportado |
| ---------------- | --------- |
| .NET 8 (`net8.0`)  | ✅        |
| .NET 10 (`net10.0`) | ✅        |

> As dependências do ASP.NET Core são resolvidas via `FrameworkReference Microsoft.AspNetCore.App`, garantindo a versão correta do runtime em cada target automaticamente.

## ⚙️ **Configuração**

### 1. Configurar appsettings.json

```json
{
  "OAuthSettings": {
    "Url": "https://seu-oauth-server.com",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "IdentityClientId": "your-identity-client-id-guid",
    "IdentityClientSecret": "your-identity-client-secret",
    "PasswordRecoveryRedirectUrl": "https://sua-app.com/reset-password"
  },
  "Jwt": {
    "Issuer": "https://sua-app.com",
    "Audience": "sua-audience",
    "Key": "sua-chave-secreta-jwt-muito-longa-e-segura"
  }
}
```

### 2. Configurar no Program.cs

```csharp
using Devplus.Security.AspNetCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Adicionar Devplus Security (registra tudo automaticamente)
builder.Services.AddDevplusSecurity(builder.Configuration);

var app = builder.Build();

// Configurar pipeline de autenticação
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

## 🚀 **Endpoints Gerados Automaticamente**

A biblioteca gera automaticamente o controller `DevplusSecurityController` com os seguintes endpoints:

| Método | Endpoint                                     | Autenticação | Descrição                              |
| ------ | -------------------------------------------- | ------------ | -------------------------------------- |
| `POST` | `/api/v1/security/login`                     | ❌ Não       | Login com usuário e senha              |
| `POST` | `/api/v1/security/refresh-token`             | ❌ Não       | Renovar token de acesso                |
| `POST` | `/api/v1/security/password-recovery-request` | ❌ Não       | Solicitar recuperação de senha         |
| `POST` | `/api/v1/security/password-reset`            | ❌ Não       | Redefinir senha com token              |
| `POST` | `/api/v1/security/exchange-code`             | ❌ Não       | Trocar código de autorização por token |
| `POST` | `/api/v1/security/logout`                    | ✅ Sim       | Logout do usuário                      |
| `GET`  | `/api/v1/security/get-tenant-info`           | ✅ Sim       | Obter informações do tenant atual      |
| `POST` | `/api/v1/security/create-client-app-user`    | ✅ Sim       | Criar usuário da aplicação cliente     |
| `POST` | `/api/v1/security/remove-client-app-user`    | ✅ Sim       | Remover usuário da aplicação cliente   |

### Exemplos de Uso dos Endpoints

#### 🔐 Login

```json
POST /api/v1/security/login
Content-Type: application/json

{
  "nomeUsuario": "usuario@exemplo.com",
  "senha": "minhasenha123"
}

// Resposta (200 OK)
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "refreshToken": "def502004b8c4...",
  "scope": "read write"
}

// Resposta de Erro (401 Unauthorized)
{
  "error": "invalid_credentials"
}
```

#### 🔄 Refresh Token

```json
POST /api/v1/security/refresh-token
Content-Type: application/json

{
  "refreshToken": "def502004b8c4..."
}

// Resposta (200 OK)
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "refreshToken": "new_refresh_token...",
  "scope": "read write"
}
```

#### 📧 Recuperação de Senha

```json
POST /api/v1/security/password-recovery-request
Content-Type: application/json

{
  "email": "usuario@exemplo.com"
}

// Resposta (201 Created)
// Email de recuperação será enviado
```

#### 🔑 Reset de Senha

```json
POST /api/v1/security/password-reset
Content-Type: application/json

{
  "token": "reset-token-recebido-por-email",
  "newPassword": "novaSenha123"
}

// Resposta (204 No Content)
// Senha alterada com sucesso
```

#### 🔄 Exchange Code (OAuth Authorization Code Flow)

```json
POST /api/v1/security/exchange-code
Content-Type: application/json

{
  "code": "authorization_code_received",
  "state": "state_parameter"
}

// Resposta (200 OK)
// Header: Bearer: {access_token}
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "refreshToken": "def502004b8c4...",
  "scope": "read write"
}
```

#### 🚪 Logout

```json
POST /api/v1/security/logout
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "refreshToken": "def502004b8c4..."
}

// Resposta (204 No Content)
// Logout realizado com sucesso
```

#### 🏢 Obter Informações do Tenant

```json
GET /api/v1/security/get-tenant-info
Authorization: Bearer {access_token}

// Resposta (200 OK)
"123e4567-e89b-12d3-a456-426614174000"
```

#### 👤 Criar Usuário da Aplicação Cliente

```json
POST /api/v1/security/create-client-app-user
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "email": "novousuario@exemplo.com",
  "nomeCompleto": "Nome Completo do Usuário"
}

// Resposta (201 Created)
// Usuário criado com sucesso
```

#### 🗑️ Remover Usuário da Aplicação Cliente

```json
POST /api/v1/security/remove-client-app-user
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "email": "usuario@exemplo.com"
}

// Resposta (204 No Content)
// Usuário removido com sucesso
```

## 🔧 **Serviços Injetáveis**

### ISecurityService

O `ISecurityService` é o serviço principal que expõe todas as funcionalidades de segurança:

```csharp
public class MinhaLogicaService
{
    private readonly ISecurityService _securityService;

    public MinhaLogicaService(ISecurityService securityService)
    {
        _securityService = securityService;
    }

    // 🔐 Autenticação
    public async Task<Token> AutenticarUsuario(string email, string senha)
    {
        var loginDto = new UserLoginDto
        {
            NomeUsuario = email,
            Senha = senha
        };
        return await _securityService.AuthorizeUser(loginDto);
    }

    // 🔄 Renovação de Token
    public async Task<Token> RenovarToken(string refreshToken)
    {
        return await _securityService.RefreshToken(refreshToken);
    }

    // 📧 Recuperação de Senha
    public async Task<Token> SolicitarRecuperacao(string email)
    {
        return await _securityService.RequestPasswordRecovery(email);
    }

    // 🔑 Reset de Senha
    public async Task<Token> RedefinirSenha(string token, string novaSenha)
    {
        return await _securityService.ResetPassword(token, novaSenha);
    }

    // 🚪 Logout
    public async Task FazerLogout(string refreshToken)
    {
        await _securityService.Logout(refreshToken);
    }

    // 🔄 Trocar Código OAuth
    public async Task<Token> TrocarCodigo(string code, string state)
    {
        return await _securityService.ExchangeCode(code, state);
    }

    // 👥 Gerenciamento de Usuários
    public async Task CriarUsuario(string email, string nomeCompleto)
    {
        var request = new CreateClientAppRequestDto
        {
            Email = email,
            NomeCompleto = nomeCompleto
        };
        await _securityService.CreateClientAppUser(request);
    }

    public async Task RemoverUsuario(string email)
    {
        var request = new RemoveClientAppRequestDto { Email = email };
        await _securityService.RemoveClientAppUser(request);
    }

    // 🏢 Informações do Contexto
    public Dictionary<string, string> ObterClaimsUsuario()
    {
        return _securityService.GetUserClaims();
    }

    public IEnumerable<string> ObterRolesUsuario()
    {
        return _securityService.GetUserRoles();
    }

    public Guid ObterTenantId()
    {
        return _securityService.GetTenantId();
    }

    public string ObterDominio()
    {
        return _securityService.GetDomain();
    }

    // ➕ Adicionar Claims Personalizados
    public void AdicionarClaim(string tipo, string valor)
    {
        _securityService.AddClaim(tipo, valor);
    }
}
```

### IOAuthService (Nível Baixo)

Para casos onde você precisa de controle mais granular sobre as chamadas OAuth:

```csharp
public class MinhaApiService
{
    private readonly IOAuthService _oauthService;

    public MinhaApiService(IOAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    public async Task<Token> FazerLoginPersonalizado()
    {
        var tokenRequest = new TokenRequestDto
        {
            GrantType = "password",
            Username = "usuario@exemplo.com",
            Password = "senha123",
            ClientId = Guid.Parse("client-id-guid"),
            ClientSecret = "client-secret",
            Scope = "read write"
        };

        return await _oauthService.GetTokenAsync(tokenRequest);
    }

    public async Task<Token> RenovarTokenPersonalizado(string refreshToken)
    {
        var refreshRequest = new RefreshTokenOAuthRequestDto
        {
            RefreshToken = refreshToken
        };

        return await _oauthService.RefreshToken(refreshRequest);
    }
}
```

### ⚡ Funcionalidades do ISecurityService

| Método                      | Descrição                         | Autenticação Necessária |
| --------------------------- | --------------------------------- | ----------------------- |
| `AuthorizeUser()`           | Autentica usuário com login/senha | ❌ Não                  |
| `RefreshToken()`            | Renova token de acesso            | ❌ Não                  |
| `ExchangeCode()`            | Troca código OAuth por token      | ❌ Não                  |
| `RequestPasswordRecovery()` | Solicita recuperação de senha     | ❌ Não                  |
| `ResetPassword()`           | Redefine senha com token          | ❌ Não                  |
| `Logout()`                  | Faz logout do usuário             | ✅ Sim                  |
| `CreateClientAppUser()`     | Cria novo usuário                 | ✅ Sim                  |
| `RemoveClientAppUser()`     | Remove usuário existente          | ✅ Sim                  |
| `GetUserClaims()`           | Obtém claims do usuário atual     | ✅ Sim                  |
| `GetUserRoles()`            | Obtém roles do usuário atual      | ✅ Sim                  |
| `GetTenantId()`             | Obtém ID do tenant atual          | ✅ Sim                  |
| `GetDomain()`               | Obtém domínio da aplicação        | ✅ Sim                  |
| `AddClaim()`                | Adiciona claim ao contexto        | ✅ Sim                  |

## � **Usando em Controllers Personalizados**

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Utiliza a autenticação JWT configurada
public class MeuController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        // Acesso aos dados do usuário autenticado
        var userId = User.FindFirst("sub")?.Value;
        var userName = User.Identity?.Name;

        return Ok(new { Message = "Acesso autorizado!", UserId = userId });
    }
}
```

## 📋 **DTOs e Models**

### 🔑 Token (Resposta de Autenticação)

```csharp
public class Token
{
    public string AccessToken { get; set; }      // JWT token de acesso
    public string TokenType { get; set; }        // "Bearer"
    public int ExpiresIn { get; set; }           // Tempo de expiração em segundos
    public string RefreshToken { get; set; }     // Token para renovação
    public string Scope { get; set; }            // Escopo de permissões
}
```

### 👤 UserLoginDto (Login de Usuário)

```csharp
public class UserLoginDto
{
    public string? NomeUsuario { get; set; }     // Email ou username
    public string? Senha { get; set; }           // Senha do usuário
}
```

### 🔄 RefreshTokenRequestDto (Renovação de Token)

```csharp
public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; }     // Token de renovação
}
```

### 📧 PasswordRecoveryDto (Recuperação de Senha)

```csharp
public class PasswordRecoveryDto
{
    public string Email { get; set; }            // Email para recuperação
}
```

### 🔑 PasswordResetDto (Reset de Senha)

```csharp
public class PasswordResetDto
{
    public string Token { get; set; }            // Token de reset recebido por email
    public string NewPassword { get; set; }      // Nova senha
}
```

### 🔄 ExchangeCodeRequestDto (Troca de Código OAuth)

```csharp
public class ExchangeCodeRequestDto
{
    public string Code { get; set; }             // Código de autorização OAuth
    public string State { get; set; }            // Parâmetro de estado OAuth
}
```

### 👥 CreateClientAppRequestDto (Criar Usuário)

```csharp
public class CreateClientAppRequestDto
{
    public string Email { get; set; }            // Email do novo usuário
    public string? NomeCompleto { get; set; }    // Nome completo (opcional)
}
```

### 🗑️ RemoveClientAppRequestDto (Remover Usuário)

```csharp
public class RemoveClientAppRequestDto
{
    public string Email { get; set; }            // Email do usuário a ser removido
}
```

### 🏢 UsuarioResponseDto (Resposta de Usuário)

```csharp
public class UsuarioResponseDto
{
    public long Id { get; set; }                 // ID único do usuário
    public string? Nome { get; set; }            // Nome do usuário
    public string? Login { get; set; }           // Login/username
    public string? Email { get; set; }           // Email do usuário
    public string? Domain { get; set; }          // Domínio da aplicação
    public bool Ativada { get; set; }            // Status de ativação
    public DateTime? Validade { get; set; }      // Data de validade da conta
}
```

### ⚙️ TokenRequestDto (Configuração Interna de Token)

```csharp
public class TokenRequestDto
{
    public string GrantType { get; set; }        // Tipo de concessão OAuth
    public string Username { get; set; }         // Nome de usuário
    public string Password { get; set; }         // Senha
    public Guid ClientId { get; set; }           // ID do cliente OAuth
    public string ClientSecret { get; set; }     // Segredo do cliente
    public Guid? TenantId { get; set; }          // ID do tenant (opcional)
    public string? Scope { get; set; }           // Escopo solicitado (opcional)
    public string? Code { get; set; }            // Código de autorização (opcional)
    public string? RedirectUri { get; set; }     // URI de redirecionamento (opcional)
    public string? CodeVerifier { get; set; }    // Verificador PKCE (opcional)
}
```

## ⚡ **Funcionalidades Técnicas**

### 📊 Códigos de Status HTTP dos Endpoints

| Endpoint          | Sucesso          | Erro Comum         | Não Autorizado     |
| ----------------- | ---------------- | ------------------ | ------------------ |
| Login             | `200 OK`         | `401 Unauthorized` | -                  |
| Refresh Token     | `200 OK`         | `400 Bad Request`  | -                  |
| Password Recovery | `201 Created`    | `400 Bad Request`  | -                  |
| Password Reset    | `204 No Content` | `400 Bad Request`  | -                  |
| Exchange Code     | `200 OK`         | `400 Bad Request`  | -                  |
| Logout            | `204 No Content` | `400 Bad Request`  | `401 Unauthorized` |
| Get Tenant Info   | `200 OK`         | -                  | `401 Unauthorized` |
| Create User       | `201 Created`    | `400 Bad Request`  | `401 Unauthorized` |
| Remove User       | `204 No Content` | `400 Bad Request`  | `401 Unauthorized` |

### 🔄 HttpClient com Retry (Polly)

A biblioteca configura automaticamente retry policies para chamadas HTTP:

- **6 tentativas** com backoff exponencial
- **Tratamento automático** de erros transitórios (5xx, timeout)
- **Circuit breaker** para evitar cascata de falhas
- **Timeout configurável** por requisição

```csharp
// Configuração automática do Polly
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(
        retryCount: 6,
        sleepDurationProvider: retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            // Log de retry automático
        });
```

### 🔐 OAuthAccessTokenHandler

Handler automático que adiciona tokens de acesso `client_credentials` nas requisições para serviços externos:

```csharp
// Configuração automática nos HttpClients registrados
services.AddHttpClient<MeuServico>()
    .AddHttpMessageHandler<OAuthAccessTokenHandler>(); // Adiciona token automaticamente
```

### 🏗️ Autenticação JWT Legacy

Suporte a configuração JWT simples com chaves diretas no appsettings:

```json
{
  "Jwt": {
    "Issuer": "https://meuapp.com",
    "Audience": "minha-api",
    "Key": "minha-chave-super-secreta-de-pelo-menos-256-bits"
  }
}
```

### 🛡️ Tratamento de Erros e Exceções

```csharp
// Exemplos de tratamento de exceções comuns
try
{
    var token = await _securityService.AuthorizeUser(loginDto);
    return Ok(token);
}
catch (UnauthorizedAccessException)
{
    return Unauthorized(new { error = "invalid_credentials" });
}
catch (HttpRequestException ex) when (ex.Message.Contains("timeout"))
{
    return StatusCode(503, new { error = "service_unavailable" });
}
catch (HttpRequestException ex) when (ex.Message.Contains("404"))
{
    return NotFound(new { error = "oauth_service_not_found" });
}
catch (Exception ex)
{
    // Log do erro
    return StatusCode(500, new { error = "internal_server_error" });
}
```

### 🔍 Validação de Claims e Contexto

```csharp
public class ValidacaoService
{
    private readonly ISecurityService _security;

    public ValidacaoService(ISecurityService security) => _security = security;

    public bool ValidarAcessoTenant(Guid tenantId)
    {
        try
        {
            var userTenantId = _security.GetTenantId();
            return userTenantId == tenantId;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    public bool ValidarDominio(string dominio)
    {
        try
        {
            var userDomain = _security.GetDomain();
            return userDomain.Equals(dominio, StringComparison.OrdinalIgnoreCase);
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    public bool TemRole(string role)
    {
        var userRoles = _security.GetUserRoles();
        return userRoles.Contains(role);
    }
}
```

## 🏗️ **Arquitetura**

```
Devplus.Security/
├── src/
│   ├── AspNetCore/
│   │   ├── Controllers/
│   │   │   └── DevplusSecurityController.cs
│   │   ├── DependencyInjection/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── LegacyJwtAuthenticationExtensions.cs
│   │   └── Services/
│   │       ├── ISecurityService.cs
│   │       └── SecurityService.cs
│   └── OAuth/
│       ├── Contracts/
│       │   ├── IOAuthService.cs
│       │   ├── Token.cs
│       │   ├── UserLoginDto.cs
│       │   └── ...
│       ├── Refit/
│       │   ├── IDevplusOAuthService.cs
│       │   └── IDevplusOAuthLogoutService.cs
│       ├── DevplusOAuthService.cs
│       ├── OAuthAccessTokenHandler.cs
│       ├── OAuthSettings.cs
│       └── DevplusOAuthServiceCollectionExtensions.cs
```

## 🧪 **Exemplo de Implementação Completa**

```csharp
// Program.cs
using Devplus.Security.AspNetCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDevplusSecurity(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// MeuController.cs - Exemplo com vários recursos
[ApiController]
[Route("api/[controller]")]
public class MeuController : ControllerBase
{
    private readonly ISecurityService _securityService;

    public MeuController(ISecurityService securityService)
    {
        _securityService = securityService;
    }

    // 🔐 Endpoint personalizado de login
    [HttpPost("custom-login")]
    public async Task<IActionResult> CustomLogin([FromBody] UserLoginDto dto)
    {
        try
        {
            var token = await _securityService.AuthorizeUser(dto);
            return Ok(token);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Credenciais inválidas" });
        }
    }

    // 👥 Gerenciar usuários (requer autenticação)
    [HttpPost("usuarios")]
    [Authorize]
    public async Task<IActionResult> CriarUsuario([FromBody] CreateClientAppRequestDto dto)
    {
        try
        {
            await _securityService.CreateClientAppUser(dto);
            return Created($"/usuarios/{dto.Email}", new { message = "Usuário criado com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("usuarios/{email}")]
    [Authorize]
    public async Task<IActionResult> RemoverUsuario(string email)
    {
        try
        {
            var request = new RemoveClientAppRequestDto { Email = email };
            await _securityService.RemoveClientAppUser(request);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 🏢 Informações do usuário autenticado
    [HttpGet("me")]
    [Authorize]
    public IActionResult ObterInformacoesUsuario()
    {
        try
        {
            var claims = _securityService.GetUserClaims();
            var roles = _securityService.GetUserRoles();
            var tenantId = _securityService.GetTenantId();
            var domain = _securityService.GetDomain();

            return Ok(new
            {
                TenantId = tenantId,
                Domain = domain,
                Claims = claims,
                Roles = roles,
                UserId = User.FindFirst("sub")?.Value,
                UserName = User.Identity?.Name
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // 🔒 Endpoint protegido com validação de role
    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    public IActionResult RecursoAdmin()
    {
        var userRoles = _securityService.GetUserRoles();
        return Ok(new
        {
            message = "Acesso autorizado para administradores!",
            userRoles = userRoles
        });
    }

    // ⚡ Endpoint com adição de claims personalizados
    [HttpPost("add-custom-claim")]
    [Authorize]
    public IActionResult AdicionarClaimPersonalizado([FromBody] AddClaimRequest request)
    {
        _securityService.AddClaim(request.Type, request.Value);

        return Ok(new
        {
            message = "Claim adicionado com sucesso",
            claims = _securityService.GetUserClaims()
        });
    }
}

// DTO para adicionar claims
public class AddClaimRequest
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
```

### appsettings.json Completo

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "OAuthSettings": {
    "Url": "https://seu-oauth-server.com",
    "ClientId": "123e4567-e89b-12d3-a456-426614174000",
    "ClientSecret": "seu-client-secret-super-seguro",
    "PasswordRecoveryRedirectUrl": "https://sua-app.com/reset-password"
  },
  "Jwt": {
    "Issuer": "https://sua-app.com",
    "Audience": "sua-api",
    "Key": "sua-chave-secreta-jwt-de-pelo-menos-256-bits-muito-longa-e-segura"
  },
  "AllowedHosts": "*"
}
```

## 📜 **Histórico de Versões**

### 1.9.0

- 🎯 **Multi-target `net8.0` + `net10.0`** — suporte explícito a .NET 8 e .NET 10.
- 🏗️ Dependências do ASP.NET Core migradas para `FrameworkReference Microsoft.AspNetCore.App` (resolução por runtime, sem pacotes pinados).
- 🔒 **Correção de segurança:** atualização do `Refit`/`Refit.HttpClientFactory` `7.1.2 → 7.2.22`, eliminando a [CVE-2024-51501](https://github.com/advisories/GHSA-3hxg-fxwm-8gf7) (CRLF injection, severidade crítica / CVSS 10.0).

### 1.8.0

- Versão anterior estável (target único `net8.0`).

## 📄 **Licença**

Este projeto está licenciado sob a [MIT License](../LICENSE).

## 📞 **Suporte**

Para dúvidas, sugestões ou problemas:

📧 Email: [clayton@devplus.com.br](mailto:clayton@devplus.com.br)  
🔗 LinkedIn: [Clayton Oliveira](https://www.linkedin.com/in/clayton-oliveira-7929b121/)  
🚀 Website: [www.devplus.com.br](https://www.devplus.com.br)
