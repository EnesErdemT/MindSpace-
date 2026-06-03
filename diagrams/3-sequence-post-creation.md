# Sequence Diagram - Post Oluşturma ve Bildirim Akışı

```mermaid
sequenceDiagram
    actor Client
    participant API as API Controller
    participant Service as Post Service
    participant Repo as Repository
    participant DB as PostgreSQL
    participant ES as Elasticsearch
    participant MQ as RabbitMQ
    participant Consumer as Notification Consumer
    participant Hub as SignalR Hub
    participant Email as Email Service
    
    Note over Client,Email: Post Oluşturma Akışı
    
    Client->>+API: POST /api/posts<br/>{title, content, ...}
    
    API->>API: Validate JWT Token
    API->>API: Check Authorization<br/>(Author/Admin role)
    API->>API: Validate Request Model
    
    API->>+Service: CreatePostAsync(request, userId)
    Service->>Service: Calculate ReadTime
    Service->>Service: Generate Slug
    
    Service->>+Repo: AddAsync(post)
    Repo->>+DB: INSERT INTO Posts
    DB-->>-Repo: Post.Id
    Repo-->>-Service: Post Entity
    
    Service->>Service: Map to PostResponse DTO
    Service-->>-API: PostResponse
    
    Note over API,ES: Asenkron İşlemler Başlıyor
    
    par Elasticsearch Indexing
        API->>ES: IndexPostAsync(post)
        Note over ES: Post arama indeksine eklenir
    and RabbitMQ Publishing
        API->>+MQ: Publish(PostPublishedMessage)
        Note over MQ: Mesaj kuyruğa eklenir
        MQ-->>-API: Ack
    end
    
    API-->>-Client: 201 Created<br/>PostResponse
    
    Note over MQ,Email: Arka Plan İşlemleri
    
    MQ->>+Consumer: Consume(PostPublishedMessage)
    
    Consumer->>Consumer: Get Author Followers
    
    loop Her Takipçi İçin
        Consumer->>+DB: INSERT INTO Notifications
        DB-->>-Consumer: Notification.Id
        
        Consumer->>+Hub: SendNotificationToUser(userId, data)
        Hub->>Client: Real-time Notification
        Hub-->>-Consumer: Sent
        
        Consumer->>+Email: SendEmailAsync(follower.Email)
        Email->>Email: Generate HTML Template
        Email->>Email: Send via SMTP
        Email-->>-Consumer: Email Sent
    end
    
    Consumer-->>-MQ: Message Processed
    
    Note over Client,Email: ✅ Post başarıyla oluşturuldu ve takipçiler bilgilendirildi
```

**Akış Açıklaması:**

1. **Senkron İşlemler** (Client bekler):
   - JWT token doğrulama
   - Authorization kontrolü
   - Model validation
   - Veritabanına kayıt
   - Response dönme

2. **Asenkron İşlemler** (Client beklemez):
   - Elasticsearch'e indeksleme
   - RabbitMQ'ya mesaj gönderme

3. **Arka Plan İşlemleri**:
   - Consumer mesajı alır
   - Her takipçi için bildirim oluşturur
   - SignalR ile real-time bildirim gönderir
   - Email gönderir
