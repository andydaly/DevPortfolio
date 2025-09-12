using DevPortfolio.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace DevPortfolio.Services
{

    public interface IGitHubService
    {
        Task<IReadOnlyList<RepoDto>> GetReposAsync(CancellationToken ct = default);
        Task<IReadOnlyList<RepoDto>> GetRecentReposAsync(CancellationToken ct = default);
        Task<string?> GetReadmeHtmlAsync(string repoName, string defaultBranch, CancellationToken ct = default);
    }
    public sealed class GitHubService : IGitHubService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly GitHubOptions _opts;

        public GitHubService(IHttpClientFactory httpFactory, IOptions<GitHubOptions> opts)
        {
            _httpFactory = httpFactory;
            _opts = opts.Value;
        }

        public async Task<IReadOnlyList<RepoDto>> GetReposAsync(CancellationToken ct = default)
        {
            var username = ExtractUsername(_opts.ProfileUrl)
                ?? throw new InvalidOperationException("GitHub ProfileUrl is missing or invalid in configuration.");

            var client = _httpFactory.CreateClient("GitHub");
            if (!string.IsNullOrWhiteSpace(_opts.Token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _opts.Token);

            var endpoint = $"users/{username}/repos?sort=created&direction=desc&per_page={_opts.PerPage}";
            var repos = await client.GetFromJsonAsync<List<RepoDto>>(endpoint, ct) ?? new List<RepoDto>();

            return repos.Where(r => !r.Archived).ToList();
        }

        public async Task<IReadOnlyList<RepoDto>> GetRecentReposAsync(CancellationToken ct = default)
        {
            var all = await GetReposAsync(ct);
            var n = _opts.RecentCount;
            if (n > 0) all = all.Take(n).ToList();
            return all;
        }

        public async Task<string?> GetReadmeHtmlAsync(string repoName, string defaultBranch, CancellationToken ct = default)
        {
            var owner = ExtractUsername(_opts.ProfileUrl)
                ?? throw new InvalidOperationException("GitHub ProfileUrl is missing or invalid in configuration.");

            var client = _httpFactory.CreateClient("GitHub");

            using var req = new HttpRequestMessage(HttpMethod.Get, $"repos/{owner}/{repoName}/readme");
            req.Headers.Accept.Clear();
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3.html"));

            if (!string.IsNullOrWhiteSpace(_opts.Token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opts.Token);

            using var resp = await client.SendAsync(req, ct);
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                return "<em>No README found for this repository.</em>";
            resp.EnsureSuccessStatusCode();

            var html = await resp.Content.ReadAsStringAsync(ct);

            html = RewriteRelativeUrls(html, owner, repoName, defaultBranch);

            return string.IsNullOrWhiteSpace(html) ? "<em>README is empty.</em>" : html;
        }

        private static string? ExtractUsername(string? profileUrl)
        {
            if (string.IsNullOrWhiteSpace(profileUrl)) return null;

            try
            {
                var uri = new Uri(profileUrl.Trim());
                var segs = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
                return segs.Length >= 1 ? segs[0] : null;
            }
            catch
            {
                var parts = profileUrl.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
                return parts.LastOrDefault();
            }
        }

        private static string RewriteRelativeUrls(string html, string owner, string repo, string branch)
        {
            var imgPattern = "(?i)(<img[^>]+src=[\"'])(?!https?://|//|data:|#)([^\"'>]+)([\"'])";
            html = Regex.Replace(html, imgPattern, m =>
            {
                var before = m.Groups[1].Value;
                var path = m.Groups[2].Value;
                var after = m.Groups[3].Value;
                var fixedPath = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{TrimRel(path)}";
                return $"{before}{fixedPath}{after}";
            });

            var aPattern = "(?i)(<a[^>]+href=[\"'])(?!https?://|//|mailto:|#)([^\"'>]+)([\"'])";
            html = Regex.Replace(html, aPattern, m =>
            {
                var before = m.Groups[1].Value;
                var path = m.Groups[2].Value;
                var after = m.Groups[3].Value;
                var fixedPath = $"https://github.com/{owner}/{repo}/blob/{branch}/{TrimRel(path)}";
                return $"{before}{fixedPath}{after}";
            });

            return html;
        }

        private static string TrimRel(string path)
            => path.Replace("\\", "/").TrimStart('.', '/');
    }
}
