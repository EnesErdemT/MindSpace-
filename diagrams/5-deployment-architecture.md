# Deployment Architecture - Production Environment

```mermaid
graph TB
    subgraph Internet["🌐 INTERNET"]
        Users[Users<br/>Web Browsers]
    end
    
    subgraph DMZ["🔒 DMZ (Demilitarized Zone)"]
        LB[Load Balancer<br/>Nginx<br/>Port: 80, 443]
        Firewall[Firewall<br/>Security Rules]
    end
    
    subgraph AppTier["🖥️ APPLICATION TIER"]
        API1[API Server 1<br/>.NET 9<br/>Port: 5000]
        API2[API Server 2<br/>.NET 9<br/>Port: 5000]
        API3[API Server 3<br/>.NET 9<br/>Port: 5000]
    end
    
    subgraph DataTier["💾 DATA TIER"]
        subgraph DBCluster["PostgreSQL Cluster"]
            DBMaster[(PostgreSQL<br/>Master<br/>Port: 5432)]
            DBSlave1[(PostgreSQL<br/>Slave 1)]
            DBSlave2[(PostgreSQL<br/>Slave 2)]
        end
        
        subgraph MQCluster["RabbitMQ Cluster"]
            MQ1[RabbitMQ<br/>Node 1<br/>Port: 5672]
            MQ2[RabbitMQ<br/>Node 2<br/>Port: 5672]
        end
        
        subgraph ESCluster["Elasticsearch Cluster"]
            ES1[Elasticsearch<br/>Node 1<br/>Port: 9200]
            ES2[Elasticsearch<br/>Node 2<br/>Port: 9200]
            ES3[Elasticsearch<br/>Node 3<br/>Port: 9200]
        end
        
        Cache[(Redis Cache<br/>Port: 6379)]
    end
    
    subgraph Storage["📦 STORAGE"]
        FileStorage[File Storage<br/>Images, Documents]
        Backup[Backup Storage<br/>Daily Backups]
    end
    
    subgraph Monitoring["📊 MONITORING & LOGGING"]
        Prometheus[Prometheus<br/>Metrics]
        Grafana[Grafana<br/>Dashboards]
        Seq[Seq<br/>Log Aggregation]
    end
    
    %% Connections
    Users -->|HTTPS| LB
    LB --> Firewall
    Firewall --> API1
    Firewall --> API2
    Firewall --> API3
    
    API1 --> DBMaster
    API2 --> DBMaster
    API3 --> DBMaster
    
    DBMaster -.->|Replication| DBSlave1
    DBMaster -.->|Replication| DBSlave2
    
    API1 --> MQ1
    API2 --> MQ1
    API3 --> MQ2
    MQ1 <-.->|Cluster| MQ2
    
    API1 --> ES1
    API2 --> ES2
    API3 --> ES3
    ES1 <-.->|Cluster| ES2
    ES2 <-.->|Cluster| ES3
    
    API1 --> Cache
    API2 --> Cache
    API3 --> Cache
    
    API1 -.->|Upload| FileStorage
    API2 -.->|Upload| FileStorage
    API3 -.->|Upload| FileStorage
    
    DBMaster -.->|Backup| Backup
    
    API1 -.->|Metrics| Prometheus
    API2 -.->|Metrics| Prometheus
    API3 -.->|Metrics| Prometheus
    Prometheus --> Grafana
    
    API1 -.->|Logs| Seq
    API2 -.->|Logs| Seq
    API3 -.->|Logs| Seq
    
    %% Styling
    classDef internetStyle fill:#E8F4F8,stroke:#4A90E2,stroke-width:2px
    classDef dmzStyle fill:#FFF4E6,stroke:#F5A623,stroke-width:2px
    classDef appStyle fill:#E8F5E9,stroke:#7ED321,stroke-width:2px
    classDef dataStyle fill:#FCE4EC,stroke:#D0021B,stroke-width:2px
    classDef storageStyle fill:#F3E5F5,stroke:#9C27B0,stroke-width:2px
    classDef monitorStyle fill:#E0F2F1,stroke:#009688,stroke-width:2px
    
    class Internet internetStyle
    class DMZ dmzStyle
    class AppTier appStyle
    class DataTier dataStyle
    class Storage storageStyle
    class Monitoring monitorStyle
```

**Deployment Özellikleri:**

### 🔒 Security Layer
- **Load Balancer**: Nginx ile SSL termination, rate limiting
- **Firewall**: IP filtering, DDoS protection
- **HTTPS**: TLS 1.3, Let's Encrypt certificates

### 🖥️ Application Layer
- **3 API Servers**: Horizontal scaling, load balancing
- **Health Checks**: /health endpoint monitoring
- **Auto-scaling**: CPU/Memory based scaling

### 💾 Data Layer
- **PostgreSQL Cluster**: Master-Slave replication, automatic failover
- **RabbitMQ Cluster**: High availability, message persistence
- **Elasticsearch Cluster**: 3-node cluster, shard replication
- **Redis Cache**: In-memory caching, session storage

### 📦 Storage
- **File Storage**: CDN integration, image optimization
- **Backup**: Daily automated backups, 30-day retention

### 📊 Monitoring
- **Prometheus**: Metrics collection (CPU, memory, requests/sec)
- **Grafana**: Real-time dashboards, alerting
- **Seq**: Structured logging, log search

### 🔧 Infrastructure as Code
```yaml
# docker-compose.yml
version: '3.8'
services:
  nginx:
    image: nginx:alpine
    ports: ["80:80", "443:443"]
  
  api:
    image: mindspace-api:latest
    replicas: 3
    
  postgres:
    image: postgres:16
    
  rabbitmq:
    image: rabbitmq:3.12-management
    
  elasticsearch:
    image: elasticsearch:8.11.0
    
  redis:
    image: redis:7-alpine
```
