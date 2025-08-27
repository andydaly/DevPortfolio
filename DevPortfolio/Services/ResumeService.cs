using DevPortfolio.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DevPortfolio.Services
{
    public interface IResumeService
    {
        Task<ResumeDto> ParseAsync(CancellationToken ct = default); 
        string? GetResumeDocxUrl();
    }

    public sealed class ResumeService : IResumeService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IMemoryCache _cache;

        private readonly string _configuredDocxPath;

        private const string MultipartFieldName = "file";
        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
        { PropertyNameCaseInsensitive = true };

        private static readonly Regex GoogleDocIdRx =
            new(@"docs\.google\.com\/document\/d\/([^\/\?\#]+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public ResumeService(IHttpClientFactory httpFactory, IOptions<ResumeSettings> opts, IMemoryCache cache)
        {
            _httpFactory = httpFactory;
            _cache = cache;
            _configuredDocxPath = opts.Value.DocxPath ?? throw new InvalidOperationException("Resume:DocxPath is missing.");
        }

        public string? GetResumeDocxUrl()
        {
            return BuildDocxDownloadUrl(_configuredDocxPath);
        }

        public Task<ResumeDto> ParseAsync(CancellationToken ct = default)
            => _cache.GetOrCreateAsync($"resume::{GetResumeDocxUrl()}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var downloadUrl = GetResumeDocxUrl()!;
                var g = _httpFactory.CreateClient("GoogleDocs");

                using var download = await g.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
                download.EnsureSuccessStatusCode();
                var bytes = await download.Content.ReadAsByteArrayAsync(ct);

                var api = _httpFactory.CreateClient("ResumeApi");
                using var form = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                form.Add(fileContent, MultipartFieldName, "resume.docx");

                using var resp = await api.PostAsync("parse", form, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await SafeReadAsync(resp.Content, ct);
                    throw new HttpRequestException($"Parser error {(int)resp.StatusCode}: {resp.StatusCode}. {body}");
                }

                var dto = await resp.Content.ReadFromJsonAsync<ResumeDto>(JsonOpts, ct);
                return dto ?? new ResumeDto();
            })!;

        private static async Task<string> SafeReadAsync(HttpContent c, CancellationToken ct)
        {
            try { return await c.ReadAsStringAsync(ct); }
            catch { return "<unreadable body>"; }
        }

        private static string BuildDocxDownloadUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;

            if (path.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                return path;

            if (path.Contains("/export?format=docx", StringComparison.OrdinalIgnoreCase))
                return path;

            var m = GoogleDocIdRx.Match(path);
            if (m.Success)
            {
                var id = m.Groups[1].Value;
                return $"https://docs.google.com/document/d/{id}/export?format=docx";
            }
            return path;
        }
    }
}
