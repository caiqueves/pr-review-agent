using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrEmpty(openAiKey))
{
    Console.WriteLine("OPENAI_API_KEY not found");
    return;
}

// 1. Read standards
var standards = File.ReadAllText("docs/pr-standards.md");

// 2. Read diff (simplificado – GitHub expõe via API)
var repo = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
var prNumber = Environment.GetEnvironmentVariable("GITHUB_REF_NAME")?.Replace("refs/pull/", "").Replace("/merge", "");

var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("pr-review-agent", "1.0"));
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
    Environment.GetEnvironmentVariable("GITHUB_TOKEN"));

var diff = await http.GetStringAsync(
    $"https://api.github.com/repos/{repo}/pulls/{prNumber}.diff");

// 3. Build prompt
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
You are a senior .NET code reviewer.

Team standards:
{standards}

Pull request diff:
{diff}

Check if the PR follows the standards.
List violations with severity (Low, Medium, High).
Respond ONLY in JSON:
{jsonSchema}
""";


// 4. Call OpenAI
var openAiRequest = new
{
    model = "gpt-4.1",
    messages = new[]
    {
        new { role = "user", content = prompt }
    }
};

var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", openAiKey);
request.Content = new StringContent(JsonSerializer.Serialize(openAiRequest), Encoding.UTF8, "application/json");

var response = await http.SendAsync(request);
var content = await response.Content.ReadAsStringAsync();

Console.WriteLine(content);
