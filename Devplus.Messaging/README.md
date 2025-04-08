# Devplus.Messaging 📬

[![NuGet](https://img.shields.io/nuget/v/Devplus.Messaging.svg)](https://www.nuget.org/packages/Devplus.Messaging/)
[![Downloads](https://img.shields.io/nuget/dt/Devplus.Messaging.svg)](https://www.nuget.org/packages/Devplus.Messaging/)

📬 Biblioteca para integração com RabbitMQ e mensageria.

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
using Devplus.Messaging.Interfaces;

    private readonly IMessagingPublisher _messagingPublisher;
    public TestMessageService(IMessagingPublisher messagingProducer)
    {
        _messagingPublisher = messagingProducer;
    }
    public async Task SendMessage()
    {
        await _messagingPublisher.PublishAsync(exchangeName: "devplus-test-exchange",
                                                message: "{responseData: \"Test message from TestMessageService\"}",
                                                source: "devplus.test.app",
                                                typeEvent: "test-event");
    }
```

### 📩 **Consumindo Mensagens**

```csharp
using Devplus.Messaging.Interfaces;
using Devplus.Messaging.Models;

namespace Devplus.TestApp.Consumers;
public class TestConsumer : IMessagingConsumer
{
    public string ExchangeName => "devplus-test-exchange";

    //Opcionais, caso não informados a lib atribuirá valores padrões
    public string QueueName => "devplus-test-queue";
    public string RoutingKey => "";
    public int MaxRetry => 5;
    public ushort PrefetchCount => 3;
    QueueType QueueType => QueueType.Quorum;

    private readonly ILogger<TestConsumer> _logger;

    public TestConsumer(ILogger<TestConsumer> logger)
    {
        _logger = logger;
    }

    public Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received message: {Message}", cloudEvent.Data);
        return Task.CompletedTask;
    }
}
```

| Propriedade       | Descrição                                                                                                  | Obrig. | Padrão                                                                    |
| ----------------- | ---------------------------------------------------------------------------------------------------------- | ------ | ------------------------------------------------------------------------- |
| **ExchangeName**  | Nome da exchange. É obrigatórios para a lib realizar o bind no RabbitMq.                                   | Sim    | N/A                                                                       |
| **QueueName**     | Configura o nome da fila para o criar o bind no Rabbit                                                     | Não    | $"{ExchangeName.Replace("-exchange", "").Replace(".exchange", "")}-queue" |
| **RoutingKey**    | Configura uma chave de roteamento para realizar o bind na fila.                                            | Não    | ""                                                                        |
| **MaxRetry**      | Configura o numero máximo de tentativas que a menssagem é processada até ser enviada para DLQ              | Não    | 5                                                                         |
| **PrefetchCount** | Configura o numero máximo de menssagens simultâneas que um consumidor pode consumir antes de enviar um ACK | Não    | 3                                                                         |
| **QueueType** | Configura o tipo de fila no RabbitMQ (Quorum, Classic) | Não    | Quorum                                                                         |

---

## 🗂 **DLQ (Dead Letter Queue)**

A **Dead Letter Queue (DLQ)** é utilizada para armazenar mensagens que não puderam ser processadas com sucesso após o número máximo de tentativas configurado.

### Configuração de DLQ

O suporte a DLQ, está habilitado por padrão na lib, e usará o seguinte padrão para criar a Exchange e fila:

```csharp
    var dlxExchange = $"{exchangeName}-dlx";
    var dlqQueue = $"{queueName}-dlq";
```

### Observação

Caso a menssagem sejá processada até o limite de tentativas configurado em **MaxRetry**, elá será enviada para a DLQ criada pela lib adicionando o seguintes **Headers**:

```csharp
    ["x-retry-count"]   //Total de tentativas
    ["x-last-process"]  //Data hora do útimo processamento
    ["x-send-dlq"]      //Data hora de envio para DLQ
```

## 🔧 **Configuração via `appsettings.json`**

```json
{
  "RabbitMq": {
    "Host": "localhost",
    "Port": "5672",
    "Username": "user",
    "Password": "pass",
    "VHost": "my_vhost"
  }
}
```

E no `Program.cs`:

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddScoped<ITestMessageService, TestMessageService>();
builder.Services.AddScoped<IMessagingConsumer, TestConsumer>();
builder.Services.AddScoped<TestConsumer>();

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
