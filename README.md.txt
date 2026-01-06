# 🤖 PR Review Agent (.NET + ChatGPT)

Automated Pull Request review agent for **.NET projects**, integrated with **GitHub Pull Requests**, using **ChatGPT** to validate code against **team-defined standards**.

This agent runs automatically whenever a Pull Request is created or updated and provides objective, consistent feedback directly in the PR.

---

## 🎯 Purpose

The goal of this agent is to:

- Enforce **team coding standards**
- Reduce manual review effort
- Improve architectural consistency
- Provide fast and objective feedback
- Act as a **quality gate** for Pull Requests

The agent **does not replace human code review** — it complements it.

---

## ⚙️ How It Works

1. A Pull Request is created or updated on GitHub
2. GitHub Actions is triggered automatically
3. The PR Review Agent runs as a .NET 8 console application
4. The agent:
   - Reads the validation criteria from a versioned Markdown file
   - Collects the Pull Request diff
   - Sends the context to ChatGPT
   - Receives a structured JSON response
   - (Optional) Posts feedback as a comment on the PR
   - (Optional) Fails the pipeline if critical violations are found

---

## 📁 Repository Structure

Recommended project structure:

.
├── src/ # Application source code
├── docs/
│ └── pr-standards.md # ✅ Team validation criteria
├── tools/
│ └── PrReview.Agent/ # 🤖 PR Review Agent (.NET 8)
│ ├── PrReview.Agent.csproj
│ └── Program.cs
├── .github/
│ └── workflows/
│ └── pr-review.yml # GitHub Action workflow
└── README.md


---

## 🧠 Validation Criteria (Standards)

All validation rules are defined in a **versioned Markdown file**:

