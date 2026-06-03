import { useEffect, useRef, useState } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { HubConnection } from '@microsoft/signalr';

interface Notification {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
  type: 'NewComment' | 'NewLike' | 'PostLiked' | 'NewFollower' | 'CommentLike' | 'CommentReply' | 'PostPublished';
}

export const useSignalR = () => {
  const { isAuthenticated, user } = useAuth();
  const connectionRef = useRef<HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [notifications, setNotifications] = useState<Notification[]>([]);

  useEffect(() => {
    if (!isAuthenticated || !user) return;

    const connectToSignalR = async () => {
      try {
        // Încărcați dinamic importurile necesare pentru conexiunea SignalR
        const { HubConnectionBuilder, LogLevel } = await import('@microsoft/signalr');
        
        // Obțineți tokenul JWT
        const token = localStorage.getItem('token');
        if (!token) return;

        // Conectați-vă la Hub
        const connection = new HubConnectionBuilder()
          .withUrl('https://localhost:7237/notificationHub', {
            accessTokenFactory: () => token
          })
          .configureLogging(LogLevel.Information)
          .withAutomaticReconnect()
          .build();

        // Ascultați evenimentele de conexiune
        connection.on('NewNotification', (notification) => {
          console.log('Notificare nouă primită:', notification);
          setNotifications(prev => [notification, ...prev]);
        });

        connection.on('NotificationRead', (notificationId) => {
          console.log('Notificare citită:', notificationId);
          setNotifications(prev => 
            prev.map(n => 
              n.id === notificationId ? { ...n, isRead: true } : n
            )
          );
        });

        connection.on('AllNotificationsRead', () => {
          console.log('Toate notificările au fost marcate ca citite');
          setNotifications(prev => 
            prev.map(n => ({ ...n, isRead: true }))
          );
        });

        // Inițiați conexiunea
        await connection.start();
        setIsConnected(true);
        connectionRef.current = connection;

        console.log('Conexiunea SignalR a fost realizată cu succes');
      } catch (error) {
        console.error('Eroare de conexiune SignalR:', error);
      }
    };

    connectToSignalR();

    // Cleanup
    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
        setIsConnected(false);
      }
    };
  }, [isAuthenticated, user]);

  const markAsRead = async (notificationId: string) => {
    if (connectionRef.current && isConnected) {
      try {
        await connectionRef.current.invoke('MarkNotificationAsRead', notificationId);
      } catch (error) {
        console.error('Eroare la marcarea notificării ca citită:', error);
      }
    }
  };

  return {
    isConnected,
    notifications,
    markAsRead
  };
}; 