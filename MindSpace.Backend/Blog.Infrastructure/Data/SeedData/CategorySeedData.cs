using Blog.Domain.Entities;

namespace Blog.Infrastructure.Data.SeedData;

public static class CategorySeedData
{
    public static readonly Category[] Categories = new[]
    {
        // Tehnologie - 5 categorii
        new Category 
        { 
            Name = "Tehnologie", 
            Slug = "tehnologie",
            Description = "Cele mai recente tendințe tehnologice, inovații și dezvoltări din industrie",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/2570/2570579.png",
            Color = "#3B82F6",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Inteligență Artificială", 
            Slug = "inteligenta-artificiala",
            Description = "Tehnologii AI, machine learning și aplicații de deep learning",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/2103/2103633.png",
            Color = "#EC4899",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Blockchain", 
            Slug = "blockchain",
            Description = "Tehnologie blockchain, criptomonede și aplicații descentralizate",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/6001/6001368.png",
            Color = "#F7931A",
            PostCount = 0
        },
        new Category 
        { 
            Name = "IoT", 
            Slug = "iot",
            Description = "Internet of Things, dispozitive inteligente și sisteme conectate",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/3059/3059997.png",
            Color = "#10B981",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Tehnologie 5G", 
            Slug = "tehnologie-5g",
            Description = "Rețele 5G, tehnologii mobile și comunicații de generație următoare",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/3059/3059997.png",
            Color = "#8B5CF6",
            PostCount = 0
        },

        // Dezvoltare Software - 5 categorii
        new Category 
        { 
            Name = "Dezvoltare Software", 
            Slug = "dezvoltare-software",
            Description = "Limbaje de programare, arhitectură software, design patterns și bune practici",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1006/1006771.png",
            Color = "#10B981",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Cod Curat", 
            Slug = "cod-curat",
            Description = "Principii de scriere a codului curat, refactorizare și calitatea codului",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1006/1006771.png",
            Color = "#059669",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Design Patterns", 
            Slug = "design-patterns",
            Description = "Șabloane de proiectare software și exemple practice de aplicare",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1006/1006771.png",
            Color = "#DC2626",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Principii SOLID", 
            Slug = "principii-solid",
            Description = "Principii SOLID și design orientat pe obiecte",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1006/1006771.png",
            Color = "#7C3AED",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Test Driven Development", 
            Slug = "test-driven-development",
            Description = "Abordare TDD, unit testing și strategii de testare",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1006/1006771.png",
            Color = "#F59E0B",
            PostCount = 0
        },

        // Dezvoltare Web - 5 categorii
        new Category 
        { 
            Name = "Dezvoltare Web", 
            Slug = "dezvoltare-web",
            Description = "Tehnologii de dezvoltare web frontend și backend, framework-uri și instrumente",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1005/1005141.png",
            Color = "#F59E0B",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Dezvoltare Frontend", 
            Slug = "dezvoltare-frontend",
            Description = "HTML, CSS, JavaScript și framework-uri frontend moderne",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1005/1005141.png",
            Color = "#3B82F6",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Dezvoltare Backend", 
            Slug = "dezvoltare-backend",
            Description = "Programare server-side, dezvoltare API și management baze de date",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1005/1005141.png",
            Color = "#EF4444",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Dezvoltare Full Stack", 
            Slug = "dezvoltare-full-stack",
            Description = "Dezvoltare end-to-end a aplicațiilor web și deployment",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1005/1005141.png",
            Color = "#10B981",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Progressive Web Apps", 
            Slug = "progressive-web-apps",
            Description = "Tehnologii PWA și aplicații web moderne",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1005/1005141.png",
            Color = "#8B5CF6",
            PostCount = 0
        },

        // Dezvoltare Mobile - 5 categorii
        new Category 
        { 
            Name = "Dezvoltare Mobile", 
            Slug = "dezvoltare-mobile",
            Description = "Ghiduri de dezvoltare aplicații mobile iOS, Android și cross-platform",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/2991/2991110.png",
            Color = "#8B5CF6",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Dezvoltare iOS", 
            Slug = "dezvoltare-ios",
            Description = "Swift, SwiftUI și dezvoltare aplicații iOS",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/2991/2991110.png",
            Color = "#000000",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Dezvoltare Android", 
            Slug = "dezvoltare-android",
            Description = "Kotlin, Jetpack Compose și dezvoltare aplicații Android",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/2991/2991110.png",
            Color = "#3DDC84",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Dezvoltare Cross-Platform", 
            Slug = "dezvoltare-cross-platform",
            Description = "Dezvoltare cross-platform cu React Native, Flutter și Xamarin",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/2991/2991110.png",
            Color = "#02569B",
            PostCount = 0
        },
        new Category 
        { 
            Name = "UI/UX Mobile", 
            Slug = "ui-ux-mobile",
            Description = "Design interfață utilizator mobile și experiență utilizator",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/2991/2991110.png",
            Color = "#EC4899",
            PostCount = 0
        },

        // DevOps - 5 categorii
        new Category 
        { 
            Name = "DevOps", 
            Slug = "devops",
            Description = "Pipeline-uri CI/CD, containerizare, deployment și infrastructure as code",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/888/888839.png",
            Color = "#EF4444",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Integrare Continuă", 
            Slug = "integrare-continua",
            Description = "Pipeline-uri CI/CD, testare automată și procese de build",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/888/888839.png",
            Color = "#FF6B6B",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Containerizare", 
            Slug = "containerizare",
            Description = "Docker, Kubernetes și orchestrare containere",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/888/888839.png",
            Color = "#2496ED",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Infrastructură ca și Cod", 
            Slug = "infrastructura-ca-si-cod",
            Description = "Terraform, Ansible și automatizare infrastructură",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/888/888839.png",
            Color = "#7C3AED",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Monitorizare & Logging", 
            Slug = "monitorizare-logging",
            Description = "Monitorizare aplicații, logging și observabilitate",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/888/888839.png",
            Color = "#10B981",
            PostCount = 0
        },

        // Cloud Computing - 5 categorii
        new Category 
        { 
            Name = "Cloud Computing", 
            Slug = "cloud-computing",
            Description = "AWS, Azure, Google Cloud și pattern-uri de arhitectură cloud",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1067/1067356.png",
            Color = "#06B6D4",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Servicii AWS", 
            Slug = "servicii-aws",
            Description = "Amazon Web Services și soluții cloud computing",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1067/1067356.png",
            Color = "#FF9900",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Platformă Azure", 
            Slug = "platforma-azure",
            Description = "Microsoft Azure și soluții cloud enterprise",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1067/1067356.png",
            Color = "#0089D6",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Google Cloud", 
            Slug = "google-cloud",
            Description = "Google Cloud Platform și servicii cloud AI-first",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1067/1067356.png",
            Color = "#4285F4",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Serverless Computing", 
            Slug = "serverless-computing",
            Description = "Arhitectură serverless și function-as-a-service",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/1067/1067356.png",
            Color = "#10B981",
            PostCount = 0
        },

        // Baze de Date - 5 categorii
        new Category 
        { 
            Name = "Baze de Date", 
            Slug = "baze-de-date",
            Description = "SQL, NoSQL, design baze de date, optimizare și management",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/4248/4248443.png",
            Color = "#84CC16",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Baze de Date SQL", 
            Slug = "baze-de-date-sql",
            Description = "Baze de date relaționale, SQL și proprietăți ACID",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/4248/4248443.png",
            Color = "#E48E00",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Baze de Date NoSQL", 
            Slug = "baze-de-date-nosql",
            Description = "Baze de date non-relaționale și stocare distribuită a datelor",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/4248/4248443.png",
            Color = "#4DB33D",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Design Baze de Date", 
            Slug = "design-baze-de-date",
            Description = "Modelare baze de date, normalizare și design schemă",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/4248/4248443.png",
            Color = "#336791",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Performanță Baze de Date", 
            Slug = "performanta-baze-de-date",
            Description = "Optimizare query-uri, indexare și tuning performanță",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/4248/4248443.png",
            Color = "#DC2626",
            PostCount = 0
        },

        // Securitate - 5 categorii
        new Category 
        { 
            Name = "Securitate", 
            Slug = "securitate",
            Description = "Securitate cibernetică, securitate aplicații, testare de penetrare și bune practici",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/2092/2092663.png",
            Color = "#DC2626",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Securitate Aplicații", 
            Slug = "securitate-aplicatii",
            Description = "Securitate aplicații web, OWASP și codare sigură",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/2092/2092663.png",
            Color = "#EF4444",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Securitate Rețea", 
            Slug = "securitate-retea",
            Description = "Protecție rețea, firewall și detectare intruziuni",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/2092/2092663.png",
            Color = "#7C3AED",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Criptografie", 
            Slug = "criptografie",
            Description = "Criptare, hashing și protocoale criptografice",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/2092/2092663.png",
            Color = "#F59E0B",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Penetration Testing", 
            Slug = "penetration-testing",
            Description = "Hacking etic, evaluare vulnerabilități și testare securitate",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/2092/2092663.png",
            Color = "#10B981",
            PostCount = 0
        },

        // Carieră - 5 categorii
        new Category 
        { 
            Name = "Carieră", 
            Slug = "cariera",
            Description = "Dezvoltare carieră, sfaturi interviu, dezvoltare profesională și networking",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
            Color = "#059669",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Interviuri de Angajare", 
            Slug = "interviuri-angajare",
            Description = "Pregătire interviu, întrebări tehnice și strategii de interviu",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
            Color = "#DC2626",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Dezvoltare Competențe", 
            Slug = "dezvoltare-competente",
            Description = "Dezvoltare competențe software și căi de învățare",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
            Color = "#3B82F6",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Leadership", 
            Slug = "leadership",
            Description = "Leadership tehnologic, management echipă și competențe interpersonale",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
            Color = "#8B5CF6",
            PostCount = 0
        },
        new Category 
        { 
            Name = "Freelancing", 
            Slug = "freelancing",
            Description = "Dezvoltare software freelance și muncă la distanță",
            IconUrl = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
            Color = "#F59E0B",
            PostCount = 0
        }
    };
}
