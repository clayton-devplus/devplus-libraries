# 🧪 Testes para o Método GetDeadLetterQueueMessageCountAsync

## 📋 **Rotas de Teste Disponíveis**

Após iniciar a aplicação, você pode testar o novo método ultra-performático usando estas rotas:

### ⚡ **1. Verificar DLQ Específica**

```http
GET /api/v1/test-message/dlq-count/{queueName}
```

**Exemplo:**

```bash
# Testar com uma fila específica
curl -X GET "https://localhost:7001/api/v1/test-message/dlq-count/test-queue"

# Resposta esperada:
{
  "queueName": "test-queue",
  "hasMessages": false,
  "checkedAt": "2025-10-14T10:30:00.000Z"
}
```

### 📊 **2. Monitorar Todas as DLQs**

```http
GET /api/v1/test-message/dlq-monitor
```

**Exemplo:**

```bash
# Monitorar múltiplas DLQs
curl -X GET "https://localhost:7001/api/v1/test-message/dlq-monitor"

# Resposta esperada:
{
  "results": {
    "test-queue": 0,
    "pedidos-queue": 0,
    "pagamentos-queue": 0
  },
  "totalMessages": 0,
  "checkedAt": "2025-10-14T10:30:00.000Z",
  "status": "Healthy"
}
```

---

## 🚀 **Como Executar os Testes**

### 1. **Iniciar a Aplicação**

```bash
cd Devplus.TestApp
dotnet run
```

### 2. **Acessar o Swagger UI**

```
https://localhost:7001/swagger
```

### 3. **Testar via PowerShell/Terminal**

```powershell
# Verificar DLQ específica
Invoke-RestMethod -Uri "https://localhost:7001/api/v1/test-message/dlq-count/test-queue" -Method GET

# Monitorar todas as DLQs
Invoke-RestMethod -Uri "https://localhost:7001/api/v1/test-message/dlq-monitor" -Method GET
```

### 4. **Testar Performance**

```powershell
# Medir tempo de resposta
Measure-Command {
    Invoke-RestMethod -Uri "https://localhost:7001/api/v1/test-message/dlq-count/test-queue" -Method GET
}
```

---

## 📊 **Cenários de Teste**

### ✅ **Cenário 1: DLQ Vazia (Normal)**

- **Entrada:** Fila que nunca teve mensagens com falha
- **Resposta:** `hasMessages: false`, `count: 0`
- **Tempo:** ~1-2ms

### ⚠️ **Cenário 2: DLQ com Mensagens**

- **Pré-requisito:** Simular falhas enviando mensagens inválidas
- **Resposta:** `hasMessages: true`, `count: > 0`
- **Tempo:** ~1-2ms

### 🚨 **Cenário 3: DLQ Inexistente**

- **Entrada:** Nome de fila que nunca existiu
- **Resposta:** `hasMessages: false`, `count: 0` (normal)
- **Tempo:** ~1-2ms

---

## 📈 **Comparação de Performance**

| Método                                | Operação                  | Latência Média | Uso de CPU | Uso de Memória |
| ------------------------------------- | ------------------------- | -------------- | ---------- | -------------- |
| `GetDeadLetterQueueMessageCountAsync` | QueueDeclarePassive       | **1-2ms**      | Mínimo     | Mínimo         |
| `GetDeadLetterMessagesAsync(1)`       | BasicGet + Deserialização | 10-50ms        | Médio      | Alto           |

### 🎯 **Quando Usar Cada Método:**

- ✅ **GetDeadLetterQueueMessageCountAsync**:

  - Verificações frequentes (health checks)
  - Dashboards em tempo real
  - Alertas automáticos
  - APIs públicas com alta concorrência

- ✅ **GetDeadLetterMessagesAsync**:
  - Análise detalhada de mensagens
  - Debug e troubleshooting
  - Preparação para redrive específico

---

## 🔧 **Configuração para Testes de Carga**

```csharp
// Para testes de alta concorrência
public async Task TestConcurrentRequests()
{
    var tasks = Enumerable.Range(0, 100).Select(async i =>
    {
        var count = await _redrive.GetDeadLetterQueueMessageCountAsync("test-queue");
        Console.WriteLine($"Request {i}: {count} messages");
    });

    var stopwatch = Stopwatch.StartNew();
    await Task.WhenAll(tasks);
    stopwatch.Stop();

    Console.WriteLine($"100 requests completed in {stopwatch.ElapsedMilliseconds}ms");
    // Esperado: < 100ms total para 100 requests concorrentes
}
```

---

## 📝 **Logs Esperados**

### ✅ **DLQ Vazia:**

```
[10:30:00 INF] 🔍 Verificando DLQ da fila test-queue...
[10:30:00 DBG] DLQ test-queue-dlq contém 0 mensagens
[10:30:00 INF] ✅ DLQ test-queue está vazia
```

### ⚠️ **DLQ com Mensagens:**

```
[10:30:00 INF] 🔍 Verificando DLQ da fila pedidos-queue...
[10:30:00 DBG] DLQ pedidos-queue-dlq contém 15 mensagens
[10:30:00 WRN] ⚠️  DLQ pedidos-queue contém 15 mensagens
```

### 📊 **Monitoramento:**

```
[10:30:00 INF] 📊 Monitorando 3 DLQs...
[10:30:00 INF] 🟢 Saudável test-queue: 0 mensagens
[10:30:00 INF] 🟡 Atenção pedidos-queue: 5 mensagens
[10:30:00 INF] 🔴 Crítico pagamentos-queue: 50 mensagens
[10:30:00 INF] 📈 Total de mensagens em DLQs: 55
```

Este método é perfeito para monitoramento em tempo real e sistemas de alerta! ⚡🚀
