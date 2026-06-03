# RabbitMQ Publish/Subscribe și Consumer - Flux de Date

Această diagramă ilustrează arhitectura de mesagerie asincronă cu RabbitMQ, pattern-ul Publish/Subscribe și procesarea mesajelor prin consumatori în fundal.

## Diagrama Mermaid

```mermaid
graph TB
    subgraph "API Layer - Publishers"
        PostAPI[Posts Controller]
        CommentAPI[Comments Controller]
        LikeAPI[Likes Controller]
        UserAPI[Users Controller]
    end
    
    subgraph "Application Services"
        PostService[Post Service]
        NotifService[Notification Service]
        EmailService[Email Service]
        Publisher[RabbitMQ Message Publisher]
    end
    
    subgraph "RabbitMQ Broker"
        Exchange1[📮 Exchange: notifications<br/>Type: Topic]
        Exchange2[📮 Exchange: emails<br/>Type: Fanout]
        
        Queue1[📬 Queue: post.liked]
        Queue2[📬 Queue: post.commented]
        Queue3[📬 Queue: post.published]
        Queue4[📬 Queue: user.followed]
        Queue5[📬 Queue: email.verification]
        Queue6[📬 Queue: email.notification]
        
        DLQ1[☠️ Dead Letter Queue<br/>Failed Messages]
    end
    
    subgraph "Background Workers - Consumers"
        Consumer1[🔄 Notification Consumer]
        Consumer2[📧 Email Consumer]
        Consumer3[📊 Analytics Consumer]
        Consumer4[🔍 Search Indexer]
    end
    
    subgraph "External Services"
        SignalR[SignalR Hub<br/>Real-time Notifications]
        SMTP[SMTP Server<br/>Email Sending]
        Elastic[(Elasticsearch<br/>Search Index)]
        SQL[(SQL Server<br/>Database)]
    end
    
    %% Publishing flow
    PostAPI -->|1. Postare apreciată| PostService
    PostService -->|2. Business logic| Publisher
    Publisher -->|3. Publish message| Exchange1
    
    CommentAPI -->|Comentariu nou| PostService
    PostService --> Publisher
    
    LikeAPI -->|Like adăugat| PostService
    UserAPI -->|Utilizator urmărit| PostService
    
    %% Email publishing
    NotifService -->|Email verification| Publisher
    EmailService -->|Email notification| Publisher
    Publisher -->|Publish email| Exchange2
    
    %% Exchange to Queue routing
    Exchange1 -->|Routing key:<br/>post.liked| Queue1
    Exchange1 -->|Routing key:<br/>post.commented| Queue2
    Exchange1 -->|Routing key:<br/>post.published| Queue3
    Exchange1 -->|Routing key:<br/>user.followed| Queue4
    
    Exchange2 -->|Broadcast| Queue5
    Exchange2 -->|Broadcast| Queue6
    
    %% Consumer processing
    Queue1 -->|4. Consume| Consumer1
    Queue2 -->|Consume| Consumer1
    Queue3 -->|Consume| Consumer1
    Queue4 -->|Consume| Consumer1
    
    Queue5 -->|Consume| Consumer2
    Queue6 -->|Consume| Consumer2
    
    Queue1 -.->|Analytics| Consumer3
    Queue2 -.->|Analytics| Consumer3
    
    Queue3 -->|Index post| Consumer4
    
    %% Consumer actions
    Consumer1 -->|5. Creare notificare| SQL
    Consumer1 -->|6. Push real-time| SignalR
    SignalR -->|7. WebSocket| User[👤 Utilizator]
    
    Consumer2 -->|Trimitere email| SMTP
    SMTP -.->|Email| User
    
    Consumer3 -->|Salvare metrici| SQL
    Consumer4 -->|Indexare| Elastic
    
    %% Error handling
    Queue1 -.->|Retry failed| DLQ1
    Queue2 -.->|Retry failed| DLQ1
    Queue5 -.->|Retry failed| DLQ1
    
    DLQ1 -.->|Manual reprocess| Consumer1
    
    %% Message flow annotations
    Publisher -.->|Message Properties:<br/>- MessageId<br/>- Timestamp<br/>- UserId<br/>- EventType| Exchange1
    
    %% Styling
    classDef apiClass fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    classDef serviceClass fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    classDef brokerClass fill:#fff9c4,stroke:#f57f17,stroke-width:3px
    classDef consumerClass fill:#e8f5e9,stroke:#388e3c,stroke-width:2px
    classDef externalClass fill:#ffebee,stroke:#c62828,stroke-width:2px
    classDef queueClass fill:#ffe0b2,stroke:#e65100,stroke-width:2px
    classDef dlqClass fill:#ffcdd2,stroke:#b71c1c,stroke-width:2px
    
    class PostAPI,CommentAPI,LikeAPI,UserAPI apiClass
    class PostService,NotifService,EmailService,Publisher serviceClass
    class Exchange1,Exchange2 brokerClass
    class Queue1,Queue2,Queue3,Queue4,Queue5,Queue6 queueClass
    class DLQ1 dlqClass
    class Consumer1,Consumer2,Consumer3,Consumer4 consumerClass
    class SignalR,SMTP,Elastic,SQL externalClass
```

## Explicație Arhitectură

### 1. **Publishers (Producători)**
```csharp
// Exemplu: Publicare mesaj când o postare este apreciată
await _messagePublisher.PublishNotificationAsync(new PostLikedMessage
{
    PostId = postId,
    PostTitle = post.Title,
    PostAuthorId = post.AuthorId,
    LikerUserId = currentUserId,
    LikerUserName = currentUser.UserName
});
```

### 2. **RabbitMQ Exchanges**

#### **Topic Exchange (notifications)**
- Routing flexibil bazat pe routing keys
- Pattern matching: `post.*`, `user.*`, `comment.*`
- Mesaje rutate către cozi specifice

#### **Fanout Exchange (emails)**
- Broadcast către toate cozile conectate
- Fără routing keys
- Ideal pentru notificări email

### 3. **Message Queues**
- **Durabilitate**: Mesajele persistă în caz de restart
- **Acknowledgment**: Manual ACK după procesare cu succes
- **Prefetch**: Limită de mesaje procesate simultan
- **TTL**: Time-to-live pentru mesaje vechi

### 4. **Consumers (Consumatori)**

#### **Notification Consumer**
```csharp
public async Task HandlePostLikedAsync(PostLikedMessage message)
{
    // 1. Salvează notificarea în DB
    await _notificationService.CreateNotificationAsync(...);
    
    // 2. Trimite prin SignalR în timp real
    await _hubService.SendNotificationAsync(...);
}
```

#### **Email Consumer**
```csharp
public async Task HandleEmailAsync(EmailNotificationMessage message)
{
    // Procesare în batch pentru eficiență
    await _emailService.SendEmailAsync(message);
}
```

### 5. **Dead Letter Queue (DLQ)**
- Mesaje care au eșuat după multiple reîncercări
- Monitorizare și alertare
- Reprocesare manuală sau automată

## Pattern-uri Implementate

### ✅ **Publish/Subscribe**
- Decuplare între producători și consumatori
- Un mesaj poate fi procesat de multiple consumatori
- Scalabilitate orizontală

### ✅ **Competing Consumers**
- Multiple instanțe de consumatori pentru aceeași coadă
- Load balancing automat
- Procesare paralelă

### ✅ **Retry Pattern**
- Reîncercări automate cu exponential backoff
- Dead letter queue pentru mesaje eșuate
- Circuit breaker pentru servicii externe

### ✅ **Idempotency**
- Mesajele pot fi procesate de multiple ori fără efecte secundare
- MessageId unic pentru deduplicare
- Verificare în DB înainte de procesare

## Avantaje Arhitectură

✅ **Asincronicitate**: Procesare în fundal fără blocare  
✅ **Scalabilitate**: Adăugare ușoară de consumatori  
✅ **Reziliență**: Retry automat și DLQ  
✅ **Decuplare**: Servicii independente  
✅ **Performanță**: Procesare paralelă  

## Configurare RabbitMQ

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchanges": {
      "Notifications": {
        "Name": "notifications",
        "Type": "topic",
        "Durable": true
      },
      "Emails": {
        "Name": "emails",
        "Type": "fanout",
        "Durable": true
      }
    }
  }
}
```

## Monitorizare

- **RabbitMQ Management UI**: http://localhost:15672
- **Metrici**: Message rate, consumer utilization, queue depth
- **Alerting**: Queue size thresholds, consumer failures
