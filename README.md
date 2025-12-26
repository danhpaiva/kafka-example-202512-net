# 🚀 Kafka Event-Driven Architecture – .NET 10

![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Apache Kafka](https://img.shields.io/badge/Apache_Kafka-231F20?style=for-the-badge&logo=apachekafka&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

Ecossistema completo de **arquitetura orientada a eventos** utilizando **Apache Kafka** e **.NET 10**.

O projeto demonstra desde a **produção de eventos** até **processamento resiliente**, **Dead Letter Queue (DLQ)** e **reprocessamento automático**, aplicando boas práticas de **mensageria distribuída**, **tolerância a falhas** e **desacoplamento**.

Projeto com foco em **escalabilidade, confiabilidade, observabilidade e resiliência de dados**.

---

## 🏗️ Arquitetura

```
Producer
   ↓
Kafka (vendas-pedidos)
   ↓
KafkaWorker
   ├─ Sucesso → Commit Manual
   └─ Falha → DLQ (vendas-pedidos-erros)
                     ↓
              KafkaRetryWorker
                     ↓
            Reenvio ao tópico principal
```

---

## 🚀 Tecnologias Utilizadas

- **.NET 10**
- **C#**
- **Apache Kafka**
- **Confluent.Kafka**
- **Background Services (Worker Service)**
- **System.Text.Json**
- **Docker & Docker Compose**
- **Kafka UI**

---

## 📦 Estrutura da Solution

```
Kafka.EventDrivenArchitecture
│
├── Kafka.Producer      → Console App (Publicação de eventos)
├── Kafka.Consumer      → Console App (Consumo simples / debug)
└── Kafka.Worker        → Worker Service (Processamento + DLQ + Retry)
```

---

## 📢 Kafka.Producer

Responsável por **publicar eventos de pedidos de venda** no Kafka.

### Características
- Producer **assíncrono**
- Uso de **Key** para particionamento
- Tolerância a falhas com timeout
- Serialização com `System.Text.Json`

---

## 📥 Kafka.Consumer

Consumer simples para:
- Visualização de mensagens
- Testes locais
- Debug de offsets

### Características
- Auto Commit habilitado
- Tratamento de **Poison Pill** (JSON inválido)
- Graceful Shutdown

---

## ⚙️ Kafka.Worker (Processamento Resiliente)

Worker Service responsável pelo **processamento de negócio**.

### Características
- Execução contínua (24/7)
- **Commit manual**
- Garantia de **At-Least-Once Delivery**
- Tratamento de falhas de negócio
- Envio para **DLQ** em caso de erro

---

## 🔄 KafkaRetryWorker (DLQ Monitor)

Worker dedicado ao **reprocessamento de mensagens com falha**.

### Características
- Consumo do tópico DLQ
- Commit manual
- Retry com delay (Backoff simples)
- Reenvio ao tópico principal
- Uso de headers (`retry-count`)

---

## 📦 Modelo de Evento

```csharp
public record Order(
    int Id,
    string Product,
    decimal Price,
    DateTime CreatedAt
);
```

---

## ⚙️ Configuração (appsettings.json)

```json
{
  "KafkaConfig": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "vendas-worker-group",
    "Topic": "vendas-pedidos",
    "DLQTopic": "vendas-pedidos-erros"
  }
}
```

---

## 🐳 Ambiente Kafka (Docker)

O projeto utiliza Kafka em modo **KRaft** (sem Zookeeper) e **Kafka UI**.

### ▶️ Subir infraestrutura

```bash
cd docker
docker compose up -d
```

### 🔗 Serviços

- Kafka Broker: `localhost:9092`
- Kafka UI: `http://localhost:8080`

---

## ▶️ Executar Localmente

### Clone o repositório

```bash
git clone https://github.com/seu-usuario/kafka-event-driven-net
cd kafka-event-driven-net
```

### Suba o Kafka

```bash
docker compose up -d
```

### Execute o Worker

```bash
dotnet run --project Kafka.Worker
```

### Execute o Producer

```bash
dotnet run --project Kafka.Producer
```

---

## 🛡️ Resiliência e Confiabilidade

- Commit manual de offsets
- Dead Letter Queue (DLQ)
- Retry automático
- Poison Pill Handling
- Graceful Shutdown
- Separação clara de responsabilidades

---

## 📄 Licença

Este projeto está licenciado sob a licença MIT.

---

## 👨‍💻 Autor

**Daniel Paiva**  
Desenvolvedor .NET

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/danhpaiva/)
