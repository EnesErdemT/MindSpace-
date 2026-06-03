using Blog.Domain.Entities;

namespace Blog.Infrastructure.Data.SeedData;

public static class TagSeedData
{
    public static readonly Tag[] Tags = new[]
    {
        // Limbaje de Programare - 10 tag-uri
        new Tag { Name = "csharp", Slug = "csharp", Description = "Limbaj de programare modern de la Microsoft", Color = "#178600", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "dotnet", Slug = "dotnet", Description = "Platformă .NET și ecosistem", Color = "#512BD4", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "javascript", Slug = "javascript", Description = "Limbajul de programare al web-ului", Color = "#F7DF1E", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "typescript", Slug = "typescript", Description = "JavaScript cu tipuri statice", Color = "#3178C6", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "python", Slug = "python", Description = "Limbaj de programare versatil și puternic", Color = "#3776AB", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "java", Slug = "java", Description = "Limbaj de programare pentru aplicații enterprise", Color = "#ED8B00", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "go", Slug = "go", Description = "Limbaj de programare pentru sisteme de la Google", Color = "#00ADD8", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "rust", Slug = "rust", Description = "Limbaj de programare pentru sisteme sigure", Color = "#000000", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "php", Slug = "php", Description = "Limbaj de scripting pentru server", Color = "#777BB4", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "ruby", Slug = "ruby", Description = "Limbaj de programare dinamic și elegant", Color = "#CC342D", PostCount = 0, FollowerCount = 0 },

        // Framework-uri Web - 10 tag-uri
        new Tag { Name = "aspnetcore", Slug = "aspnetcore", Description = "Framework web cross-platform de la Microsoft", Color = "#512BD4", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "react", Slug = "react", Description = "Bibliotecă UI JavaScript de la Facebook", Color = "#61DAFB", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "angular", Slug = "angular", Description = "Framework web complet de la Google", Color = "#DD0031", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "vue", Slug = "vue", Description = "Framework JavaScript progresiv și flexibil", Color = "#4FC08D", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "nodejs", Slug = "nodejs", Description = "Mediu de execuție JavaScript pe server", Color = "#339933", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "express", Slug = "express", Description = "Framework web minimalist pentru Node.js", Color = "#000000", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "django", Slug = "django", Description = "Framework web Python de nivel înalt", Color = "#092E20", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "spring", Slug = "spring", Description = "Framework pentru aplicații Java enterprise", Color = "#6DB33F", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "laravel", Slug = "laravel", Description = "Framework web PHP elegant și expresiv", Color = "#FF2D20", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "nextjs", Slug = "nextjs", Description = "Framework React pentru aplicații de producție", Color = "#000000", PostCount = 0, FollowerCount = 0 },

        // Containere & Orchestrare - 5 tag-uri
        new Tag { Name = "docker", Slug = "docker", Description = "Platformă de containerizare pentru aplicații", Color = "#2496ED", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "kubernetes", Slug = "kubernetes", Description = "Sistem de orchestrare pentru containere", Color = "#326CE5", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "rancher", Slug = "rancher", Description = "Platformă de management pentru Kubernetes", Color = "#0075A8", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "helm", Slug = "helm", Description = "Manager de pachete pentru Kubernetes", Color = "#0DB9F0", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "openshift", Slug = "openshift", Description = "Platformă Kubernetes pentru întreprinderi", Color = "#EE0000", PostCount = 0, FollowerCount = 0 },

        // Platforme Cloud - 5 tag-uri
        new Tag { Name = "aws", Slug = "aws", Description = "Servicii cloud Amazon Web Services", Color = "#FF9900", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "azure", Slug = "azure", Description = "Platformă cloud Microsoft Azure", Color = "#0089D6", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "gcp", Slug = "gcp", Description = "Platformă cloud Google", Color = "#4285F4", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "digitalocean", Slug = "digitalocean", Description = "Furnizor de infrastructură cloud", Color = "#0080FF", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "heroku", Slug = "heroku", Description = "Platformă cloud pentru aplicații", Color = "#430098", PostCount = 0, FollowerCount = 0 },

        // Pattern-uri Arhitectură - 5 tag-uri
        new Tag { Name = "microservices", Slug = "microservices", Description = "Arhitectură bazată pe microservicii", Color = "#FF6B6B", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "monolith", Slug = "monolith", Description = "Arhitectură monolitică pentru aplicații", Color = "#4ECDC4", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "serverless", Slug = "serverless", Description = "Arhitectură fără servere", Color = "#10B981", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "event-driven", Slug = "event-driven", Description = "Arhitectură bazată pe evenimente", Color = "#8B5CF6", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "ddd", Slug = "ddd", Description = "Design bazat pe domeniu", Color = "#F59E0B", PostCount = 0, FollowerCount = 0 },

        // Tehnologii API - 5 tag-uri
        new Tag { Name = "api", Slug = "api", Description = "Interfață de programare a aplicațiilor", Color = "#4ECDC4", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "rest", Slug = "rest", Description = "Arhitectură REST pentru API-uri", Color = "#45B7D1", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "graphql", Slug = "graphql", Description = "Limbaj de interogare pentru API-uri", Color = "#E10098", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "grpc", Slug = "grpc", Description = "Framework RPC de înaltă performanță", Color = "#00ADD8", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "soap", Slug = "soap", Description = "Protocol pentru schimb de mesaje", Color = "#FF6600", PostCount = 0, FollowerCount = 0 },

        // Tehnologii Baze de Date - 10 tag-uri
        new Tag { Name = "database", Slug = "database", Description = "Sisteme de stocare și management al datelor", Color = "#336791", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "sql", Slug = "sql", Description = "Limbaj de interogare structurat", Color = "#E48E00", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "nosql", Slug = "nosql", Description = "Baze de date non-relaționale", Color = "#4DB33D", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "mongodb", Slug = "mongodb", Description = "Bază de date orientată pe documente", Color = "#47A248", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "postgresql", Slug = "postgresql", Description = "Bază de date relațională avansată", Color = "#336791", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "mysql", Slug = "mysql", Description = "Sistem de management baze de date", Color = "#4479A1", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "redis", Slug = "redis", Description = "Stocare de date în memorie", Color = "#DC382D", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "elasticsearch", Slug = "elasticsearch", Description = "Motor de căutare și analiză", Color = "#FED10A", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "cassandra", Slug = "cassandra", Description = "Bază de date distribuită NoSQL", Color = "#1287B1", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "dynamodb", Slug = "dynamodb", Description = "Bază de date NoSQL de la AWS", Color = "#4053D6", PostCount = 0, FollowerCount = 0 },

        // Message Brokers - 5 tag-uri
        new Tag { Name = "rabbitmq", Slug = "rabbitmq", Description = "Broker de mesaje pentru aplicații", Color = "#FF6600", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "kafka", Slug = "kafka", Description = "Platformă de streaming distribuit", Color = "#231F20", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "activemq", Slug = "activemq", Description = "Broker de mesaje Apache", Color = "#D93A35", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "nats", Slug = "nats", Description = "Sistem de mesagerie cloud native", Color = "#20BF6B", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "pulsar", Slug = "pulsar", Description = "Platformă de mesagerie Apache", Color = "#188FFF", PostCount = 0, FollowerCount = 0 },

        // Comunicare Real-time - 5 tag-uri
        new Tag { Name = "signalr", Slug = "signalr", Description = "Bibliotecă pentru comunicare în timp real", Color = "#512BD4", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "websockets", Slug = "websockets", Description = "Protocol de comunicare bidirecțională", Color = "#4ECDC4", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "socketio", Slug = "socketio", Description = "Comunicare în timp real pentru web", Color = "#010101", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "pusher", Slug = "pusher", Description = "Serviciu de mesagerie în timp real", Color = "#300D4F", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "firebase", Slug = "firebase", Description = "Platformă de dezvoltare mobilă Google", Color = "#FFCA28", PostCount = 0, FollowerCount = 0 },

        // AI & Machine Learning - 10 tag-uri
        new Tag { Name = "machinelearning", Slug = "machinelearning", Description = "Învățare automată și recunoaștere de pattern-uri", Color = "#FF6B6B", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "ai", Slug = "ai", Description = "Inteligență artificială și sisteme inteligente", Color = "#00D4FF", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "tensorflow", Slug = "tensorflow", Description = "Framework pentru machine learning", Color = "#FF6F00", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "pytorch", Slug = "pytorch", Description = "Framework pentru deep learning", Color = "#EE4C2C", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "scikit-learn", Slug = "scikit-learn", Description = "Bibliotecă Python pentru machine learning", Color = "#F7931E", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "deeplearning", Slug = "deeplearning", Description = "Învățare profundă cu rețele neuronale", Color = "#FF6B6B", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "nlp", Slug = "nlp", Description = "Procesare limbaj natural", Color = "#8B5CF6", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "computervision", Slug = "computervision", Description = "Viziune computerizată și procesare imagini", Color = "#F59E0B", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "chatgpt", Slug = "chatgpt", Description = "Model de limbaj conversațional AI", Color = "#10A37F", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "generativeai", Slug = "generativeai", Description = "Inteligență artificială generativă", Color = "#EC4899", PostCount = 0, FollowerCount = 0 },

        // Blockchain & Crypto - 5 tag-uri
        new Tag { Name = "blockchain", Slug = "blockchain", Description = "Tehnologie registru distribuit", Color = "#F7931A", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "ethereum", Slug = "ethereum", Description = "Platformă blockchain pentru contracte inteligente", Color = "#627EEA", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "bitcoin", Slug = "bitcoin", Description = "Criptomonedă descentralizată", Color = "#F7931A", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "solidity", Slug = "solidity", Description = "Limbaj pentru contracte inteligente", Color = "#363636", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "web3", Slug = "web3", Description = "Web descentralizat bazat pe blockchain", Color = "#F16822", PostCount = 0, FollowerCount = 0 },

        // Tehnologii Frontend - 10 tag-uri
        new Tag { Name = "frontend", Slug = "frontend", Description = "Dezvoltare interfață utilizator", Color = "#61DAFB", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "html", Slug = "html", Description = "Limbaj de marcare pentru web", Color = "#E34F26", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "css", Slug = "css", Description = "Foi de stil în cascadă", Color = "#1572B6", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "sass", Slug = "sass", Description = "Preprocesor CSS avansat", Color = "#CC6699", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "tailwindcss", Slug = "tailwindcss", Description = "Framework CSS utility-first", Color = "#06B6D4", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "bootstrap", Slug = "bootstrap", Description = "Framework CSS responsive", Color = "#7952B3", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "webpack", Slug = "webpack", Description = "Bundler de module pentru aplicații", Color = "#8DD6F9", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "vite", Slug = "vite", Description = "Tool de build rapid pentru frontend", Color = "#646CFF", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "redux", Slug = "redux", Description = "Container pentru management de stare", Color = "#764ABC", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "svelte", Slug = "svelte", Description = "Framework JavaScript compilat", Color = "#FF3E00", PostCount = 0, FollowerCount = 0 },

        // Dezvoltare Mobile - 5 tag-uri
        new Tag { Name = "mobile", Slug = "mobile", Description = "Dezvoltare aplicații pentru dispozitive mobile", Color = "#8B5CF6", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "ios", Slug = "ios", Description = "Sistem de operare mobil Apple", Color = "#000000", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "android", Slug = "android", Description = "Sistem de operare mobil Google", Color = "#3DDC84", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "flutter", Slug = "flutter", Description = "Framework UI cross-platform de la Google", Color = "#02569B", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "reactnative", Slug = "reactnative", Description = "Framework React pentru aplicații mobile", Color = "#61DAFB", PostCount = 0, FollowerCount = 0 },

        // Securitate & Testare - 10 tag-uri
        new Tag { Name = "security", Slug = "security", Description = "Securitate cibernetică și protecție", Color = "#DC2626", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "testing", Slug = "testing", Description = "Testare software și asigurare calitate", Color = "#10B981", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "junit", Slug = "junit", Description = "Framework de testare pentru Java", Color = "#25A162", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "jest", Slug = "jest", Description = "Framework de testare pentru JavaScript", Color = "#C21325", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "selenium", Slug = "selenium", Description = "Automatizare testare browser", Color = "#43B02A", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "cypress", Slug = "cypress", Description = "Framework de testare end-to-end", Color = "#17202C", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "owasp", Slug = "owasp", Description = "Securitate aplicații web", Color = "#000000", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "penetrationtesting", Slug = "penetrationtesting", Description = "Testare de penetrare și vulnerabilități", Color = "#DC2626", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "encryption", Slug = "encryption", Description = "Criptare și securitate date", Color = "#7C3AED", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "authentication", Slug = "authentication", Description = "Autentificare și autorizare utilizatori", Color = "#F59E0B", PostCount = 0, FollowerCount = 0 },

        // DevOps & CI/CD - 10 tag-uri
        new Tag { Name = "cicd", Slug = "cicd", Description = "Integrare și deployment continuu", Color = "#FF6B6B", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "jenkins", Slug = "jenkins", Description = "Server de automatizare open source", Color = "#D33833", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "gitlab", Slug = "gitlab", Description = "Platformă DevOps completă", Color = "#FC6D26", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "github", Slug = "github", Description = "Platformă de hosting pentru cod", Color = "#181717", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "terraform", Slug = "terraform", Description = "Infrastructură ca și cod", Color = "#7C3AED", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "ansible", Slug = "ansible", Description = "Automatizare IT și configurare", Color = "#EE0000", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "prometheus", Slug = "prometheus", Description = "Sistem de monitorizare și alertare", Color = "#E6522C", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "grafana", Slug = "grafana", Description = "Platformă de analiză și monitorizare", Color = "#F46800", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "nginx", Slug = "nginx", Description = "Server web și reverse proxy", Color = "#009639", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "linux", Slug = "linux", Description = "Sistem de operare open source", Color = "#FCC624", PostCount = 0, FollowerCount = 0 },

        // Agile & Management - 5 tag-uri
        new Tag { Name = "agile", Slug = "agile", Description = "Metodologie de dezvoltare agilă", Color = "#F59E0B", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "scrum", Slug = "scrum", Description = "Framework agile pentru echipe", Color = "#8B5CF6", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "kanban", Slug = "kanban", Description = "Sistem vizual de management", Color = "#00ADD8", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "jira", Slug = "jira", Description = "Instrument de management proiecte", Color = "#0052CC", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "confluence", Slug = "confluence", Description = "Platformă de colaborare pentru echipe", Color = "#172B4D", PostCount = 0, FollowerCount = 0 },

        // Carieră & Dezvoltare Profesională - 10 tag-uri
        new Tag { Name = "cariera", Slug = "cariera", Description = "Dezvoltare profesională", Color = "#059669", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "interviu", Slug = "interviu", Description = "Pregătire interviu de angajare", Color = "#DC2626", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "cv", Slug = "cv", Description = "Scriere și optimizare CV", Color = "#3B82F6", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "networking", Slug = "networking", Description = "Networking profesional", Color = "#8B5CF6", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "mentorship", Slug = "mentorship", Description = "Mentorat și îndrumare carieră", Color = "#F59E0B", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "leadership", Slug = "leadership", Description = "Leadership și management", Color = "#7C3AED", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "softskills", Slug = "softskills", Description = "Competențe interpersonale", Color = "#10B981", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "remotework", Slug = "remotework", Description = "Muncă la distanță", Color = "#EC4899", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "freelancing", Slug = "freelancing", Description = "Freelancing și consultanță", Color = "#F59E0B", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "productivity", Slug = "productivity", Description = "Productivitate și eficiență", Color = "#06B6D4", PostCount = 0, FollowerCount = 0 },

        // Învățare & Tutoriale - 5 tag-uri
        new Tag { Name = "tutorial", Slug = "tutorial", Description = "Ghiduri de învățare pas cu pas", Color = "#3B82F6", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "incepatori", Slug = "incepatori", Description = "Conținut pentru dezvoltatori începători", Color = "#10B981", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "avansat", Slug = "avansat", Description = "Concepte avansate de programare", Color = "#DC2626", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "bestpractices", Slug = "bestpractices", Description = "Best practices programare", Color = "#8B5CF6", PostCount = 0, FollowerCount = 0 },
        new Tag { Name = "codereview", Slug = "codereview", Description = "Tehnici de code review", Color = "#F59E0B", PostCount = 0, FollowerCount = 0 }
    };
}
