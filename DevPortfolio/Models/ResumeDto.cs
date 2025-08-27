using System.Text.Json.Serialization;

namespace DevPortfolio.Models
{
    public sealed class ResumeDto
    {
        [JsonPropertyName("candidate")]
        public Candidate Candidate { get; set; } = new();

        [JsonPropertyName("education")]
        public List<EducationItem> Education { get; set; } = new();

        [JsonPropertyName("experience")]
        public List<ExperienceItem> Experience { get; set; } = new();

        [JsonPropertyName("skills")]
        public List<string> Skills { get; set; } = new();

        [JsonPropertyName("profile")]
        public string? Profile { get; set; }

        [JsonPropertyName("skills_profile")]
        public string? SkillsProfile { get; set; }

        [JsonPropertyName("achievements")]
        public List<string> Achievements { get; set; } = new();

        [JsonPropertyName("raw_text")]
        public string? RawText { get; set; }
    }

    public sealed class Candidate
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("email")] public string? Email { get; set; }
        [JsonPropertyName("phone")] public string? Phone { get; set; }
        [JsonPropertyName("phone_other")] public string? PhoneOther { get; set; }
        [JsonPropertyName("github_url")] public string? GithubUrl { get; set; }
        [JsonPropertyName("linkedin_url")] public string? LinkedinUrl { get; set; }
    }

    public sealed class EducationItem
    {
        [JsonPropertyName("graduation_date")] public string? GraduationDate { get; set; }
        [JsonPropertyName("course")] public string? Course { get; set; }
        [JsonPropertyName("result")] public string? Result { get; set; }
        [JsonPropertyName("institution")] public string? Institution { get; set; }
    }

    public sealed class ExperienceItem
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("company")] public string? Company { get; set; }
        [JsonPropertyName("start_date")] public string? StartDate { get; set; }
        [JsonPropertyName("end_date")] public string? EndDate { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
    }
}
