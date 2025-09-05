# DevPortfolio

A clean, responsive developer portfolio built with **.NET 8 Blazor** (interactive server components) that:

- Pulls your **resume** from Google Docs as a `.docx`, sends it to a local **Python ResumeParser REST API** ([andydaly/ResumeParser](https://github.com/andydaly/ResumeParser)), and renders the parsed data (profile, skills, work history, education).
- Shows your **latest GitHub projects** (with README rendering and images) and a compact **Projects** section on the home page.
- Includes **About Me**, **Work History**, **Education**, **Projects**, and **Contact** sections.
- Has a **Schedule a Meeting** button wired to Calendly.
- Designed to run smoothly in Visual Studio 2022.

---

## Features

- Home (Hero): avatar, name, role, CTA buttons (Resume/Calendly), quick links (LinkedIn/GitHub).
- About Me: parsed from the resume’s profile; smooth reveal on scroll.
- Work History and Education: parsed from resume; month/year dates with scroll-reveal.
- Projects:
  - `/projects` – vertical list with inline README rendering (images supported). Click the title/row to toggle README; an “Open in GitHub” button opens the repo.
  - `/githuboverview` – tiled overview of public repos.
  - Home page “Projects” strip shows `RecentCount` repos from configuration.
- Resume page (`/resume`): full resume, black “Download” button, email mailto link, social icons.
- Contact: name, back-to-top link, icons, parsed email and two phone numbers.
- Config-driven: values from `appsettings.json` or user secrets; sensitive data is not committed to Git.

---

## Tech Stack

- Frontend: .NET 8 **Blazor Web App** (Interactive Server), Bootstrap, light custom CSS.
- Backend: C# with `HttpClientFactory` to call Google Docs and the Python API.
- Resume parsing: external **Python REST API** ([andydaly/ResumeParser](https://github.com/andydaly/ResumeParser)) expecting a multipart `file` field and returning a normalized JSON DTO.
- GitHub: REST API (optional token for higher rate limits).

---

## Solution Structure

```
DevPortfolio/               # Blazor app (this repo)
  Pages/                    # Home, Resume, Projects, GitHubOverview, etc.
  Services/                 # ResumeService, GitHubService
  wwwroot/                  # Static assets (images, css, js)
  appsettings.*.json        # Local configuration (ignored in git)
ResumeParser/               # Separate Python project (run locally)
```

---

## Configuration

Create a local `appsettings.json` (do not commit). Example:

```json
{
  "Resume": {
    "DocxPath": "https://docs.google.com/document/d/<DOC_ID>/",
    "ApiBase": "http://127.0.0.1:8000/"
  },
  "Meetings": {
    "ScheduleUrl": "https://calendly.com/andrewdalyslash/15-minute-phone-call"
  },
  "GitHub": {
    "ProfileUrl": "https://github.com/andydaly",
    "PerPage": 100,
    "RecentCount": 4,
    "Token": ""
  },
  "Site": {
    "OwnerName": "Andrew Daly"
  }
}
```

Notes:

- `Resume:DocxPath` accepts either the Google Doc **ID** or the **document URL**. The app converts it to the correct `export?format=docx` download link.
- Keep secrets out of git. The repo’s `.gitignore` ignores `**/appsettings*.json`.
- Use **User Secrets** in Development (Visual Studio: Project → Manage User Secrets) or environment variables in production, for example:
  - `Meetings__ScheduleUrl`
  - `GitHub__Token`
  - `Resume__ApiBase`
 
## Running the App

### Prerequisites
- .NET 8 SDK
- Python 3.9+ for the ResumeParser API
- Visual Studio 2022 (or use the `dotnet` CLI)

### 1) Start the Python ResumeParser
Example with Uvicorn/FastAPI:

```bash
uvicorn main:app --host 127.0.0.1 --port 8000 --reload
```

The API must accept `POST /parse` with multipart field `file` and return a JSON DTO like:

```json
{
  "candidate": {
    "name": "string",
    "email": "string",
    "phone": "string",
    "phone_other": "string",
    "github_url": "string",
    "linkedin_url": "string"
  },
  "education": [
    { "graduation_date": "string", "course": "string", "result": "string", "institution": "string" }
  ],
  "experience": [
    { "title": "string", "company": "string", "start_date": "string", "end_date": "string", "description": "string" }
  ],
  "skills": ["string"],
  "profile": "string",
  "achievements": ["string"],
  "raw_text": "string"
}
```

### 2) Run the Blazor app
- Visual Studio 2022: F5
- CLI: `dotnet run` (from the `DevPortfolio` project directory)

Open in the browser:
- `/` – Home
- `/projects` – Projects list with inlined READMEs
- `/githuboverview` – Projects overview grid
- `/resume` – Resume

---

## Navigation

- Home – hero, About Me, Work History, Education, Projects, Contact
- Projects – vertical list with inline READMEs
- Projects Overview – grid of repos
- Resume – full parsed resume with download button and icons

---

## Notes and Tips

- README images: relative image paths in repo READMEs are rewritten so they display correctly.
- Dates: values like `YYYY-MM` (for example, `2019-11`) render as `Nov. 2019`. `present` renders as `Present`.
- Rate limits: set `GitHub:Token` to avoid unauthenticated GitHub API limits.
- Scroll-reveal: About/Work/Education entries fade in as they enter the viewport.

---

## Troubleshooting

- 422 (Unprocessable Entity) from the parser:
  - Ensure the multipart field name is `file`.
  - Confirm you can reach `Resume:ApiBase` (default `http://127.0.0.1:8000/`).
  - Make sure `Resume:DocxPath` is a valid Google Doc (ID or URL).

- Images not showing in README:
  - If images are referenced by relative paths, ensure the repo contains those assets and the repo is public (or implement authenticated access).

---

## Credits

- Blazor (.NET 8)
- Python ResumeParser (FastAPI/Flask) – **Python ResumeParser REST API** (https://github.com/andydaly/ResumeParser)
- GitHub REST API

---

## Contact

- LinkedIn: https://www.linkedin.com/in/andrew-daly-b5997394/
- GitHub: https://github.com/andydaly
- Schedule: https://calendly.com/andrewdalyslash/15-minute-phone-call
