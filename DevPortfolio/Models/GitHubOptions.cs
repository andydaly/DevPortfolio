namespace DevPortfolio.Models
{
    public sealed class GitHubOptions
    {
        public string? ProfileUrl { get; set; }
        public int PerPage { get; set; } = 100;
        public int RecentCount { get; set; } = 0;
        public string? Token { get; set; }
    }
}
