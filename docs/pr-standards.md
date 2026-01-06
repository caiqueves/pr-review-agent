# PR Standards (.NET)

- Controllers must be thin
- No business logic in controllers
- Use async/await (no .Result or .Wait)
- Public async methods must accept CancellationToken
- No Console.WriteLine (use ILogger)
- Methods should not exceed 50 lines