# Devplus.Security 🔐

[![NuGet](https://img.shields.io/nuget/v/Devplus.Security.svg)](https://www.nuget.org/packages/Devplus.Security/)
[![Downloads](https://img.shields.io/nuget/dt/Devplus.Security.svg)](https://www.nuget.org/packages/Devplus.Security/)

**Devplus.Security** é uma biblioteca .NET que fornece funcionalidades de autenticação e autorização OAuth com endpoints pré-configurados e integração JWT, eliminando a necessidade de criar repetitivamente controllers de autenticação em suas aplicações.

## ✨ **Características**

- 🔐 **OAuth Service integrado com Refit**
- 🎛️ **Controller de segurança pré-configurado**
- 🔑 **Autenticação JWT Bearer (Legacy)**
- ⚙️ **Configuração via appsettings.json**
- 🚀 **Integração automática com DI**
- 📦 **HttpClient com Polly para retry automático**
- 🔒 **Endpoints de login, logout, refresh token e recuperação de senha**

## 📦 **Instalação**

```bash
dotnet add package Devplus.Security
```

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

| Método | Endpoint                                     | Descrição                      |
| ------ | -------------------------------------------- | ------------------------------ |
| `POST` | `/api/v1/security/login`                     | Login com usuário e senha      |
| `POST` | `/api/v1/security/refresh-token`             | Renovar token de acesso        |
| `POST` | `/api/v1/security/password-recovery-request` | Solicitar recuperação de senha |
| `POST` | `/api/v1/security/password-reset`            | Redefinir senha com token      |
| `POST` | `/api/v1/security/logout`                    | Logout (requer autenticação)   |

### Exemplos de Uso dos Endpoints

#### Login

```json
POST /api/v1/security/login
{
  "nomeUsuario": "usuario@exemplo.com",
  "senha": "minhasenha123"
}

// Resposta
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "refreshToken": "def502004b8c4...",
  "scope": "read write"
}
```

#### Refresh Token

```json
POST /api/v1/security/refresh-token
{
  "refreshToken": "def502004b8c4..."
}
```

#### Recuperação de Senha

```json
POST /api/v1/security/password-recovery-request
{
  "email": "usuario@exemplo.com"
}
```

#### Reset de Senha

```json
POST /api/v1/security/password-reset
{
  "token": "reset-token-recebido-por-email",
  "newPassword": "novaSenha123"
}
```

## 🔧 **Serviços Injetáveis**

### IOAuthService

```csharp
public class MinhaApiService
{
    private readonly IOAuthService _oauthService;

    public MinhaApiService(IOAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    public async Task<Token> FazerLogin()
    {
        var tokenRequest = new TokenRequestDto
        {
            // configurar dados do request
        };

        return await _oauthService.GetTokenAsync(tokenRequest);
    }
}
```

### ISecurityService

```csharp
public class MinhaLogicaService
{
    private readonly ISecurityService _securityService;

    public MinhaLogicaService(ISecurityService securityService)
    {
        _securityService = securityService;
    }

    public async Task<Token> AutenticarUsuario(string email, string senha)
    {
        var loginDto = new UserLoginDto
        {
            NomeUsuario = email,
            Senha = senha
        };

        return await _securityService.AuthorizeUser(loginDto);
    }
}
```

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

### Token

```csharp
public class Token
{
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; }
    public string Scope { get; set; }
}
```

### UserLoginDto

```csharp
public class UserLoginDto
{
    public string? NomeUsuario { get; set; }
    public string? Senha { get; set; }
}
```

## ⚡ **Funcionalidades Técnicas**

### HttpClient com Retry (Polly)

A biblioteca configura automaticamente retry policies para chamadas HTTP:

- 6 tentativas com backoff exponencial
- Tratamento automático de erros transitórios
- Timeout e circuit breaker

### OAuthAccessTokenHandler

Handler automático que adiciona tokens de acesso client_credentials nas requisições para serviços externos.

### Autenticação JWT Legacy

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

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// MeuController.cs
[ApiController]
[Route("api/[controller]")]
public class MeuController : ControllerBase
{
    private readonly ISecurityService _securityService;

    public MeuController(ISecurityService securityService)
    {
        _securityService = securityService;
    }

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
            return Unauthorized();
        }
    }
}
```

## 📄 **Licença**

Este projeto está licenciado sob a [MIT License](../LICENSE).

## 📞 **Suporte**

Para dúvidas, sugestões ou problemas:

📧 Email: [clayton@devplus.com.br](mailto:clayton@devplus.com.br)  
🔗 LinkedIn: [Clayton Oliveira](https://www.linkedin.com/in/clayton-oliveira-7929b121/)  
🚀 Website: [www.devplus.com.br](https://www.devplus.com.br)
