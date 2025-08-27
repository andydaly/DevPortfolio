using System.Text.Json.Serialization;

namespace DevPortfolio.Models
{
    public sealed class RepoDto
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("html_url")] public string? HtmlUrl { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("stargazers_count")] public int Stars { get; set; }
        [JsonPropertyName("language")] public string? Language { get; set; }
        [JsonPropertyName("fork")] public bool Fork { get; set; }
        [JsonPropertyName("archived")] public bool Archived { get; set; }
        [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }
        [JsonPropertyName("default_branch")] public string? DefaultBranch { get; set; }
    }
}
