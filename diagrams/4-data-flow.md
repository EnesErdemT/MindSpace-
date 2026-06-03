# Veri Akışı - Senkron, Asenkron ve Real-Time

```mermaid
graph TB
    subgraph Sync["🔄 SENKRON İLETİŞİM (HTTP REST API)"]
        Client1[Client<br/>Next.js]
        API1[API Controller<br/>ASP.NET Core]
        Service1[Service Layer]
        Repo1[Repository]
        DB1[(PostgreSQL<br/>Database)]
        
        Client1 -->|HTTP Request<br/>GET/POST/PUT/DELETE| API1
        API1 -->|Business Logic| Service1
        Service1 -->|Query/Command| Repo1
        Repo1 -->|SQL| DB1
        DB1 -->|Result| Repo1
        Repo1 -->|Entity| Service1
        Service1 -->|DTO| API1
        API1 -->|HTTP Response<br/>JSON| Client1
    end
    
    subgraph Async["⚡ ASENKRON İLETİŞİM (Message Queue)"]
        Producer[Producer<br/>API/Service]
        MQ[RabbitMQ<br/>Message Broker]
        Consumer1[Notification<br/>Consumer]
        Consumer2[Email<br/>Consumer]
        Consumer3[Analytics<br/>Consumer]
        
        Producer -->|Publish Message| MQ
        MQ -->|Subscribe| Consumer1
        MQ -->|Subscribe| Consumer2
        MQ -->|Subscribe| Consumer3
        
        Consumer1 -->|Create Notification| DB2[(Database)]
        Consumer1 -->|Send Real-time| SignalR1[SignalR Hub]
        Consumer2 -->|Send Email| SMTP[SMTP Server]
        Consumer3 -->|Track Event| Analytics[(Analytics DB)]
    end
    
    subgraph Realtime["🔴 REAL-TIME İLETİŞİM (WebSocket)"]
        Client2[Client<br/>Browser]
        SignalR2[SignalR Hub<br/>ASP.NET Core]
        
        Client2 <-->|WebSocket<br/>Bidirectional| SignalR2
        
        SignalR2 -->|Broadcast| Group1[User Group]
        SignalR2 -->|Broadcast| Group2[Post Room]
        SignalR2 -->|Send| Client3[Specific Client]
    end
    
    subgraph Search["🔍 ARAMA SİSTEMİ (Full-Text Search)"]
        SearchClient[Client]
        SearchAPI[Search API]
        ES[(Elasticsearch<br/>Cluster)]
        
        SearchClient -->|Search Query| SearchAPI
        SearchAPI -->|Multi-field Search<br/>Fuzzy Matching| ES
        ES -->|Ranked Results<br/>Highlights| SearchAPI
        SearchAPI -->|Search Response| SearchClient
        
        IndexService[Indexing Service]
        IndexService -->|Index/Update/Delete| ES
    end
    
    %% Cross-connections
    API1 -.->|Async| Producer
    Service1 -.->|Index Post| IndexService
    Consumer1 -.->|Notify| SignalR2
    
    %% Styling
    classDef syncStyle fill:#4A90E2,stroke:#2E5C8A,stroke-width:2px,color:#fff
    classDef asyncStyle fill:#F5A623,stroke:#B87A1A,stroke-width:2px,color:#fff
    classDef realtimeStyle fill:#D0021B,stroke:#9A0115,stroke-width:2px,color:#fff
    classDef searchStyle fill:#7ED321,stroke:#5A9B18,stroke-width:2px,color:#fff
    
    class Sync syncStyle
    class Async asyncStyle
    class Realtime realtimeStyle
    class Search searchStyle
```

**İletişim Modelleri:**

### 1. Senkron İletişim (HTTP REST API)
- **Kullanım**: CRUD işlemleri, veri sorgulama
- **Özellik**: Request-Response pattern, client bekler
- **Örnek**: Post listesi getirme, kullanıcı profili güncelleme

### 2. Asenkron İletişim (RabbitMQ)
- **Kullanım**: Uzun süren işlemler, bildirimler
- **Özellik**: Fire-and-forget, retry mekanizması
- **Örnek**: Email gönderme, bildirim oluşturma, analytics

### 3. Real-Time İletişim (SignalR)
- **Kullanım**: Anlık güncellemeler, canlı bildirimler
- **Özellik**: Bidirectional, push notifications
- **Örnek**: Yeni bildirim, yeni yorum, like counter

### 4. Arama Sistemi (Elasticsearch)
- **Kullanım**: Full-text search, filtreleme
- **Özellik**: Fuzzy matching, relevance scoring
- **Örnek**: Post arama, tag arama, yazar arama
