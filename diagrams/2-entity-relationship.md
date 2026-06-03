# Entity Relationship Diagram (ERD)

```mermaid
erDiagram
    USERS ||--o{ POSTS : "writes"
    USERS ||--o{ COMMENTS : "writes"
    USERS ||--o{ LIKES : "creates"
    USERS ||--o{ NOTIFICATIONS : "receives"
    USERS ||--o{ BOOKMARKS : "saves"
    USERS ||--o{ USER_FOLLOWS : "follows"
    USERS ||--o{ USER_FOLLOWS : "followed_by"
    
    POSTS ||--o{ COMMENTS : "has"
    POSTS ||--o{ LIKES : "receives"
    POSTS ||--o{ POST_TAGS : "tagged_with"
    POSTS }o--|| CATEGORIES : "belongs_to"
    
    COMMENTS ||--o{ LIKES : "receives"
    COMMENTS ||--o{ COMMENTS : "replies_to"
    
    TAGS ||--o{ POST_TAGS : "used_in"
    
    USERS {
        string Id PK
        string UserName UK
        string Email UK
        string PasswordHash
        string FirstName
        string LastName
        string Bio
        string ProfileImageUrl
        int FollowerCount
        int FollowingCount
        datetime JoinDate
        bool IsVerified
    }
    
    POSTS {
        guid Id PK
        string Title
        string Slug UK
        text Content
        string Excerpt
        string FeaturedImageUrl
        enum Status
        datetime PublishedAt
        string AuthorId FK
        guid CategoryId FK
        int ViewCount
        int LikeCount
        int CommentCount
        int ReadTimeMinutes
        string MetaDescription
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    COMMENTS {
        guid Id PK
        text Content
        guid PostId FK
        string AuthorId FK
        guid ParentCommentId FK
        int LikeCount
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    LIKES {
        guid Id PK
        enum Type
        string UserId FK
        guid PostId FK
        guid CommentId FK
        datetime CreatedAt
    }
    
    CATEGORIES {
        guid Id PK
        string Name UK
        string Slug UK
        string Description
        string IconUrl
        string Color
        guid ParentCategoryId FK
        int PostCount
        datetime CreatedAt
    }
    
    TAGS {
        guid Id PK
        string Name UK
        string Slug UK
        string Description
        string Color
        int PostCount
        int FollowerCount
        datetime CreatedAt
    }
    
    POST_TAGS {
        guid PostId PK,FK
        guid TagId PK,FK
    }
    
    NOTIFICATIONS {
        guid Id PK
        string Title
        string Message
        enum Type
        bool IsRead
        datetime ReadAt
        string ActionUrl
        string ActionData
        string UserId FK
        string ActorId FK
        guid PostId FK
        guid CommentId FK
        datetime CreatedAt
    }
    
    USER_FOLLOWS {
        string FollowerId PK,FK
        string FollowingId PK,FK
        datetime CreatedAt
    }
    
    BOOKMARKS {
        guid Id PK
        string UserId FK
        guid PostId FK
        datetime CreatedAt
    }
```

**Açıklama:**
- **PK**: Primary Key (Birincil Anahtar)
- **FK**: Foreign Key (Yabancı Anahtar)
- **UK**: Unique Key (Benzersiz Anahtar)
- **||--o{**: One-to-Many ilişki (1:N)
- **}o--||**: Many-to-One ilişki (N:1)
- **||--||**: One-to-One ilişki (1:1)
