using ClaudeAPI;
using Message = ClaudeAPI.Message;

var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
List<Message> messages = new List<Message>();
var claudeService = new ClaudeService(apiKey!);
string answer = string.Empty;
string input = string.Empty;

Console.WriteLine("¿How can I help you today?");
input = Console.ReadLine() ?? string.Empty;

do
{
    claudeService.AddMessage(messages, "user", input);
    answer = await claudeService.ChatWithClaudeAsync(messages);
    claudeService.AddMessage(messages, "assistant", answer);
    Console.WriteLine(answer);
    input = Console.ReadLine() ?? string.Empty;
} 
while (!string.IsNullOrEmpty(input));

