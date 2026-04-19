using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ClaudeAPI
{
    public class ClaudeService
    {
        private readonly HttpClient _httpClient;
        public ClaudeService(string apiKey)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("Anthropic-version", "2023-06-01");
        }

        public void AddMessage(List<Message> messages, string role, string text)
        {
            messages.Add(new Message
            {
                role = role,
                content = text
            });
        }

        public void AddUserMessage(List<Message> messages, string text)
        {
            messages.Add(new Message
            {
                role = "user",
                content = text
            });
        }

        public class Params
        {
            public string Model { get; set; }
            public int MaxTokens { get; set; }
            public string[] Messages { get; set; }
            public string? System { get; set; }
        }

        public void AddAssistantMessage(List<Message> messages, string text)
        {
            messages.Add(new Message
            {
                role = "assistant",
                content = text
            });
        }

        public async Task<string> ChatWithClaudeAsync(List<Message> messages, string model, string system="", double temperature=1.0)
        {
            var body = new
            {
                model = "claude-haiku-4-5-20251001",
                max_tokens = 1000,
                messages = messages,
                system = string.IsNullOrEmpty(system) ? null : system,
                temperature = temperature
            };

            //var body = new Params
            //{
            //    Model = model,
            //    MaxTokens = 1000,
            //    Messages = messages.Select(m => m.content).ToArray(),

            //};

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var response = await _httpClient.PostAsync(
                "https://api.anthropic.com/v1/messages",
                new StringContent(JsonSerializer.Serialize(body, options), Encoding.UTF8, "application/json")
            );

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();
        }
    }
    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class Parameters
    {
        public string Model { get; set; }
        public int MaxTokens { get; set; }
        public List<Message> Messages { get; set; }
        public string? System { get; set; }
    }
}
