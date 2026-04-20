using ClaudeAPI;
using Message = ClaudeAPI.Message;

static async Task Main(string[] args)
{
    var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
    List<Message> messages = new List<Message>();
    var claudeService = new ClaudeService(apiKey!);
    string answer = string.Empty;
    string input = string.Empty;

    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("¿How can I help you today?");
    Console.ForegroundColor = ConsoleColor.Blue;
    input = Console.ReadLine() ?? string.Empty;

    //var system = "You are a patient math tutor. Do not directly answer a student's questions. Guide them to a solution step by step";
    var system = "Please, could you provide the answer the most accurately possible?, and also a short and precise answer, also, is it possible to send a final question to continue with the thread?";

    do
    {
        claudeService.AddMessage(messages, "user", input);
        var model = "claude-haiku-4-5-20251001";
        answer = await claudeService.StreamClaudeAsync(messages, system);
        claudeService.AddMessage(messages, "assistant", answer);
        Console.ForegroundColor = ConsoleColor.Blue;
        input = Console.ReadLine() ?? string.Empty;
    }
    while (!string.IsNullOrEmpty(input));
}

await Main(args);

