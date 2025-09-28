# Devplus.Messaging 📬

[![NuGet](https://img.shields.io/nuget/v/Devplus.Messaging.svg)](https://www.nuget.org/packages/Devplus.Messaging/)
[![Downloads](https://img.shields.io/nuget/dt/Devplus.Messaging.svg)](https://www.nuget.org/packages/Devplus.Messaging/)

**Devplus.Messaging** é uma biblioteca .NET para integração avançada com RabbitMQ, oferecendo funcionalidades completas de publicação e consumo de mensagens baseadas no padrão **CloudEvents**, com suporte a Dead Letter Queue (DLQ), retry automático, múltiplos tipos de fila e configurações flexíveis.

## ✨ **Características**

- 📬 **Publicação e consumo de mensagens** baseado no padrão **CloudEvents**
- 🔄 **Sistema de retry automático** com configuração personalizável
- 💀 **Dead Letter Queue (DLQ)** automática para mensagens falhas
- 🏗️ **Suporte a filas Quorum e Classic** do RabbitMQ
- ⚙️ **Configuração flexível** via appsettings.json
- 🚀 **Hosted Service integrado** para gerenciamento automático de consumidores
- 📊 **QoS (Quality of Service)** configurável por consumidor
- 🔗 **Binding automático** de exchanges e filas
- 🛡️ **Tratamento robusto de erros** e reconexão automática
- 📝 **Logging detalhado** para monitoramento e debug

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

## ⚡ **Como Usar**

### 📦 **Publicando Mensagens**

O `IMessagingPublisher` permite publicar mensagens seguindo o padrão CloudEvents:

```csharp
using Devplus.Messaging.Interfaces;

public class ProdutoService
{
    private readonly IMessagingPublisher _messagingPublisher;

    public ProdutoService(IMessagingPublisher messagingPublisher)
    {
        _messagingPublisher = messagingPublisher;
    }

    public async Task CriarProduto(Produto produto)
    {
        // Salvar produto no banco...

        // Publicar evento
        await _messagingPublisher.PublishAsync(
            exchangeName: "produtos-exchange",
            message: new {
                ProdutoId = produto.Id,
                Nome = produto.Nome,
                Preco = produto.Preco
            },
            source: "produtos.api",
            typeEvent: "produto.criado",
            messageId: Guid.NewGuid().ToString(), // Opcional
            routingKey: "produtos.criados" // Opcional
        );
    }
}
```

#### Parâmetros do `PublishAsync`:

| Parâmetro      | Tipo     | Obrigatório | Descrição                                                      |
| -------------- | -------- | ----------- | -------------------------------------------------------------- |
| `exchangeName` | `string` | ✅ Sim      | Nome do exchange RabbitMQ                                      |
| `message`      | `T`      | ✅ Sim      | Objeto da mensagem a ser publicada                             |
| `source`       | `string` | ✅ Sim      | Origem da mensagem (ex: "api.produtos")                        |
| `typeEvent`    | `string` | ✅ Sim      | Tipo do evento (ex: "produto.criado")                          |
| `messageId`    | `string` | ❌ Não      | ID único da mensagem (gerado automaticamente se não informado) |
| `routingKey`   | `string` | ❌ Não      | Chave de roteamento (padrão: vazio)                            |

### 📩 **Consumindo Mensagens**

Para consumir mensagens, implemente a interface `IMessagingConsumer`:

```csharp
using Devplus.Messaging.Interfaces;
using Devplus.Messaging.Models;
using Devplus.Messaging.Enum;

public class ProdutoCriadoConsumer : IMessagingConsumer
{
    // ✅ Obrigatório
    public string ExchangeName => "produtos-exchange";

    // ⚙️ Configurações opcionais (valores padrão mostrados)
    public string QueueName => "produtos-criados-queue";
    public string RoutingKey => "produtos.criados";
    public int MaxRetry => 5;
    public ushort PrefetchCount => 3;
    public QueueType QueueType => QueueType.Quorum;

    private readonly ILogger<ProdutoCriadoConsumer> _logger;
    private readonly IEmailService _emailService;

    public ProdutoCriadoConsumer(
        ILogger<ProdutoCriadoConsumer> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando produto criado: {Data}", cloudEvent.Data);

        try
        {
            // Processar a mensagem
            var produto = JsonSerializer.Deserialize<ProdutoDto>(cloudEvent.Data.ToString());

            // Enviar email de notificação
            await _emailService.EnviarNotificacao($"Novo produto: {produto.Nome}");

            _logger.LogInformation("Produto {ProdutoId} processado com sucesso", produto.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar produto criado");
            throw; // Será reenviado automaticamente até MaxRetry
        }
    }
}
```

### ⚙️ **Propriedades do IMessagingConsumer**

| Propriedade       | Obrigatório | Padrão                                | Descrição                                            |
| ----------------- | ----------- | ------------------------------------- | ---------------------------------------------------- |
| **ExchangeName**  | ✅ **Sim**  | N/A                                   | Nome do exchange RabbitMQ para binding               |
| **QueueName**     | ❌ Não      | `"{ExchangeName-sem-exchange}-queue"` | Nome da fila personalizada                           |
| **RoutingKey**    | ❌ Não      | `""`                                  | Chave de roteamento para binding específico          |
| **MaxRetry**      | ❌ Não      | `5`                                   | Número máximo de tentativas antes de enviar para DLQ |
| **PrefetchCount** | ❌ Não      | `3`                                   | Máximo de mensagens simultâneas sem ACK (QoS)        |
| **QueueType**     | ❌ Não      | `QueueType.Quorum`                    | Tipo de fila: `Quorum` ou `Classic`                  |

#### 📋 **Exemplos de Nomes Automáticos**:

```csharp
// ExchangeName: "produtos-exchange" → QueueName: "produtos-queue"
// ExchangeName: "pedidos.exchange" → QueueName: "pedidos-queue"
// ExchangeName: "eventos-sistema" → QueueName: "eventos-sistema-queue"
```

### 🔄 **Sistema de Retry e Recuperação**

```csharp
public class ProcessadorPedidos : IMessagingConsumer
{
    public string ExchangeName => "pedidos-exchange";
    public int MaxRetry => 3; // Tentar até 3 vezes antes de enviar para DLQ

    public async Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken)
    {
        try
        {
            // Processar pedido...
            await ProcessarPedido(cloudEvent.Data);
        }
        catch (HttpRequestException ex)
        {
            // Erro temporário - será reprocessado automaticamente
            _logger.LogWarning("Falha temporária: {Error}", ex.Message);
            throw;
        }
        catch (ArgumentException ex)
        {
            // Erro permanente - registrar e não relançar (vai para ACK)
            _logger.LogError("Dados inválidos: {Error}", ex.Message);
            // Não faz throw - mensagem será confirmada (ACK)
        }
    }
}
```

## � **DLQ (Dead Letter Queue)**

A **Dead Letter Queue (DLQ)** é uma funcionalidade automática para lidar com mensagens que falharam após múltiplas tentativas.

### 🏗️ **Configuração Automática**

A biblioteca cria automaticamente:

```csharp
// Exchange principal: "produtos-exchange"
// Exchange DLX:      "produtos-exchange-dlx"
// Fila principal:    "produtos-queue"
// Fila DLQ:          "produtos-queue-dlq"
```

### 📊 **Headers Automáticos na DLQ**

Quando uma mensagem é enviada para DLQ, os seguintes headers são adicionados:

```csharp
{
    "x-retry-count": "5",                           // Total de tentativas
    "x-last-process": "2025-09-28T10:30:00.000Z",   // Última tentativa
    "x-send-dlq": "2025-09-28T10:30:05.000Z"        // Timestamp do envio para DLQ
}
```

### 🔍 **Monitoramento de DLQ**

```csharp
public class DlqMonitorService
{
    public async Task ProcessarDlq()
    {
        // Processar mensagens na DLQ manualmente
        // ou configurar alertas para DLQ com muitas mensagens
    }
}
```

### ⚠️ **Cenários de Envio para DLQ**

- ✅ **Exception não tratada** após MaxRetry tentativas
- ✅ **Timeout de processamento** recorrente
- ✅ **Falhas de conexão** com APIs externas
- ❌ **Erro de dados inválidos** (deve ser tratado sem throw)

## 📋 **Modelos e Interfaces**

### 🌐 **CloudEvent&lt;T&gt; (Padrão CloudEvents)**

```csharp
public class CloudEvent<T>
{
    [JsonPropertyName("specversion")]
    public string SpecVersion { get; set; } = "1.0";

    [JsonPropertyName("type")]
    public string Type { get; set; }              // Tipo do evento

    [JsonPropertyName("source")]
    public string Source { get; set; }            // Origem da mensagem

    [JsonPropertyName("id")]
    public string Id { get; set; }                // ID único da mensagem

    [JsonPropertyName("time")]
    public DateTimeOffset Time { get; set; }      // Timestamp do evento

    [JsonPropertyName("datacontenttype")]
    public string DataContentType { get; set; } = "application/json";

    [JsonPropertyName("data")]
    public T Data { get; set; }                   // Payload da mensagem
}
```

### 🔌 **IMessagingPublisher**

```csharp
public interface IMessagingPublisher
{
    /// <summary>
    /// Publica uma mensagem no exchange especificado
    /// </summary>
    Task PublishAsync<T>(
        string exchangeName,    // Exchange de destino
        T message,             // Dados da mensagem
        string typeEvent,      // Tipo do evento (ex: "pedido.criado")
        string source,         // Origem (ex: "api.pedidos")
        string messageId = "", // ID único (auto-gerado se vazio)
        string routingKey = "" // Chave de roteamento (opcional)
    );
}
```

### 📥 **IMessagingConsumer**

```csharp
public interface IMessagingConsumer
{
    // ✅ Obrigatórias
    string ExchangeName { get; }
    Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken);

    // ⚙️ Opcionais (com valores padrão)
    string QueueName => /* gerado automaticamente */;
    string RoutingKey => "";
    int MaxRetry => 5;
    ushort PrefetchCount => 3;
    QueueType QueueType => QueueType.Quorum;
}
```

### 🏗️ **Tipos de Fila (QueueType)**

```csharp
public enum QueueType
{
    Quorum,  // ✅ Recomendado - Alta disponibilidade e durabilidade
    Classic  // ⚠️  Tradicional - Para compatibilidade com versões antigas
}
```

## 🔧 **Configuração**

### ⚙️ **appsettings.json**

```json
{
  "RabbitMq": {
    "Host": "localhost", // Endereço do servidor RabbitMQ
    "Port": 5672, // Porta de conexão (padrão: 5672)
    "Username": "admin", // Usuário de autenticação
    "Password": "senha123", // Senha de autenticação
    "VHost": "/", // Virtual Host (padrão: "/")
    "GlobalPrefetchCount": 10, // QoS global (padrão: 3)
    "UseGlobalPrefetch": true // Usar QoS global (padrão: true)
  }
}
```

### 🚀 **Configuração no Program.cs**

```csharp
using Devplus.Messaging;

var builder = WebApplication.CreateBuilder(args);

// ✅ Registrar a biblioteca Messaging
builder.Services.AddMessaging(builder.Configuration);

// ✅ Registrar seus consumidores
builder.Services.AddScoped<IMessagingConsumer, ProdutoCriadoConsumer>();
builder.Services.AddScoped<IMessagingConsumer, PedidoCanceladoConsumer>();
builder.Services.AddScoped<IMessagingConsumer, EmailEnviadoConsumer>();

// ✅ Registrar outros serviços
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// O RabbitMqHostedService será iniciado automaticamente
app.Run();
```

### 📊 **Configurações Avançadas de RabbitMQ**

```json
{
  "RabbitMq": {
    "Host": "rabbitmq-cluster.empresa.com",
    "Port": 5672,
    "Username": "app-user",
    "Password": "senha-super-segura",
    "VHost": "/producao",
    "GlobalPrefetchCount": 50, // Para alto throughput
    "UseGlobalPrefetch": false // QoS por consumidor individual
  }
}
```

## 🧪 **Exemplo Completo de Implementação**

### 📦 **Serviço de Publicação**

```csharp
public class PedidoService
{
    private readonly IMessagingPublisher _publisher;
    private readonly IPedidoRepository _repository;

    public PedidoService(IMessagingPublisher publisher, IPedidoRepository repository)
    {
        _publisher = publisher;
        _repository = repository;
    }

    public async Task CriarPedido(CriarPedidoDto dto)
    {
        // 1. Salvar no banco
        var pedido = new Pedido(dto.ClienteId, dto.Produtos);
        await _repository.SalvarAsync(pedido);

        // 2. Publicar eventos
        await _publisher.PublishAsync(
            exchangeName: "pedidos-exchange",
            message: new PedidoCriadoEvent
            {
                PedidoId = pedido.Id,
                ClienteId = pedido.ClienteId,
                ValorTotal = pedido.ValorTotal,
                DataCriacao = pedido.DataCriacao
            },
            source: "api.pedidos",
            typeEvent: "pedido.criado",
            routingKey: "pedidos.criados"
        );

        // 3. Evento para estoque
        await _publisher.PublishAsync(
            exchangeName: "estoque-exchange",
            message: new { PedidoId = pedido.Id, Produtos = pedido.Produtos },
            source: "api.pedidos",
            typeEvent: "estoque.reserva-solicitada",
            routingKey: "estoque.reservas"
        );
    }
}
```

### 📥 **Múltiplos Consumidores**

```csharp
// ✅ Consumidor para notificações por email
public class NotificacaoEmailConsumer : IMessagingConsumer
{
    public string ExchangeName => "pedidos-exchange";
    public string QueueName => "notificacoes-email-queue";
    public string RoutingKey => "pedidos.criados";

    private readonly IEmailService _emailService;

    public async Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken)
    {
        var pedido = JsonSerializer.Deserialize<PedidoCriadoEvent>(cloudEvent.Data.ToString());
        await _emailService.EnviarConfirmacao(pedido.ClienteId, pedido.PedidoId);
    }
}

// ✅ Consumidor para integração com ERP
public class IntegracaoErpConsumer : IMessagingConsumer
{
    public string ExchangeName => "pedidos-exchange";
    public string QueueName => "integracao-erp-queue";
    public string RoutingKey => "pedidos.criados";
    public int MaxRetry => 10; // ERP pode estar instável

    private readonly IErpService _erpService;

    public async Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken)
    {
        var pedido = JsonSerializer.Deserialize<PedidoCriadoEvent>(cloudEvent.Data.ToString());
        await _erpService.SincronizarPedido(pedido);
    }
}

// ✅ Consumidor para reserva de estoque
public class ReservaEstoqueConsumer : IMessagingConsumer
{
    public string ExchangeName => "estoque-exchange";
    public string QueueName => "reservas-estoque-queue";
    public string RoutingKey => "estoque.reservas";
    public QueueType QueueType => QueueType.Quorum; // Alta disponibilidade

    private readonly IEstoqueService _estoqueService;

    public async Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken)
    {
        var reserva = JsonSerializer.Deserialize<ReservaEstoqueEvent>(cloudEvent.Data.ToString());
        await _estoqueService.ReservarProdutos(reserva.PedidoId, reserva.Produtos);
    }
}
```

## ⚡ **Funcionalidades Técnicas**

### 🏗️ **Arquitetura da Biblioteca**

```
Devplus.Messaging/
├── src/
│   ├── Configuration/
│   │   └── RabbitMqConfig.cs           # Configurações RabbitMQ
│   ├── Enum/
│   │   └── QueueType.cs                # Tipos de fila (Quorum/Classic)
│   ├── Interfaces/
│   │   ├── IMessagingConsumer.cs       # Interface para consumidores
│   │   └── IMessagingPublisher.cs      # Interface para publicação
│   ├── Models/
│   │   └── CloudEvent.cs               # Modelo CloudEvents
│   ├── Services/
│   │   ├── RabbitMqHostedService.cs    # Background service para consumidores
│   │   └── RabbitMqPublisher.cs        # Implementação do publisher
│   └── MessagingServiceCollectionExtensions.cs # DI configuration
```

### 🔗 **Recursos Automáticos**

| Recurso                  | Descrição                                          |
| ------------------------ | -------------------------------------------------- |
| **Exchange Declaration** | Criação automática de exchanges do tipo `topic`    |
| **Queue Declaration**    | Criação automática de filas (principais e DLQ)     |
| **Binding Automation**   | Vinculação automática entre exchanges e filas      |
| **DLX Setup**            | Configuração automática de Dead Letter Exchange    |
| **Reconnection**         | Reconexão automática em caso de falhas             |
| **QoS Management**       | Gerenciamento de Quality of Service por consumidor |

### 📊 **Métricas e Monitoramento**

```csharp
// Logs automáticos gerados pela biblioteca
[INFO] Message publish - Exchange: produtos-exchange, RoutingKey: produtos.criados, MessageId: abc123
[INFO] Message received Queue: produtos-queue Consumer: ProdutoCriadoConsumer
[WARN] Erro ao processar mensagem da fila produtos-queue - Retry 2/5
[ERROR] Mensagem enviada para DLQ após 5 tentativas - Queue: produtos-queue-dlq
[WARN] Canal do consumidor ProdutoCriadoConsumer foi encerrado. Reason: Connection lost
[INFO] Consumidor ProdutoCriadoConsumer reconectado com sucesso
```

### �️ **Tratamento de Erros e Resiliência**

```csharp
public class ProcessadorPagamentos : IMessagingConsumer
{
    public string ExchangeName => "pagamentos-exchange";
    public int MaxRetry => 3;

    public async Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken)
    {
        try
        {
            await ProcessarPagamento(cloudEvent.Data);
        }
        catch (PaymentGatewayException ex) when (ex.IsRetryable)
        {
            // ✅ Erro temporário - será reprocessado
            _logger.LogWarning("Falha temporária no gateway: {Error}", ex.Message);
            throw; // Reprocessar
        }
        catch (InvalidCardException ex)
        {
            // ❌ Erro permanente - não reprocessar
            _logger.LogError("Cartão inválido: {Error}", ex.Message);
            await _notificationService.NotificarCartaoInvalido(ex.CardId);
            // Não faz throw - vai para ACK
        }
        catch (Exception ex)
        {
            // ⚠️ Erro desconhecido - reprocessar
            _logger.LogError(ex, "Erro inesperado no processamento");
            throw;
        }
    }
}
```

### 🔧 **Configurações de Performance**

```json
{
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "user",
    "Password": "pass",
    "VHost": "/",
    "GlobalPrefetchCount": 100, // Alto throughput
    "UseGlobalPrefetch": false // QoS individual por consumidor
  }
}
```

```csharp
// Consumidor otimizado para alta performance
public class HighVolumeConsumer : IMessagingConsumer
{
    public string ExchangeName => "eventos-volume-exchange";
    public ushort PrefetchCount => 50;           // Processar até 50 mensagens simultâneas
    public QueueType QueueType => QueueType.Quorum; // Alta disponibilidade
    public int MaxRetry => 2;                    // Retry rápido

    public async Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken)
    {
        // Processamento otimizado e rápido
        await ProcessarRapidamente(cloudEvent.Data);
    }
}
```

## 🛠️ **Compatibilidade e Versioning**

| .NET Version   | Suportado          | RabbitMQ Client |
| -------------- | ------------------ | --------------- |
| .NET 8.0       | ✅ **Recomendado** | 6.8.x           |
| .NET 7.0       | ✅ Sim             | 6.8.x           |
| .NET 6.0       | ✅ Sim             | 6.8.x           |
| .NET Core 3.1  | ⚠️ Limitado        | 6.x             |
| .NET Framework | ❌ Não             | -               |

### 📈 **Versionamento Semântico**

- **Major (X.y.z)**: Mudanças incompatíveis na API
- **Minor (x.Y.z)**: Novas funcionalidades compatíveis
- **Patch (x.y.Z)**: Correções de bugs

## 🚀 **Docker e Desenvolvimento**

### 🐳 **RabbitMQ com Docker Compose**

A biblioteca inclui um `docker-compose.yaml` para desenvolvimento local:

```yaml
# docker/docker-compose.yaml
version: "3.8"
services:
  rabbitmq:
    image: rabbitmq:3.13-management
    container_name: devplus-rabbitmq
    ports:
      - "5672:5672" # AMQP port
      - "15672:15672" # Management UI
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: admin123
      RABBITMQ_DEFAULT_VHOST: /
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq

volumes:
  rabbitmq_data:
```

**Comandos úteis**:

```bash
# Iniciar RabbitMQ
docker-compose -f docker/docker-compose.yaml up -d

# Acessar Management UI
open http://localhost:15672
# Usuário: admin, Senha: admin123

# Parar RabbitMQ
docker-compose -f docker/docker-compose.yaml down
```

## 🏗️ **Melhores Práticas**

### ✅ **Recomendações**

1. **📝 Nomeação Consistente**: Use padrões como `{dominio}-{acao}-exchange`
2. **🔄 Retry Inteligente**: Configure `MaxRetry` baseado no tipo de operação
3. **📊 QoS Adequado**: Ajuste `PrefetchCount` conforme capacidade de processamento
4. **🏗️ Filas Quorum**: Use `QueueType.Quorum` para alta disponibilidade
5. **🔍 Logs Detalhados**: Implemente logging estruturado nos consumidores
6. **⚠️ Tratamento de Erros**: Diferencie erros temporários de permanentes
7. **🎯 Routing Keys**: Use routing keys para roteamento específico

### ❌ **Evite**

- ❌ Processar mensagens grandes (>1MB) - use referências
- ❌ Operações síncronas longas nos consumidores
- ❌ Fazer throw para erros de dados inválidos
- ❌ Usar `PrefetchCount` muito alto sem CPU/memória adequada
- ❌ Ignorar mensagens na DLQ sem monitoramento

### 🔧 **Exemplo de Configuração de Produção**

```json
{
  "RabbitMq": {
    "Host": "rabbitmq-cluster.prod.empresa.com",
    "Port": 5672,
    "Username": "${RABBITMQ_USER}",
    "Password": "${RABBITMQ_PASSWORD}",
    "VHost": "/producao",
    "GlobalPrefetchCount": 20,
    "UseGlobalPrefetch": false
  }
}
```

## 📄 **Licença**

Este projeto está licenciado sob a [MIT License](../LICENSE).

---

## 📞 **Suporte**

Para dúvidas, sugestões ou problemas:

📧 **Email**: [clayton@devplus.com.br](mailto:clayton@devplus.com.br)  
🔗 **LinkedIn**: [Clayton Oliveira](https://www.linkedin.com/in/clayton-oliveira-7929b121/)  
🚀 **Website**: [www.devplus.com.br](https://www.devplus.com.br)

### 🤝 **Contribuições**

Contribuições são bem-vindas! Para contribuir:

1. 🍴 **Fork** o repositório
2. 🌿 **Crie uma branch**: `git checkout -b feature/nova-funcionalidade`
3. 💻 **Implemente** sua funcionalidade com testes
4. 📝 **Commit**: `git commit -m "feat: adicionar nova funcionalidade"`
5. 📤 **Push**: `git push origin feature/nova-funcionalidade`
6. 🔄 **Abra um Pull Request** com descrição detalhada

---

**Devplus.Messaging v2.7.4** - Mensageria robusta e escalável para aplicações .NET 🚀
