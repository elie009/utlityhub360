using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace UtilityHub360.Services
{
    public interface IDocumentationSearchService
    {
        Task<List<DocumentChunk>> SearchDocumentationAsync(string query, int maxResults = 5);
        Task<string> GetDocumentationContextAsync(string query);
    }

    public class DocumentationSearchService : IDocumentationSearchService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<DocumentationSearchService> _logger;
        private readonly string _documentationPath;
        private const string CACHE_KEY = "documentation_index";
        private const int CACHE_DURATION_HOURS = 24;

        public DocumentationSearchService(
            IMemoryCache cache,
            ILogger<DocumentationSearchService> logger,
            IWebHostEnvironment environment)
        {
            _cache = cache;
            _logger = logger;
            _documentationPath = Path.Combine(environment.ContentRootPath, "Documentation");
        }

        public async Task<List<DocumentChunk>> SearchDocumentationAsync(string query, int maxResults = 5)
        {
            try
            {
                var index = await GetOrBuildDocumentationIndexAsync();
                var queryWords = ExtractKeywords(query);
                
                var scoredChunks = new List<(DocumentChunk chunk, double score)>();

                foreach (var chunk in index)
                {
                    var score = CalculateRelevanceScore(chunk, queryWords);
                    if (score > 0)
                    {
                        scoredChunks.Add((chunk, score));
                    }
                }

                return scoredChunks
                    .OrderByDescending(x => x.score)
                    .Take(maxResults)
                    .Select(x => x.chunk)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documentation");
                return new List<DocumentChunk>();
            }
        }

        public async Task<string> GetDocumentationContextAsync(string query)
        {
            var relevantChunks = await SearchDocumentationAsync(query, 3);
            
            if (!relevantChunks.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== RELEVANT DOCUMENTATION ===");
            
            foreach (var chunk in relevantChunks)
            {
                sb.AppendLine($"\n📄 From: {chunk.FileName} (Section: {chunk.Section})");
                sb.AppendLine($"{chunk.Content}");
                sb.AppendLine("---");
            }
            
            return sb.ToString();
        }

        private async Task<List<DocumentChunk>> GetOrBuildDocumentationIndexAsync()
        {
            if (_cache.TryGetValue(CACHE_KEY, out List<DocumentChunk>? cachedIndex) && cachedIndex != null)
            {
                return cachedIndex;
            }

            var index = await BuildDocumentationIndexAsync();
            _cache.Set(CACHE_KEY, index, TimeSpan.FromHours(CACHE_DURATION_HOURS));
            
            return index;
        }

        private async Task<List<DocumentChunk>> BuildDocumentationIndexAsync()
        {
            var chunks = new List<DocumentChunk>();

            try
            {
                if (!Directory.Exists(_documentationPath))
                {
                    _logger.LogWarning($"Documentation path not found: {_documentationPath}");
                    return chunks;
                }

                var mdFiles = Directory.GetFiles(_documentationPath, "*.md", SearchOption.AllDirectories);
                _logger.LogInformation($"Indexing {mdFiles.Length} documentation files...");

                foreach (var filePath in mdFiles)
                {
                    try
                    {
                        var content = await File.ReadAllTextAsync(filePath);
                        var fileChunks = ParseMarkdownFile(filePath, content);
                        chunks.AddRange(fileChunks);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error reading file: {filePath}");
                    }
                }

                _logger.LogInformation($"Successfully indexed {chunks.Count} documentation chunks");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building documentation index");
            }

            return chunks;
        }

        private List<DocumentChunk> ParseMarkdownFile(string filePath, string content)
        {
            var chunks = new List<DocumentChunk>();
            var fileName = Path.GetFileName(filePath);
            var relativePath = Path.GetRelativePath(_documentationPath, filePath);

            // Split by headers (# ## ### etc.)
            var sections = Regex.Split(content, @"(?=^#{1,6}\s+.+$)", RegexOptions.Multiline);

            foreach (var section in sections)
            {
                if (string.IsNullOrWhiteSpace(section))
                    continue;

                var lines = section.Split('\n');
                var header = lines.FirstOrDefault()?.Trim() ?? "Introduction";
                var sectionContent = string.Join("\n", lines.Skip(1)).Trim();

                if (sectionContent.Length > 50) // Only include substantial sections
                {
                    chunks.Add(new DocumentChunk
                    {
                        FileName = relativePath,
                        Section = CleanHeaderText(header),
                        Content = sectionContent.Length > 2000 
                            ? sectionContent.Substring(0, 2000) + "..." 
                            : sectionContent,
                        Keywords = ExtractKeywords(header + " " + sectionContent)
                    });
                }
            }

            return chunks;
        }

        private string CleanHeaderText(string header)
        {
            // Remove markdown header symbols (# ## ###)
            return Regex.Replace(header, @"^#+\s*", "").Trim();
        }

        private List<string> ExtractKeywords(string text)
        {
            // Remove markdown syntax and extract meaningful words
            var cleanText = Regex.Replace(text, @"[#*`\[\](){}]", " ");
            
            var words = Regex.Split(cleanText.ToLower(), @"\W+")
                .Where(w => w.Length > 3) // Filter short words
                .Where(w => !IsStopWord(w))
                .Distinct()
                .ToList();

            return words;
        }

        private double CalculateRelevanceScore(DocumentChunk chunk, List<string> queryWords)
        {
            double score = 0;

            foreach (var word in queryWords)
            {
                // Higher weight for keywords in section headers
                if (chunk.Section.ToLower().Contains(word))
                    score += 3;

                // Medium weight for keywords in file name
                if (chunk.FileName.ToLower().Contains(word))
                    score += 2;

                // Base weight for keywords in content
                if (chunk.Content.ToLower().Contains(word))
                    score += 1;

                // Exact keyword match
                if (chunk.Keywords.Contains(word))
                    score += 0.5;
            }

            return score;
        }

        private bool IsStopWord(string word)
        {
            var stopWords = new HashSet<string>
            {
                "the", "and", "for", "are", "but", "not", "you", "all", "can", "her", "was", "one",
                "our", "out", "this", "that", "with", "have", "from", "they", "been", "what", "which",
                "their", "said", "each", "she", "will", "there", "than", "when", "some", "them", "these"
            };

            return stopWords.Contains(word);
        }
    }

    public class DocumentChunk
    {
        public string FileName { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new();
    }
}

