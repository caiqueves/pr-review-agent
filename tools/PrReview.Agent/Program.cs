using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

// =====================
// ENV VALIDATION
// =====================
var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new Exception("OPENAI_API_KEY not found");

var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new Exception("GITHUB_TOKEN not found");

// =====================
// READ STANDARDS
// =====================
var standards = File.ReadAllText("pr-review-agent/docs/pr-standards.md");

// =====================
// READ PR NUMBER FROM EVENT
// =====================
var eventPath = Environment.GetEnvironmentVariable("GITHUB_EVENT_PATH")
    ?? throw new Exception("GITHUB_EVENT_PATH not found");

var eventJson = await File.ReadAllTextAsync(eventPath);
using var eventDoc = JsonDocument.Parse(eventJson);

var prNumber = eventDoc.RootElement
    .GetProperty("pull_request")
    .GetProperty("number")
    .GetInt32();

// =====================
// READ REPOSITORY
// =====================
var repo = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY")
    ?? throw new Exception("GITHUB_REPOSITORY not found");

// =====================
// HTTP CLIENT (GitHub)
// =====================
var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.ParseAdd("pr-review-agent");
http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", githubToken);

// Request diff
http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v3.diff");

var diffUrl = $"https://api.github.com/repos/{repo}/pulls/{prNumber}";
var diff = await http.GetStringAsync(diffUrl);

Console.WriteLine($"PR #{prNumber} diff length: {diff.Length}");

// =====================
// BUILD PROMPT
// =====================
var jsonSchema = """
{
  "summary": "",
  "violations": [
    {
      "file": "",
      "rule": "",
      "severity": "",
      "reason": "",
      "suggestion": ""
    }
  ]
}
""";

var prompt = $"""
You are a senior .NET software architect performing a Pull Request review.

Team standards:
{standards}

Pull Request diff:
{diff}

Tasks:
- Validate the PR against the standards
- Identify violations
- Classify severity as Low, Medium, or High
- Suggest concrete improvements

Respond ONLY in JSON using this schema:
{jsonSchema}
""";

// =====================
// CALL OPENAI
// =====================
var openAiRequest = new
{
    model = "gpt-4.1",
    messages = new[]
    {
        new { role = "user", content = prompt }
    }
};

var openAiHttp = new HttpClient();
openAiHttp.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", openAiKey);

var aiRequest = new HttpRequestMessage(
    HttpMethod.Post,
    "https://api.openai.com/v1/chat/completions");

aiRequest.Content = new StringContent(
    JsonSerializer.Serialize(openAiRequest),
    Encoding.UTF8,
    "application/json");

var aiResponse = await openAiHttp.SendAsync(aiRequest);
aiResponse.EnsureSuccessStatusCode();

var aiContent = await aiResponse.Content.ReadAsStringAsync();

// =====================
// PARSE OPENAI RESPONSE
// =====================
using var aiDoc = JsonDocument.Parse(aiContent);

var aiMessage = aiDoc.RootElement
    .GetProperty("choices")[0]
    .GetProperty("message")
    .GetProperty("content")
    .GetString()
    ?? throw new Exception("Empty AI response");

var cleanJson = ExtractPureJson(aiMessage);
using var reviewDoc = JsonDocument.Parse(cleanJson);


// =====================
// BUILD MARKDOWN COMMENT
// =====================
var summary = reviewDoc.RootElement.GetProperty("summary").GetString();
var violations = reviewDoc.RootElement.GetProperty("violations");

var sb = new StringBuilder();
sb.AppendLine("## 🤖 Automated PR Review");
sb.AppendLine();
sb.AppendLine($"**Summary:** {summary}");
sb.AppendLine();

bool hasHighSeverity = false;

if (violations.GetArrayLength() == 0)
{
    sb.AppendLine("✅ No violations found.");
}
else
{
    sb.AppendLine("### 🚨 Violations");
    sb.AppendLine();

    foreach (var v in violations.EnumerateArray())
    {
        var file = v.GetProperty("file").GetString();
        var rule = v.GetProperty("rule").GetString();
        var severity = v.GetProperty("severity").GetString();
        var reason = v.GetProperty("reason").GetString();
        var suggestion = v.GetProperty("suggestion").GetString();

        if (severity?.Equals("High", StringComparison.OrdinalIgnoreCase) == true)
            hasHighSeverity = true;

        sb.AppendLine($"- **{file}**");
        sb.AppendLine($"  - Rule: {rule}");
        sb.AppendLine($"  - Severity: **{severity}**");
        sb.AppendLine($"  - Reason: {reason}");
        sb.AppendLine($"  - Suggestion: {suggestion}");
        sb.AppendLine();
    }
}

var markdownComment = sb.ToString();

// =====================
// COMMENT ON PR
// =====================
var commentUrl = $"https://api.github.com/repos/{repo}/issues/{prNumber}/comments";

var commentPayload = new
{
    body = markdownComment
};

var commentContent = new StringContent(
    JsonSerializer.Serialize(commentPayload),
    Encoding.UTF8,
    "application/json");

var commentResponse = await http.PostAsync(commentUrl, commentContent);
commentResponse.EnsureSuccessStatusCode();

Console.WriteLine("PR comment posted successfully");

// =====================
// OPTIONAL: FAIL PIPELINE
// =====================
if (hasHighSeverity)
{
    Console.WriteLine("High severity violations found. Failing pipeline.");
    Environment.Exit(1);
}

static string ExtractPureJson(string content)
{
    if (string.IsNullOrWhiteSpace(content))
        throw new Exception("Empty AI response");

    content = content.Trim();

    // Remove markdown fences ```json ``` 
    if (content.StartsWith("```"))
    {
        content = content
            .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
            .Replace("```", "")
            .Trim();
    }

    // Extract only the JSON object
    var firstBrace = content.IndexOf('{');
    var lastBrace = content.LastIndexOf('}');

    if (firstBrace < 0 || lastBrace <= firstBrace)
        throw new Exception("Invalid JSON returned by AI");

    return content.Substring(firstBrace, lastBrace - firstBrace + 1);
}
