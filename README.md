# 🤖 PR Review Agent (.NET + OpenAI + GitHub)

An **automated Pull Request review agent** built with **.NET 8**, **GitHub Actions**, and **OpenAI**, designed to validate Pull Requests against **team-defined standards** and act as a **quality gate** before merging into protected branches.

This project demonstrates how to apply **AI in real-world software engineering workflows**, focusing on **code quality, architecture consistency, and governance**.

---

## 🎯 Purpose

The PR Review Agent was created to solve common problems in development teams:

- Inconsistent code reviews
- Subjective feedback
- Architectural drift
- Lack of automated governance
- Over-reliance on senior engineers for basic validations

The agent **does not replace human reviewers**.  
Instead, it provides **objective, repeatable, and transparent validation** before a PR is merged.

---

## ⚙️ How the Agent Works

1. A Pull Request is created or updated on GitHub
2. GitHub Actions is triggered automatically
3. The agent:
   - Reads validation criteria defined by the team
   - Collects the real PR diff from GitHub
   - Sends the context to OpenAI
   - Receives a structured JSON review
   - Posts a comment directly on the PR
   - (Optional) Fails the pipeline if critical violations are found
4. The PR can only be merged if:
   - The agent passes
   - A human approval is given

---

## ✨ Key Features

- ✅ **Automatic PR execution**
- ✅ **Standards-as-code** (criteria versioned in Git)
- ✅ **Real diff analysis** (not metadata-only)
- ✅ **Structured AI output (JSON)**
- ✅ **Automatic PR comments**
- ✅ **Merge blocking on critical issues**
- ✅ **No manual triggers**
- ✅ **Zero changes to production code**

---

## 📁 Recommended Structure (Plug-and-Play)

The PR Review Agent was designed to be **plug-and-play**.

To use it in any project, you only need to **copy the `.github`, `docs`, and `tools` folders into the repository root** and **adjust the validation criteria written in Markdown**.

No additional code changes or configuration are required.

---

### How to use

1. Clone or copy the `https://github.com/caiqueves/pr-review-agent` folder
2. Paste it into the **root of your target project**
3. Create or update the validation rules at:


## 📁 Repository Structure

Recommended structure for using the agent inside a project:

.
├── pr-review-agent/ # Application source code
├── docs/
│ └── pr-standards.md # ✅ Validation criteria (standards-as-code)
├── tools/
│ └── PrReview.Agent/ # 🤖 PR Review Agent (.NET 8)
│ ├── PrReview.Agent.csproj
│ └── Program.cs
├── .github/
│ └── workflows/
│ └── pr-review.yml # GitHub Actions workflow
└── README.md
.

---

## 🧠 Validation Criteria (Standards)

All validation rules are defined in a **Markdown file**, fully versioned and auditable.

📍 **pr-review-agent\docs\pr-standards.md**

### Example

```md
# PR Standards (.NET)

## Architecture
- Controllers must be thin
- No business logic in controllers
- Domain must not depend on Infrastructure

## Async & Performance
- All I/O must be async
- Forbidden: .Result, .Wait()
- Public async methods must receive CancellationToken

## Code Quality
- Methods should not exceed 50 lines
- No Console.WriteLine (use ILogger)
```

## 🔐 OpenAI API Key (Required)

The agent uses the **OpenAI API** to analyze Pull Requests.

⚠️ **ChatGPT Plus is NOT the same as the OpenAI API.**  
To run this agent, you must generate an **OpenAI API Key**.

---

### 🔑 Generate an OpenAI API Key

1. Access the OpenAI platform:  
   👉 https://platform.openai.com/api-keys
2. Log in with your OpenAI account
3. Click **Create new secret key**
4. Copy the generated key (it starts with `sk-...`)

⚠️ The key is shown **only once**. Store it securely.

---

## 🔒 Configure OpenAI Key in GitHub

The API key must be stored securely using **GitHub Secrets**.

### Steps

1. Open your GitHub repository
2. Go to **Settings**
3. Navigate to:

4. Click **New repository secret**
5. Add the following:

| Field  | Value |
|------|------|
| Name | `OPENAI_API_KEY` |
| Secret | `sk-xxxxxxxxxxxxxxxx` |

❌ **Never commit API keys to the repository.**

---

## 🚀 GitHub Actions Configuration

The agent runs automatically using **GitHub Actions** whenever a Pull Request is created or updated.

### 📍 Workflow location

**pr-review-agent\docs\pr-standards.md**

## 🔑 Environment Variables Used

| Variable | Description |
|--------|------------|
| `OPENAI_API_KEY` | OpenAI API key (configured as GitHub Secret) |
| `GITHUB_TOKEN` | Automatically provided by GitHub |
| `GITHUB_EVENT_PATH` | Path to the Pull Request event payload |
| `GITHUB_REPOSITORY` | Repository identifier (`owner/repo`) |

---

## 🧪 Local Development (Optional)

You can run the agent locally by setting environment variables manually.

### Windows (PowerShell)

```powershell
$env:OPENAI_API_KEY="sk-xxx"
$env:GITHUB_TOKEN="ghp_xxx"
$env:GITHUB_REPOSITORY="owner/repo"
$env:GITHUB_EVENT_PATH="event.json"

dotnet run --project tools/PrReview.Agent
```

## 🔐 Merge Control & Governance

When combined with **Branch Protection Rules**, the agent becomes a **merge gate**.

### Recommended configuration for the `main` branch

- Require Pull Request
- Require status checks to pass
- Require **PR Review Agent**
- Require manual approval
- Disable bypass

This ensures **no code reaches `main` without validation and authorization**.

---

## 🧩 Extensibility

This agent was designed to be extensible and adaptable:

- Different standards per folder
- Severity-based policies
- Comment idempotency
- Metrics and dashboards
- Support for other languages

**Contributions are welcome.**

---

## 🏁 Final Notes

This project demonstrates **real-world AI applied to software engineering**, not demos or toy examples.

If you are interested in:

- AI agents
- .NET architecture
- DevOps automation
- Code governance

Feel free to explore, fork, and improve.

---

## 📄 License

MIT (or adjust as needed)


