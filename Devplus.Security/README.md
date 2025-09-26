# Devplus.Security 🔐

[![NuGet](https://img.shields.io/nuget/v/Devplus.Security.svg)](https://www.nuget.org/packages/Devplus.Security/)
[![Downloads](https://img.shields.io/nuget/dt/Devplus.Security.svg)](https://www.nuget.org/packages/Devplus.Security/)

**Devplus.Security** é uma biblioteca .NET que simplifica a implementação de autenticação e autorização OAuth em suas aplicações, gerando automaticamente controllers e endpoints necessários.

## ✨ **Características**

- 🔐 **Autenticação OAuth automática**
- 🎛️ **Geração automática de controllers**
- 🔑 **Validação de tokens JWT**
- ⚙️ **Configuração via appsettings.json**
- 🚀 **Integração simples com DI**
- 📦 **Middleware personalizável**

## 📦 **Instalação**

```bash
dotnet add package Devplus.Security
```

## ⚙️ **Configuração**

### 1. Configurar appsettings.json

```json
{
  "OAuthSettings": {
    "IdentityServerUrl": "https://seu-oauth-server.com",
    "IdentityClientId": "your-client-id-guid",
    "IdentityClientSecret": "your-client-secret",
    "RequireHttps": true,
    "TokenEndpoint": "/oauth/token",
    "ValidateToken": true,
    "AutoGenerateControllers": true
  }
}
```

### 2. Configurar no Program.cs

```csharp
using Devplus.Security.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Adicionar Devplus Security
builder.Services.AddDevplusSecurity(builder.Configuration);

var app = builder.Build();

// Usar Devplus Security Middleware
app.UseDevplusSecurity();

app.Run();
```

## 🚀 **Uso Básico**

### Controllers Gerados Automaticamente

A biblioteca gera automaticamente os seguintes endpoints:

```
POST /api/oauth/token          - Obter token de acesso
POST /api/oauth/refresh        - Renovar token
POST /api/oauth/validate       - Validar token
DELETE /api/oauth/revoke       - Revogar token
```

### Usando em Controllers Personalizados

```csharp
[ApiController]
[Route("api/[controller]")]
[OAuthAuthorize] // Atributo personalizado da biblioteca
public class WeatherController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        // Acesso aos claims do usuário autenticado
        var userId = HttpContext.GetUserId();
        var userRoles = HttpContext.GetUserRoles();

        return Ok(new { Message = "Acesso autorizado!", UserId = userId });
    }
}
```

### Configuração Avançada

```csharp
builder.Services.AddDevplusSecurity(builder.Configuration, options =>
{
    options.EnableSwaggerAuth = true;
    options.CustomScopes = new[] { "read", "write", "admin" };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ClockSkew = TimeSpan.Zero
    };
});
```

## 🔧 **Serviços Injetáveis**

```csharp
public class MyService
{
    private readonly IOAuthService _oauthService;
    private readonly ITokenValidator _tokenValidator;

    public MyService(IOAuthService oauthService, ITokenValidator tokenValidator)
    {
        _oauthService = oauthService;
        _tokenValidator = tokenValidator;
    }

    public async Task<string> GetTokenAsync()
    {
        var token = await _oauthService.GetAccessTokenAsync();
        return token.AccessToken;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        return await _tokenValidator.ValidateAsync(token);
    }
}
```

## 📋 **Exemplos de Uso**

### Autenticação Client Credentials

```csharp
var tokenRequest = new TokenRequestDto
{
    GrantType = "client_credentials",
    ClientId = Guid.Parse("your-client-id"),
    ClientSecret = "your-client-secret"
};

var token = await oauthService.GetTokenAsync(tokenRequest);
```

### Middleware Personalizado

```csharp
app.UseDevplusSecurity(options =>
{
    options.OnTokenValidated = context =>
    {
        // Lógica personalizada após validação do token
        var claims = context.Principal.Claims;
        // ...
    };

    options.OnAuthenticationFailed = context =>
    {
        // Lógica personalizada para falhas de autenticação
        context.Response.StatusCode = 401;
        // ...
    };
});
```

## 🧪 **Testes**

```csharp
// Exemplo de teste unitário
[Test]
public async Task Should_Generate_Token_Successfully()
{
    // Arrange
    var oauthService = serviceProvider.GetService<IOAuthService>();

    // Act
    var token = await oauthService.GetAccessTokenAsync();

    // Assert
    Assert.IsNotNull(token.AccessToken);
    Assert.IsTrue(token.ExpiresIn > 0);
}
```

## 📄 **Licença**

Este projeto está licenciado sob a [MIT License](../LICENSE).

## 📞 **Suporte**

Para dúvidas, sugestões ou problemas:

📧 Email: [clayton@devplus.com.br](mailto:clayton@devplus.com.br)  
🔗 LinkedIn: [Clayton Oliveira](https://www.linkedin.com/in/clayton-oliveira-7929b121/)  
🚀 Website: [www.devplus.com.br](https://www.devplus.com.br)
