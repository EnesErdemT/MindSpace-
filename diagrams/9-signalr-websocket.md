# SignalR WebSocket - Comunicare Bidirecțională în Timp Real

Această diagramă ilustrează arhitectura SignalR pentru comunicare în timp real între server și clienți, folosind WebSocket protocol pentru notificări instant.

## Diagrama Mermaid

```mermaid
sequenceDiagram
    participant Browser as 🌐 Browser Client
    participant NextJS as Next.js Frontend
    participant SignalRClient as SignalR Client<br/>(@microsoft/signalr)
    participant LoadBalancer as ⚖️ Load Balancer
    participant Server1 as 🖥️ API Server 1<br/>(.NET 9)
    participant Server2 as 🖥️ API Server 2<br/>(.NET 9)
    participant Hub as SignalR Hub<br/>(NotificationHub)
    participant Redis as 📮 Redis Backplane
    participant RabbitMQ as 🐰 RabbitMQ
    participant Consumer as Background Consumer
    participant SQL as 💾 SQL Server
    
    %% Connection establishment
    rect rgb(230, 245, 255)
        Note over Browser,SignalRClient: 1. Stabilire Conexiune WebSocket
        Browser->>NextJS: Încărcare pagină
        NextJS->>SignalRClient: Inițializare SignalR Client
        SignalRClient->>LoadBalancer: WebSocket Handshake
        LoadBalancer->>Server1: Route connection
        Server1->>Hub: Connect user
        Hub->>Redis: Register connection<br/>UserId → ConnectionId
        Redis-->>Hub: ACK
        Hub-->>SignalRClient: Connection established
        SignalRClient-->>Browser: ✅ Connected
    end
    
    %% User action triggering notification
    rect rgb(255, 245, 230)
        Note over Browser,RabbitMQ: 2. Acțiune Utilizator (Like Post)
        Browser->>NextJS: Click "Like" button
        NextJS->>Server2: POST /api/likes
        Server2->>SQL: INSERT Like
        SQL-->>Server2: Success
        Server2->>RabbitMQ: Publish PostLikedMessage
        Server2-->>NextJS: 200 OK
        NextJS-->>Browser: UI update (optimistic)
    end
    
    %% Background processing
    rect rgb(240, 255, 240)
        Note over RabbitMQ,Redis: 3. Procesare Asincronă
        RabbitMQ->>Consumer: Consume PostLikedMessage
        Consumer->>SQL: INSERT Notification
        SQL-->>Consumer: Notification created
        Consumer->>Redis: Get ConnectionId for UserId
        Redis-->>Consumer: ConnectionId(s)
    end
    
    %% Real-time notification delivery
    rect rgb(255, 240, 245)
        Note over Consumer,Browser: 4. Livrare Notificare în Timp Real
        Consumer->>Hub: SendNotificationAsync(userId, notification)
        Hub->>Redis: Publish to backplane
        Redis->>Server1: Broadcast notification
        Redis->>Server2: Broadcast notification
        Server1->>SignalRClient: Push notification via WebSocket
        SignalRClient->>NextJS: Trigger onNotification handler
        NextJS->>Browser: 🔔 Show notification toast
    end
    
    %% Multiple clients scenario
    rect rgb(250, 250, 255)
        Note over Browser,SignalRClient: 5. Utilizator cu Multiple Dispozitive
        Browser->>SignalRClient: Desktop connection
        SignalRClient->>Hub: Connect (same UserId)
        Hub->>Redis: Register 2nd connection
        
        Note over Hub,Redis: Același utilizator conectat<br/>pe desktop și mobile
        
        Hub->>SignalRClient: Broadcast to all connections
        SignalRClient->>Browser: Desktop notification
        SignalRClient->>Browser: Mobile notification
    end
    
    %% Reconnection scenario
    rect rgb(255, 245, 245)
        Note over Browser,Hub: 6. Reconectare Automată
        SignalRClient--xServer1: Connection lost
        SignalRClient->>SignalRClient: Retry with exponential backoff
        SignalRClient->>LoadBalancer: Reconnect attempt
        LoadBalancer->>Server2: Route to different server
        Server2->>Hub: Reconnect user
        Hub->>Redis: Update ConnectionId
        Hub-->>SignalRClient: Reconnected ✅
        SignalRClient->>NextJS: Fetch missed notifications
        NextJS->>Server2: GET /api/notifications/unread
        Server2->>SQL: Query unread
        SQL-->>Server2: Notifications list
        Server2-->>NextJS: JSON response
        NextJS-->>Browser: Display missed notifications
    end
    
    %% Graceful disconnection
    rect rgb(245, 245, 245)
        Note over Browser,Redis: 7. Deconectare
        Browser->>NextJS: User closes tab
        NextJS->>SignalRClient: Dispose connection
        SignalRClient->>Hub: Disconnect
        Hub->>Redis: Remove ConnectionId
        Redis-->>Hub: ACK
    end
```

## Explicație Arhitectură

### 1. **SignalR Hub (Server-Side)**

```csharp
[Authorize]
public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            // Adaugă utilizatorul într-un grup
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            
            // Trimite notificări nelivrate
            var unreadNotifications = await _notificationService
                .GetUnreadNotificationsAsync(userId);
            await Clients.Caller.SendAsync("ReceiveNotifications", unreadNotifications);
        }
        
        await base.OnConnectedAsync();
    }
    
    public async Task SendNotificationToUser(string userId, Notification notification)
    {
        await Clients.Group(userId).SendAsync("ReceiveNotification", notification);
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
```

### 2. **SignalR Client (Frontend - Next.js)**

```typescript
// lib/signalr-connection.ts
import * as signalR from '@microsoft/signalr';

export class NotificationConnection {
  private connection: signalR.HubConnection;
  
  constructor(accessToken: string) {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('https://api.mindspace.com/hubs/notifications', {
        accessTokenFactory: () => accessToken,
        transport: signalR.HttpTransportType.WebSockets,
        skipNegotiation: true
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 0s, 2s, 10s, 30s
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          return 30000;
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();
    
    this.setupHandlers();
  }
  
  private setupHandlers() {
    this.connection.on('ReceiveNotification', (notification) => {
      // Afișează toast notification
      toast.info(notification.message, {
        action: {
          label: 'Vezi',
          onClick: () => router.push(notification.actionUrl)
        }
      });
      
      // Actualizează badge count
      queryClient.invalidateQueries(['notifications', 'unread-count']);
    });
    
    this.connection.onreconnecting((error) => {
      console.log('Reconnecting...', error);
      toast.warning('Reconectare la server...');
    });
    
    this.connection.onreconnected((connectionId) => {
      console.log('Reconnected:', connectionId);
      toast.success('Reconectat cu succes!');
    });
    
    this.connection.onclose((error) => {
      console.error('Connection closed:', error);
      toast.error('Conexiune pierdută. Reîncercăm...');
    });
  }
  
  async start() {
    try {
      await this.connection.start();
      console.log('SignalR Connected');
    } catch (err) {
      console.error('SignalR Connection Error:', err);
      setTimeout(() => this.start(), 5000);
    }
  }
  
  async stop() {
    await this.connection.stop();
  }
}
```

### 3. **React Hook pentru SignalR**

```typescript
// hooks/useNotifications.ts
export function useNotifications() {
  const { data: session } = useSession();
  const [connection, setConnection] = useState<NotificationConnection | null>(null);
  
  useEffect(() => {
    if (session?.accessToken) {
      const notificationConnection = new NotificationConnection(session.accessToken);
      notificationConnection.start();
      setConnection(notificationConnection);
      
      return () => {
        notificationConnection.stop();
      };
    }
  }, [session?.accessToken]);
  
  return { connection };
}
```

### 4. **Redis Backplane pentru Scale-Out**

```csharp
// Program.cs
builder.Services.AddSignalR()
    .AddStackExchangeRedis(options =>
    {
        options.Configuration.EndPoints.Add("localhost:6379");
        options.Configuration.ChannelPrefix = "MindSpace.SignalR";
    });
```

**De ce Redis Backplane?**
- Multiple instanțe de server pot comunica între ele
- Mesajele sunt broadcast către toate serverele
- Utilizatorii pot fi conectați la servere diferite
- Scalabilitate orizontală

## Scenarii de Utilizare

### 📬 **Notificări în Timp Real**
- Like-uri la postări
- Comentarii noi
- Urmăritori noi
- Mențiuni în comentarii

### 💬 **Chat în Timp Real** (viitor)
- Mesaje directe între utilizatori
- Typing indicators
- Read receipts

### 📊 **Actualizări Live**
- Număr de vizualizări în timp real
- Trending posts
- Online users count

## Avantaje SignalR

✅ **WebSocket Native**: Comunicare full-duplex  
✅ **Fallback Automat**: Long polling dacă WebSocket nu e disponibil  
✅ **Reconnect Automat**: Exponential backoff  
✅ **Scale-Out**: Redis backplane pentru multiple servere  
✅ **Type-Safe**: TypeScript pe client, C# pe server  
✅ **Authentication**: JWT token integration  

## Performanță

- **Latență**: < 50ms pentru notificări
- **Throughput**: 10,000+ conexiuni concurente per server
- **Bandwidth**: ~1KB per notificare
- **Reconnect Time**: 0-30s cu exponential backoff

## Monitorizare

```csharp
// Metrici SignalR
- Conexiuni active
- Mesaje trimise/secundă
- Erori de conexiune
- Timp mediu de livrare
```
