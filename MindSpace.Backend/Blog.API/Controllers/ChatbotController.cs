using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Search;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChatbotController : ControllerBase
{
    private readonly IElasticsearchService _searchService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatbotController> _logger;
    private readonly HttpClient _httpClient;

    public ChatbotController(
        IElasticsearchService searchService,
        IConfiguration configuration,
        ILogger<ChatbotController> logger)
    {
        _searchService = searchService;
        _configuration = configuration;
        _logger = logger;
        _httpClient = new HttpClient();
        // Setați timeout scurt pentru a preveni blocarea în cazul în care local Ollama este oprit
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChatbotResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetBotResponse([FromBody] ChatbotRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Mesajul nu poate fi gol");
            }

            _logger.LogInformation("🤖 Mesaj primit de la utilizator: '{Message}'", request.Message);

            // Caută postări relevante folosind Elasticsearch (RAG)
            var searchRequest = new SearchRequest
            {
                Query = request.Message,
                Page = 1,
                PageSize = 3,
                HighlightResults = false
            };

            var searchResult = await _searchService.SearchPostsAsync(searchRequest);
            var relevantPosts = searchResult?.Results ?? new List<PostSearchResult>();

            var sources = relevantPosts.Select(p => new ChatbotSourceDto
            {
                Title = p.Title,
                Slug = p.Slug
            }).ToList();

            // Citim configurările AI
            var provider = _configuration["AiSettings:Provider"] ?? "Ollama";
            string prompt = BuildPrompt(request.Message, relevantPosts);

            if (provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
            {
                var ollamaUrl = _configuration["AiSettings:Ollama:Url"] ?? "http://localhost:11434";
                var ollamaModel = _configuration["AiSettings:Ollama:Model"] ?? "llama3";

                _logger.LogInformation("🤖 Se folosește provider-ul Ollama (Lokal LLM: {Model}) pe url {Url}", ollamaModel, ollamaUrl);

                try
                {
                    var ollamaRequest = new OllamaRequestDto
                    {
                        Model = ollamaModel,
                        Prompt = prompt,
                        Stream = false
                    };

                    var requestJson = JsonSerializer.Serialize(ollamaRequest);
                    var httpResponse = await _httpClient.PostAsync(
                        $"{ollamaUrl.TrimEnd('/')}/api/generate",
                        new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json")
                    );

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var responseJson = await httpResponse.Content.ReadAsStringAsync();
                        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponseDto>(responseJson);
                        string botResponse = ollamaResponse?.Response ?? "";

                        if (!string.IsNullOrWhiteSpace(botResponse))
                        {
                            return Ok(new ChatbotResponseDto
                            {
                                Message = botResponse.Trim(),
                                Sources = sources
                            });
                        }
                    }
                    else
                    {
                        _logger.LogError("❌ Ollama API a returnat codul de eroare: {StatusCode}", httpResponse.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Serviciul local Ollama nu a putut fi contactat. Se folosește modul fallback.");
                }

                // Fallback în caz că Ollama nu rulează sau eșuează
                string fallbackMessage = "Salut! Momentan **asistentul AI local (Ollama)** nu este pornit pe calculatorul tău sau modelul selectat nu este încărcat.\n\n" +
                                         "Pentru răspunsuri inteligente, te rugăm să te asiguri că Ollama rulează pe portul `11434`.\n\n" +
                                         "Între timp, am căutat în blogul MindSpace și am găsit următoarele articole care ar putea fi de interes:\n\n";

                if (sources.Any())
                {
                    foreach (var source in sources)
                    {
                        fallbackMessage += $"* **[{source.Title}](/post/{source.Slug})**\n";
                    }
                }
                else
                {
                    fallbackMessage += "Din păcate nu am găsit articole specifice pe această temă în blog. Încearcă să folosești alți termeni de căutare!";
                }

                return Ok(new ChatbotResponseDto
                {
                    Message = fallbackMessage,
                    Sources = sources
                });
            }
            else // Gemini Provider
            {
                var apiKey = _configuration["AiSettings:Gemini:ApiKey"];
                _logger.LogInformation("🤖 Se folosește provider-ul Gemini Cloud API");

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("⚠️ Gemini API Key nu este configurată. Se folosește fallback.");
                    
                    string fallbackMessage = "Salut! Momentan modulul de inteligență artificială **Gemini API** nu este configurat în setările serverului.\n\n" +
                                             "Pentru răspunsuri inteligente, te rugăm să adaugi cheia API în `appsettings.json`.\n\n" +
                                             "Între timp, am căutat în blogul MindSpace și am găsit următoarele articole:\n\n";

                    if (sources.Any())
                    {
                        foreach (var source in sources)
                        {
                            fallbackMessage += $"* **[{source.Title}](/post/{source.Slug})**\n";
                        }
                    }
                    else
                    {
                        fallbackMessage += "Nu am găsit articole similare în blog. Încearcă să folosești alte cuvinte cheie!";
                    }

                    return Ok(new ChatbotResponseDto
                    {
                        Message = fallbackMessage,
                        Sources = sources
                    });
                }

                var geminiRequest = new GeminiRequestDto
                {
                    Contents = new List<GeminiContentDto>
                    {
                        new GeminiContentDto
                        {
                            Parts = new List<GeminiPartDto>
                            {
                                new GeminiPartDto { Text = prompt }
                            }
                        }
                    }
                };

                var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
                var requestJson = JsonSerializer.Serialize(geminiRequest, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });

                var httpResponse = await _httpClient.PostAsync(
                    requestUrl, 
                    new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json")
                );

                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseJson = await httpResponse.Content.ReadAsStringAsync();
                    var geminiResponse = JsonSerializer.Deserialize<GeminiResponseDto>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    string botResponse = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "";
                    
                    if (!string.IsNullOrWhiteSpace(botResponse))
                    {
                        return Ok(new ChatbotResponseDto
                        {
                            Message = botResponse.Trim(),
                            Sources = sources
                        });
                    }
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Eroare la apelul Gemini API: {StatusCode} - {Error}", httpResponse.StatusCode, errorContent);
                }

                return StatusCode(500, "A apărut o eroare la comunicarea cu serviciul de AI cloud");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Eroare în timpul procesării chatbot-ului");
            return StatusCode(500, "A apărut o eroare internă la procesarea mesajului");
        }
    }

    private string BuildPrompt(string userMessage, List<PostSearchResult> relevantPosts)
    {
        string prompt = "Ești MindSpace AI, un asistent virtual inteligent pentru platforma de bloguri MindSpace.\n" +
                        "ATENȚIE: Răspunde strict și exclusiv în limba română. Este complet interzis să folosești limba turcă, engleză sau orice altă limbă.\n" +
                        "Răspunde întotdeauna într-un mod prietenos, profesionist și destul de concis (maximum 2-3 paragrafe).\n" +
                        "Folosește formatare Markdown (bold, liste, titluri, blocuri de cod dacă este cazul) pentru ca textul să fie ușor de citit.\n\n" +
                        "Mai jos este o listă de articole din blogul nostru care au legătură cu întrebarea utilizatorului. " +
                        "Folosește aceste informații pentru a răspunde. Dacă informația nu se află în articole, poți răspunde din cunoștințele tale generale, " +
                        "dar prioritizează conținutul blogului și menționează că informațiile sunt inspirate din articolele noastre.\n\n" +
                        "--- ARTICOLE RELEVANTE DIN BLOG ---\n";

        if (relevantPosts.Any())
        {
            for (int i = 0; i < relevantPosts.Count; i++)
            {
                var post = relevantPosts[i];
                prompt += $"Articolul #{i + 1}:\n" +
                          $"Titlu: {post.Title}\n" +
                          $"Categorie: {post.CategoryName}\n" +
                          $"Fragmente/Conținut: {post.Excerpt ?? post.Content}\n" +
                          $"Link: /post/{post.Slug}\n\n";
            }
        }
        else
        {
            prompt += "Nu s-au găsit articole specifice în blog pe această temă. Răspunde politicos folosind cunoștințele tale generale.\n\n";
        }

        prompt += "---------------------------------\n\n" +
                  $"Întrebarea utilizatorului: {userMessage}\n" +
                  "Răspuns (în limba română):";

        return prompt;
    }
}

public class ChatbotRequestDto
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class ChatbotResponseDto
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("sources")]
    public List<ChatbotSourceDto> Sources { get; set; } = new();
}

public class ChatbotSourceDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;
}

// DTOs specifice pentru Ollama API
public class OllamaRequestDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;
}

public class OllamaResponseDto
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}

// DTOs specifice pentru Gemini API
public class GeminiRequestDto
{
    public List<GeminiContentDto> Contents { get; set; } = new();
}

public class GeminiContentDto
{
    public List<GeminiPartDto> Parts { get; set; } = new();
}

public class GeminiPartDto
{
    public string Text { get; set; } = string.Empty;
}

public class GeminiResponseDto
{
    public List<GeminiCandidateDto> Candidates { get; set; } = new();
}

public class GeminiCandidateDto
{
    public GeminiContentDto Content { get; set; } = new();
}
