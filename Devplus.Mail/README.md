# Devplus.Mail ✉️

[![NuGet](https://img.shields.io/nuget/v/Devplus.Mail.svg)](https://www.nuget.org/packages/Devplus.Mail/)
[![Downloads](https://img.shields.io/nuget/dt/Devplus.Mail.svg)](https://www.nuget.org/packages/Devplus.Mail/)

✉️ Biblioteca para envio de emails utilizando o SendGrid.

---

## 🚀 Instalação

Para instalar a biblioteca via **NuGet**, utilize o seguinte comando:

```sh
dotnet add package Devplus.Mail
```

Ou, no **Visual Studio**:

1. Abra o **Gerenciador de Pacotes NuGet**.
2. Busque por **Devplus.Mail**.
3. Clique em **Instalar**.

---

## ⚡ Como Usar

### 📤 **Enviando Emails**

```csharp
using Devplus.Mail.Interfaces;

public class EmailServiceExample
{
    private readonly IEmailService _emailService;

    public EmailServiceExample(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendEmail()
    {
        await _emailService.SendEmailAsync(
            toEmail: "example@domain.com",
            subject: "Bem-vindo ao Devplus!",
            plainTextContent: "Obrigado por se registrar.",
            htmlContent: "<strong>Obrigado por se registrar.</strong>"
        );
    }
}
```

### 📋 **Enviando Emails com Template**

```csharp
using Devplus.Mail.Enum;
using Devplus.Mail.Interfaces;

public class TemplateEmailExample
{
    private readonly IEmailService _emailService;

    public TemplateEmailExample(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendTemplateEmail()
    {
        await _emailService.SendTemplateEmail(
            toEmail: "example@domain.com",
            subject: "Aviso Importante",
            message: "Sua conta foi atualizada com sucesso.",
            link: "https://www.devplus.com.br",
            templateType: EmailTemplateType.Success
        );
    }
}
```

---

## 🔧 **Configuração via `appsettings.json`**

```json
{
  "SendGrid": {
    "ApiKey": "sua-api-key",
    "FromEmail": "noreply@devplus.com.br",
    "FromName": "Devplus"
  }
}
```

E no `Program.cs`:

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddMail(configuration);
```

---

## 🛠 **Compatibilidade**

| .NET Version   | Suportado           |
| -------------- | ------------------- |
| .NET 8.0       | ✅ Sim              |
| .NET 7.0       | ✅ Sim              |
| .NET 6.0       | ✅ Sim              |
| .NET Core 3.1  | ⚠️ Suporte limitado |
| .NET Framework | ❌ Não suportado    |

---

## 📌 **Funcionalidades**

✅ Suporte ao **SendGrid** para envio de emails.  
✅ **Injeção de dependência** via `IServiceCollection`.  
✅ **Configuração via `appsettings.json`**.  
✅ Compatível com **.NET 6, .NET 7 e .NET 8**.  
✅ Suporte a **templates de email** embutidos.

---

## 🏗 **Contribuindo**

Se você quiser contribuir para este projeto:

1. **Fork** o repositório.
2. Crie uma **branch** (`git checkout -b feature/nova-feature`).
3. Faça **commit** das alterações (`git commit -m "Add nova feature"`).
4. Envie um **Pull Request**.

---

## 📄 **Licença**

Este projeto está licenciado sob a [MIT License](LICENSE).

---

## 📞 **Contato**

📧 Email: [clayton@devplus.com.br](mailto:clayton@devplus.com.br)  
🔗 LinkedIn: [Clayton Oliveira](https://www.linkedin.com/in/clayton-oliveira-7929b121/)  
🚀 Devplus Consultoria: [www.devplus.com.br](https://www.devplus.com.br)
