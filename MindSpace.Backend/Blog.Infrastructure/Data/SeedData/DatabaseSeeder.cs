using Blog.Domain.Entities;
using Blog.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blog.Infrastructure.Data.SeedData;

public class DatabaseSeeder
{
    private readonly BlogDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        BlogDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("🌱 Se începe popularea bazei de date...");

            // Ordinea de populare este importantă din cauza relațiilor de cheie străină
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedCategoriesAsync();
            await SeedTagsAsync();
            await SeedPostsAsync();
            await SeedCommentsAsync();
            await SeedLikesAsync();
            await SeedUserFollowsAsync();
            await SeedNotificationsAsync();

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Popularea bazei de date s-a finalizat cu succes!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ A apărut o eroare în timpul populării bazei de date");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Admin", "Author", "User" };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
                _logger.LogInformation("👑 Rol creat: {RoleName}", roleName);
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _userManager.Users.AnyAsync()) return;

        var users = new[]
        {
            new { User = new User 
            { 
                UserName = "admin", 
                Email = "admin@mindspace.com", 
                FirstName = "Admin", 
                LastName = "User",
                Bio = "Administrator de sistem",
                EmailConfirmed = true 
            }, Password = "Admin123!", Role = "Admin" },
            
            new { User = new User 
            { 
                UserName = "ion.popescu", 
                Email = "ion@example.com", 
                FirstName = "Ion", 
                LastName = "Popescu",
                Bio = "Dezvoltator software senior. Pasionat de .NET, React și cod curat.",
                EmailConfirmed = true 
            }, Password = "Ion12345!", Role = "Author" },
            
            new { User = new User 
            { 
                UserName = "maria.ionescu", 
                Email = "maria@example.com", 
                FirstName = "Maria", 
                LastName = "Ionescu",
                Bio = "Tech Lead. Îmi place să împărtășesc cunoștințe despre tehnologiile cloud.",
                EmailConfirmed = true 
            }, Password = "Maria123!", Role = "Author" },
            
            new { User = new User 
            { 
                UserName = "andrei.gheorghe", 
                Email = "andrei@example.com", 
                FirstName = "Andrei", 
                LastName = "Gheorghe",
                Bio = "Dezvoltator full-stack din România. Entuziast Angular și .NET.",
                EmailConfirmed = true 
            }, Password = "Andrei123!", Role = "Author" },
            
            new { User = new User 
            { 
                UserName = "elena.dumitrescu", 
                Email = "elena@example.com", 
                FirstName = "Elena", 
                LastName = "Dumitrescu",
                Bio = "Designer UX și dezvoltator frontend. Expert în React, Vue.js și sisteme de design.",
                EmailConfirmed = true 
            }, Password = "Elena123!", Role = "Author" },
            
            new { User = new User 
            { 
                UserName = "mihai.stanescu", 
                Email = "mihai@example.com", 
                FirstName = "Mihai", 
                LastName = "Stănescu",
                Bio = "Inginer DevOps. Specialist în Kubernetes, Docker și pipeline-uri CI/CD.",
                EmailConfirmed = true 
            }, Password = "Mihai123!", Role = "Author" },
            
            new { User = new User 
            { 
                UserName = "test.user", 
                Email = "test@example.com", 
                FirstName = "Test", 
                LastName = "Utilizator",
                Bio = "Un utilizator obișnuit care testează platforma.",
                EmailConfirmed = true 
            }, Password = "Test123!", Role = "User" }
        };

        foreach (var userData in users)
        {
            var result = await _userManager.CreateAsync(userData.User, userData.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(userData.User, userData.Role);
                _logger.LogInformation("👤 Utilizator creat: {UserName} cu rolul {Role}", userData.User.UserName, userData.Role);
            }
        }
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _context.Categories.AnyAsync()) return;

        _context.Categories.AddRange(CategorySeedData.Categories);
        await _context.SaveChangesAsync();
        _logger.LogInformation("📂 Au fost create {Count} categorii", CategorySeedData.Categories.Length);
    }

    private async Task SeedTagsAsync()
    {
        if (await _context.Tags.AnyAsync()) return;

        _context.Tags.AddRange(TagSeedData.Tags);
        await _context.SaveChangesAsync();
        _logger.LogInformation("🏷️ Au fost create {Count} etichete", TagSeedData.Tags.Length);
    }

    private async Task SeedPostsAsync()
    {
        if (await _context.Posts.AnyAsync()) return;

        var users = await _userManager.Users.Where(u => u.UserName != "admin").ToListAsync();
        var categories = await _context.Categories.ToListAsync();
        var tags = await _context.Tags.ToListAsync();

        var posts = new List<Post>();
        var postTags = new List<PostTag>();

        for (int i = 0; i < PostSeedData.PostTemplates.Length; i++)
        {
            var template = PostSeedData.PostTemplates[i];
            var author = users[i % users.Count];
            var category = categories.FirstOrDefault(c => c.Name == template.CategoryName);
            
            if (category == null)
            {
                _logger.LogWarning("Categorie negăsită: {CategoryName}", template.CategoryName);
                continue;
            }
            
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = template.Title,
                Content = template.Content,
                Excerpt = GenerateExcerpt(template.Content),
                Slug = GenerateSlug(template.Title),
                AuthorId = author.Id,
                CategoryId = category.Id,
                Status = PostStatus.Published,
                PublishedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)),
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(31, 60)),
                UpdatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 10)),
                ViewCount = Random.Shared.Next(50, 5000),
                ReadTimeMinutes = Random.Shared.Next(3, 15),
                MetaDescription = GenerateMetaDescription(template.Content),
                MetaKeywords = string.Join(", ", template.TagNames),
                FeaturedImageUrl = $"https://picsum.photos/800/400?random={i + 1}"
            };

            posts.Add(post);

            // Adaugă PostTags
            foreach (var tagName in template.TagNames)
            {
                var tag = tags.FirstOrDefault(t => t.Name == tagName);
                if (tag != null)
                {
                    postTags.Add(new PostTag
                    {
                        PostId = post.Id,
                        TagId = tag.Id
                    });
                }
            }
        }

        _context.Posts.AddRange(posts);
        _context.PostTags.AddRange(postTags);
        await _context.SaveChangesAsync();
        _logger.LogInformation("📝 Au fost create {PostCount} postări cu {TagCount} relații postare-etichetă", posts.Count, postTags.Count);
    }

    private async Task SeedCommentsAsync()
    {
        if (await _context.Comments.AnyAsync()) return;

        var posts = await _context.Posts.ToListAsync();
        var users = await _userManager.Users.ToListAsync();
        var comments = new List<Comment>();

        foreach (var post in posts.Take(10)) // Adaugă comentarii la primele 10 postări
        {
            // Comentarii principale
            for (int i = 0; i < Random.Shared.Next(2, 8); i++)
            {
                var user = users[Random.Shared.Next(users.Count)];
                var comment = new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = GetRandomCommentContent(),
                    PostId = post.Id,
                    AuthorId = user.Id,
                    CreatedAt = post.PublishedAt?.AddDays(Random.Shared.Next(1, 10)) ?? DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                comments.Add(comment);

                // Răspunsuri imbricate (50% șanse)
                if (Random.Shared.Next(0, 2) == 1)
                {
                    for (int j = 0; j < Random.Shared.Next(1, 3); j++)
                    {
                        var replyUser = users[Random.Shared.Next(users.Count)];
                        var reply = new Comment
                        {
                            Id = Guid.NewGuid(),
                            Content = GetRandomReplyContent(),
                            PostId = post.Id,
                            AuthorId = replyUser.Id,
                            ParentCommentId = comment.Id,
                            CreatedAt = comment.CreatedAt.AddMinutes(Random.Shared.Next(10, 1440)),
                            UpdatedAt = DateTime.UtcNow
                        };
                        comments.Add(reply);
                    }
                }
            }
        }

        _context.Comments.AddRange(comments);
        await _context.SaveChangesAsync();
        _logger.LogInformation("💬 Au fost create {Count} comentarii cu răspunsuri imbricate", comments.Count);
    }

    private async Task SeedLikesAsync()
    {
        if (await _context.Likes.AnyAsync()) return;

        var posts = await _context.Posts.ToListAsync();
        var comments = await _context.Comments.ToListAsync();
        var users = await _userManager.Users.ToListAsync();
        var likes = new List<Like>();

        // Aprecieri postări
        foreach (var post in posts)
        {
            var likeCount = Random.Shared.Next(1, 20);
            var randomUsers = users.OrderBy(x => Guid.NewGuid()).Take(likeCount);
            
            foreach (var user in randomUsers)
            {
                var likeTypes = new[] { LikeType.Like, LikeType.Love, LikeType.Clap };
                likes.Add(new Like
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    PostId = post.Id,
                    Type = likeTypes[Random.Shared.Next(likeTypes.Length)],
                    CreatedAt = post.PublishedAt?.AddDays(Random.Shared.Next(1, 10)) ?? DateTime.UtcNow
                });
            }
        }

        // Aprecieri comentarii
        foreach (var comment in comments.Take(50)) // Primele 50 de comentarii
        {
            var likeCount = Random.Shared.Next(0, 5);
            var randomUsers = users.OrderBy(x => Guid.NewGuid()).Take(likeCount);
            
            foreach (var user in randomUsers)
            {
                likes.Add(new Like
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    CommentId = comment.Id,
                    Type = LikeType.Like,
                    CreatedAt = comment.CreatedAt.AddMinutes(Random.Shared.Next(10, 1440))
                });
            }
        }

        _context.Likes.AddRange(likes);
        await _context.SaveChangesAsync();
        _logger.LogInformation("❤️ Au fost create {Count} aprecieri pentru postări și comentarii", likes.Count);
    }

    private async Task SeedUserFollowsAsync()
    {
        if (await _context.UserFollows.AnyAsync()) return;

        var users = await _userManager.Users.ToListAsync();
        var follows = new List<UserFollow>();

        foreach (var user in users)
        {
            // Fiecare utilizator urmărește 2-5 utilizatori aleatori
            var followCount = Random.Shared.Next(2, 6);
            var usersToFollow = users.Where(u => u.Id != user.Id)
                                   .OrderBy(x => Guid.NewGuid())
                                   .Take(followCount);

            foreach (var userToFollow in usersToFollow)
            {
                follows.Add(new UserFollow
                {
                    FollowerId = user.Id,
                    FollowingId = userToFollow.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 60))
                });
            }
        }

        _context.UserFollows.AddRange(follows);
        await _context.SaveChangesAsync();
        _logger.LogInformation("👥 Au fost create {Count} relații de urmărire între utilizatori", follows.Count);
    }

    private async Task SeedNotificationsAsync()
    {
        if (await _context.Notifications.AnyAsync()) return;

        var users = await _userManager.Users.ToListAsync();
        var posts = await _context.Posts.ToListAsync();
        var comments = await _context.Comments.ToListAsync();
        var notifications = new List<Notification>();

        foreach (var user in users.Take(5)) // Primii 5 utilizatori primesc notificări
        {
            // Notificări de apreciere postare
            for (int i = 0; i < Random.Shared.Next(3, 8); i++)
            {
                var post = posts[Random.Shared.Next(posts.Count)];
                var actor = users[Random.Shared.Next(users.Count)];
                
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Title = "Postare Apreciată",
                    Message = $"{actor.FirstName} {actor.LastName} a apreciat postarea dvs.: {post.Title}",
                    Type = NotificationType.PostLiked,
                    ActionUrl = $"/posts/{post.Slug}",
                    ActorId = actor.Id,
                    PostId = post.Id,
                    IsRead = Random.Shared.Next(0, 2) == 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 15))
                });
            }

            // Notificări de comentariu nou
            for (int i = 0; i < Random.Shared.Next(2, 5); i++)
            {
                var comment = comments[Random.Shared.Next(comments.Count)];
                var actor = users[Random.Shared.Next(users.Count)];
                
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Title = "Comentariu Nou",
                    Message = $"{actor.FirstName} {actor.LastName} a comentat la postarea dvs.",
                    Type = NotificationType.NewComment,
                    ActionUrl = $"/posts/{comment.PostId}#comment-{comment.Id}",
                    ActorId = actor.Id,
                    PostId = comment.PostId,
                    CommentId = comment.Id,
                    IsRead = Random.Shared.Next(0, 2) == 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 10))
                });
            }

            // Notificări de urmăritor nou
            for (int i = 0; i < Random.Shared.Next(1, 4); i++)
            {
                var follower = users[Random.Shared.Next(users.Count)];
                
                notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Title = "Urmăritor Nou",
                    Message = $"{follower.FirstName} {follower.LastName} a început să vă urmărească",
                    Type = NotificationType.NewFollower,
                    ActionUrl = $"/users/{follower.UserName}",
                    ActorId = follower.Id,
                    IsRead = Random.Shared.Next(0, 2) == 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 20))
                });
            }
        }

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();
        _logger.LogInformation("🔔 Au fost create {Count} notificări", notifications.Count);
    }

    #region Metode Ajutătoare

    private string GenerateSlug(string title)
    {
        return title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ă", "a")
            .Replace("â", "a")
            .Replace("î", "i")
            .Replace("ș", "s")
            .Replace("ş", "s")
            .Replace("ț", "t")
            .Replace("ţ", "t")
            .Replace(":", "")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("'", "")
            .Replace("\"", "");
    }

    private string GenerateExcerpt(string content)
    {
        return content.Length > 200 ? content.Substring(0, 200) + "..." : content;
    }

    private string GenerateMetaDescription(string content)
    {
        return content.Length > 150 ? content.Substring(0, 150) + "..." : content;
    }

    private string GetRandomCommentContent()
    {
        var comments = new[]
        {
            "Un articol excelent! Aș vrea să văd mai mult conținut pe această temă.",
            "Ați împărtășit informații foarte utile. Mulțumesc!",
            "Mă gândesc să folosesc această abordare și în proiectul meu. Soluții cu adevărat practice.",
            "Ați explicat subiectul foarte clar. Un ghid perfect pentru începători.",
            "Mulțumesc pentru împărtășirea experiențelor dvs. Informații foarte valoroase.",
            "După ce am citit acest articol, am înțeles subiectul mult mai bine.",
            "Exemplele de cod au fost cu adevărat de ajutor. Voi încerca imediat.",
            "Este frumos să vedem și abordări alternative. În ce situație ar trebui să le folosim?",
            "Această tehnologie este cu adevărat un game-changer. O voi folosi neapărat în proiectele mele.",
            "Mulțumesc pentru explicația detaliată. Am salvat articolul."
        };
        return comments[Random.Shared.Next(comments.Length)];
    }

    private string GetRandomReplyContent()
    {
        var replies = new[]
        {
            "Sunt de acord! Am experiențe similare în acest domeniu.",
            "Aveți dreptate, gândesc la fel.",
            "În acest punct, puteți încerca o abordare diferită.",
            "Mulțumesc, comentariul dvs. este foarte util.",
            "În acest caz, recomand abordarea X.",
            "Împărtășirea experienței dvs. este foarte valoroasă.",
            "Există mai multe informații detaliate despre acest subiect?",
            "Desigur, există și soluții alternative.",
            "Am întâmpinat probleme similare în proiectul meu.",
            "O observație absolut corectă."
        };
        return replies[Random.Shared.Next(replies.Length)];
    }

    // Metode de generare conținut
    private string GetDotNetContent() => @"
.NET 9, cea mai recentă versiune a platformei .NET de la Microsoft, aduce multe funcționalități noi și îmbunătățiri de performanță. În acest articol, vom analiza cele mai importante noutăți din .NET 9 și modul în care acestea oferă avantaje dezvoltatorilor.

## Îmbunătățiri de Performanță

.NET 9 conține îmbunătățiri semnificative de performanță comparativ cu versiunile anterioare:

- **Îmbunătățiri GC (Garbage Collector)**: Creștere de performanță de până la 20% cu noul algoritm de generare
- **Optimizări JIT Compiler**: Îmbunătățire de 15% a timpului de pornire
- **Alocare Memorie**: Amprentă de memorie redusă și alocare mai eficientă

## Funcționalități Noi

### 1. Minimal APIs Îmbunătățite
```csharp
app.MapGet(""/products/{id:int}"", async (int id, IProductService service) => 
{
    var product = await service.GetByIdAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});
```

### 2. Îmbunătățiri Native AOT
Suportul Native AOT a fost extins și acoperă mai multe scenarii.

### 3. Metode LINQ Noi
Cu noile metode de extensie LINQ, putem scrie cod mai expresiv.

## Ghid de Migrare

Migrarea de la .NET 8 la .NET 9 este un proces destul de lin. Modificările incompatibile au fost menținute la nivel minim.

Aceste dezvoltări consolidează și mai mult ecosistemul .NET și oferă o platformă excelentă pentru dezvoltarea aplicațiilor moderne.
";

    private string GetReactContent() => @"
Funcționalitățile Concurrent care vin cu React 18 aduc schimbări revoluționare în ceea ce privește performanța și experiența utilizatorului în aplicațiile React. În acest articol, vom analiza aceste funcționalități în detaliu.

## Ce este Concurrent Rendering?

Concurrent rendering este o tehnică care permite React să efectueze operațiuni de randare fără a bloca thread-ul principal. Datorită acesteia:

- Interfață de utilizator mai receptivă
- Prioritizare mai bună a actualizărilor
- Experiență de utilizator îmbunătățită

## Hook-uri Noi

### useTransition
```jsx
import { useTransition, useState } from 'react';

function SearchBox() {
  const [isPending, startTransition] = useTransition();
  const [query, setQuery] = useState('');
  const [results, setResults] = useState([]);

  const handleChange = (e) => {
    setQuery(e.target.value);
    startTransition(() => {
      // Actualizare non-urgentă
      setResults(searchFunction(e.target.value));
    });
  };

  return (
    <div>
      <input value={query} onChange={handleChange} />
      {isPending && <div>Se caută...</div>}
      <SearchResults results={results} />
    </div>
  );
}
```

### useDeferredValue
Cu acest hook putem amâna calculele costisitoare.

## Îmbunătățiri Suspense

În React 18, Suspense a devenit mai puternic și s-a integrat mai bine cu randarea pe server.

## Bune Practici pentru Performanță

1. Utilizați funcționalitățile concurrent cu precauție
2. Profilați aplicațiile dvs.
3. Optimizați randarea componentelor

Aceste funcționalități fac React indispensabil pentru dezvoltarea web modernă.
";

    private string GetMicroservicesContent() => @"
Arhitectura microservicii a devenit un standard pentru aplicațiile de întreprindere în zilele noastre. În acest articol, vom vedea cum să implementăm microservicii folosind .NET și Docker.

## Ce sunt Microserviciile?

Microserviciile sunt un șablon arhitectural care împarte aplicațiile mari în servicii mici și independente.

### Avantaje:
- Implementare independentă
- Diversitate tehnologică
- Izolare a defecțiunilor
- Scalabilitate

### Dezavantaje:
- Complexitatea sistemelor distribuite
- Latența rețelei
- Provocări de consistență a datelor

## Implementare cu .NET

### Șablonul API Gateway
```csharp
// Configurare Ocelot
{
  ""Routes"": [
    {
      ""DownstreamPathTemplate"": ""/api/products/{everything}"",
      ""DownstreamScheme"": ""http"",
      ""DownstreamHostAndPorts"": [
        {
          ""Host"": ""product-service"",
          ""Port"": 80
        }
      ],
      ""UpstreamPathTemplate"": ""/products/{everything}""
    }
  ]
}
```

### Descoperirea Serviciilor
Putem implementa descoperirea serviciilor folosind Consul sau Eureka.

### Șablonul Circuit Breaker
```csharp
services.AddHttpClient<IProductService, ProductService>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
```

## Containerizare cu Docker

Fiecare microserviciu rulează în propriul container:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY [""ProductService.csproj"", "".""]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [""dotnet"", ""ProductService.dll""]
```

## Orchestrare cu Docker Compose

```yaml
version: '3.8'
services:
  api-gateway:
    build: ./ApiGateway
    ports:
      - ""5000:80""
    depends_on:
      - product-service
      - user-service
  
  product-service:
    build: ./ProductService
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=Products;
  
  user-service:
    build: ./UserService
```

Cu arhitectura microservicii, putem construi sisteme scalabile și ușor de întreținut.
";

    // Alte metode de generare conținut
    private string GetTypeScriptContent() => "TypeScript aduce tipuri statice în JavaScript, oferind o experiență de dezvoltare mai sigură și mai productivă. Tipurile avansate precum mapped types, conditional types și template literal types permit crearea de cod mai expresiv și mai robust.";
    private string GetElasticsearchContent() => "Elasticsearch este un motor de căutare și analiză distribuit, bazat pe Apache Lucene. Permite indexarea și căutarea rapidă a datelor, fiind ideal pentru aplicații care necesită căutare full-text, analiză de jurnale și monitorizare în timp real.";
    private string GetSignalRContent() => "SignalR este o bibliotecă pentru comunicare în timp real în aplicațiile ASP.NET Core. Permite serverului să trimită actualizări instant către clienți folosind WebSockets, Server-Sent Events sau long polling, în funcție de capabilitățile clientului.";
    private string GetRabbitMQContent() => "RabbitMQ este un broker de mesaje open source care implementează protocolul AMQP. Facilitează comunicarea asincronă între microservicii prin cozi de mesaje, exchange-uri și rutare flexibilă a mesajelor.";
    private string GetCleanArchContent() => "Arhitectura Curată (Clean Architecture) organizează codul în straturi concentrice cu dependențe care pointează spre interior. Domeniul de afaceri este în centru, independent de framework-uri, baze de date și interfețe externe.";
    private string GetReactPerfContent() => "Optimizarea performanței în React implică tehnici precum memoizarea componentelor cu React.memo, utilizarea hook-urilor useMemo și useCallback, virtualizarea listelor lungi și împărțirea codului cu React.lazy și Suspense.";
    private string GetDevOpsContent() => "Azure DevOps oferă un set complet de instrumente pentru CI/CD, incluzând Azure Pipelines pentru build și deployment, Azure Repos pentru controlul versiunilor și Azure Boards pentru managementul proiectelor agile.";
    private string GetEFContent() => "Entity Framework Core este un ORM modern pentru .NET care permite dezvoltatorilor să lucreze cu bazele de date folosind obiecte .NET. Suportă migrări, interogări LINQ și multiple furnizori de baze de date.";
    private string GetJWTContent() => "Autentificarea cu JWT (JSON Web Token) oferă o modalitate sigură și stateless de a verifica identitatea utilizatorilor. Token-urile sunt semnate digital și pot conține informații despre utilizator și permisiunile acestuia.";
    private string GetFrameworkContent() => "Comparația framework-urilor web moderne include React, Angular, Vue.js și Svelte. Fiecare framework are avantaje și dezavantaje specifice în ceea ce privește performanța, curba de învățare și ecosistemul.";
    private string GetDockerSecContent() => "Securitatea Docker implică practici precum utilizarea imaginilor de bază oficiale, scanarea vulnerabilităților, rularea containerelor ca utilizator non-root și limitarea resurselor disponibile containerelor.";
    private string GetGraphQLContent() => "GraphQL este un limbaj de interogare pentru API-uri care permite clienților să solicite exact datele de care au nevoie. Reduce supraîncărcarea rețelei și oferă un sistem de tipuri puternic pentru validarea interogărilor.";
    private string GetMobileContent() => "Dezvoltarea mobilă modernă oferă multiple abordări: aplicații native cu Swift/Kotlin, aplicații cross-platform cu Flutter/React Native și aplicații web progresive (PWA) pentru acoperire maximă.";
    private string GetMLContent() => "Machine learning permite sistemelor să învețe din date și să facă predicții. Algoritmii de clasificare, regresie și clustering sunt utilizați în aplicații precum recunoașterea imaginilor, procesarea limbajului natural și sistemele de recomandare.";
    private string GetPostgreSQLContent() => "PostgreSQL este un sistem de management al bazelor de date relaționale open source, cunoscut pentru fiabilitatea, extensibilitatea și conformitatea cu standardele SQL. Suportă tipuri de date avansate, indexare și replicare.";
    private string GetK8sContent() => "Kubernetes automatizează deployment-ul, scalarea și managementul aplicațiilor containerizate. Oferă descoperirea serviciilor, echilibrarea încărcăturii, auto-vindecarea și actualizări progresive pentru aplicații distribuite.";
    private string GetCareerContent() => "Dezvoltarea carierei în tehnologie necesită învățare continuă, networking profesional și dezvoltarea competențelor soft. Participarea la comunități, contribuțiile open source și mentoratul sunt căi importante de creștere profesională.";

    #endregion
} 