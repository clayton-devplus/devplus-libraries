# Devplus.Messaging 📬

[![NuGet](https://img.shields.io/nuget/v/Devplus.Messaging.svg)](https://www.nuget.org/packages/Devplus.Messaging/)
[![Downloads](https://img.shields.io/nuget/dt/Devplus.Messaging.svg)](https://www.nuget.org/packages/Devplus.Messaging/)
[![License](https://img.shields.io/github/license/DevplusConsultoria/Devplus.Messaging)](LICENSE)

📬 Biblioteca para integração com RabbitMQ e mensageria na arquitetura limpa.

---

## 🚀 Instalação

Para instalar a biblioteca via **NuGet**, utilize o seguinte comando:

```sh
dotnet add package Devplus.Messaging
```

Ou, no **Visual Studio**:

1. Abra o **Gerenciador de Pacotes NuGet**.
2. Busque por **Devplus.Messaging**.
3. Clique em **Instalar**.

---

## ⚡ Como Usar

### 📦 **Publicando Mensagens**
```csharp
using Devplus.Messaging;

var publisher = new RabbitMqPublisher("localhost");
var message = new { Text = "Olá, RabbitMQ!" };
publisher.Publish(message, "my_exchange", "my_routing_key");
```

### 📩 **Consumindo Mensagens**
```csharp
using Devplus.Messaging;

var consumer = new RabbitMqConsumer("localhost");
consumer.Subscribe("my_queue", (message) => 
{
    Console.WriteLine($"Recebido: {message}");
});
```

---

## 🔧 **Configuração via `appsettings.json`**
```json
{
  "RabbitMq": {
    "HostName": "localhost",
    "Exchange": "my_exchange",
    "Queue": "my_queue",
    "RoutingKey": "my_routing_key"
  }
}
```

E no `Program.cs`:

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddRabbitMqMessaging(configuration);
```

---

## 🛠 **Compatibilidade**
| .NET Version | Suportado |
|--------------|----------|
| .NET 8.0     | ✅ Sim |
| .NET 7.0     | ✅ Sim |
| .NET 6.0     | ✅ Sim |
| .NET Core 3.1 | ⚠️ Suporte limitado |
| .NET Framework | ❌ Não suportado |

---

## 📌 **Funcionalidades**
✅ Suporte ao **RabbitMQ** com **publicação e consumo de mensagens**.  
✅ **Injeção de dependência** via `IServiceCollection`.  
✅ **Configuração via `appsettings.json`**.  
✅ Compatível com **.NET 6, .NET 7 e .NET 8**.  

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
