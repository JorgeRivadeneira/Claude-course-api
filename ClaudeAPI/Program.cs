using ClaudeAPI;
using System.Data;
using System.Text.Json;
using Message = ClaudeAPI.Message;

//1st example, with user interaction in a loop
//Code for the entire program, including the Main method and any necessary classes or methods.
//static async Task Main(string[] args)
//{
//    var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
//    List<Message> messages = new List<Message>();
//    var claudeService = new ClaudeService(apiKey!);
//    string answer = string.Empty;
//    string input = string.Empty;

//    Console.ForegroundColor = ConsoleColor.White;
//    Console.WriteLine("¿How can I help you today?");
//    Console.ForegroundColor = ConsoleColor.Blue;
//    input = Console.ReadLine() ?? string.Empty;

//    //var system = "You are a patient math tutor. Do not directly answer a student's questions. Guide them to a solution step by step";
//    var system = "Please, could you provide the answer the most accurately possible?, and also a short and precise answer, also, is it possible to send a final question to continue with the thread?";

//    do
//    {
//        claudeService.AddMessage(messages, "user", input);
//        var model = "claude-haiku-4-5-20251001";
//        answer = await claudeService.StreamClaudeAsync(messages, system);
//        claudeService.AddMessage(messages, "assistant", answer);
//        Console.ForegroundColor = ConsoleColor.Blue;
//        input = Console.ReadLine() ?? string.Empty;
//    }
//    while (!string.IsNullOrEmpty(input));
//}

//await Main(args);


//2nd example, with a single prompt and response, without user interaction
//var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
//List<Message> messages = new List<Message>();
//var claudeService = new ClaudeService(apiKey!);
//var prompt = "Generate 3 differnt samples of CLI git commands. Each should be very short";

//claudeService.AddUserMessage(messages, prompt);
//claudeService.AddAssistantMessage(messages, "Here are all the 3 commands in a single block without any comments:\n```bash");
//var text = await claudeService.ChatWithClaudeAsync(messages, "claude-haiku-4-5-20251001");
//text.Trim();
//Console.WriteLine(text);


//3rd example, with a single prompt and response, without user interaction, but with a system prompt to guide the model's response
Evaluation evaluation = new Evaluation();
var dataset = await evaluation.GenerateDataset();
File.WriteAllText("dataset.json", dataset);

string json = await File.ReadAllTextAsync("dataset.json");

var options = new JsonSerializerOptions
{
    WriteIndented = true
};
// Convertir a objetos
var ds = JsonSerializer.Deserialize<List<TestCase>>(json, options);

// Ejecutar evaluación
var results = await evaluation.RunEvalAsync(ds);
Console.WriteLine(results);
