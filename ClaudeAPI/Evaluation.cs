using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ClaudeAPI
{
    public class Evaluation
    {
        private readonly ClaudeService _claudeService;
        
        public Evaluation()
        {
            _claudeService = new ClaudeService(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")!);
        }

        public async Task<string> RunPrompt(TestCase testCase)
        {
            //"Merges the prompt with the test case input, then return the result"
            string prompt = $@"Please solve the following task: {testCase.Task}" + 
                "* Respond only with Python, JSON or a plain Regex" + 
                "* Do not add any comments or commentary or explanation";
            List<Message> messages = new List<Message>();


            _claudeService.AddUserMessage(messages, prompt);
            _claudeService.AddAssistantMessage(messages, "```code");
            var output = await _claudeService.Chat(messages, new[] { "```" });
            return output;

        }

        public async Task<Result> RunTestCase(TestCase testCase)
        {
            //"Calls RunPrompt, then grades the result"
            var output = await RunPrompt(testCase);

            //TODO: Grading
            var modelGrade = await GradeByModelAsync(testCase, output);
            var modelScore = modelGrade.score;
            var reasoning = modelGrade.reasoning;

            var syntaxScore = GradeSyntax(output, testCase);
            var score = (modelScore + syntaxScore) / 2;

            return new Result
            {
                Output = output,
                TestCase = testCase,
                Score = score,
                Reasoning = reasoning
            };
        }

        public async Task<EvaluationResult> GradeByModelAsync(TestCase testCase, string output)
        {
            var evalPrompt = $@"
                You are an expert AWS code reviewer. Your task is to evaluate the following AI-generated solution.

                Original Task:
                <task>
                {testCase.Task}
                </task>

                Solution to Evaluate:
                <solution>
                {output}
                </solution>

                Criteria you should use to evaluate the solution:
                <criteria>
                {testCase.Solution_Criteria}
                </criteria>

                Output Format
                Provide your evaluation as a structured JSON object with the following fields, in this specific order:
                - ""strengths"": An array of 1-3 key strengths
                - ""weaknesses"": An array of 1-3 key areas for improvement
                - ""reasoning"": A concise explanation of your overall assessment
                - ""score"": A number between 1-10

                Ensure all strings are valid JSON strings.
                Do not include invalid escape sequences like \- or \_.
                Do not escape characters unnecessarily.

                Example response shape:
                {{
                    ""strengths"": string[],
                    ""weaknesses"": string[],
                    ""reasoning"": string,
                    ""score"": number
                }}
                ";

            var messages = new List<Message>();

            _claudeService.AddUserMessage(messages, evalPrompt);
            _claudeService.AddAssistantMessage(messages, "```json");
            var evalText = await _claudeService.Chat(messages, new[] { "```" });

            return JsonSerializer.Deserialize<EvaluationResult>(evalText);
        }

        public int ValidateJson(string text)
        {
            try
            {
                JsonDocument.Parse(text.Trim());
                return 10;
            }
            catch
            {
                return 0;
            }
        }

        public int ValidatePython(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            // validación muy básica
            return text.Contains("def ") || text.Contains(":") ? 10 : 0;
        }

        public int ValidateRegex(string text)
        {
            try
            {
                _ = new Regex(text.Trim());
                return 10;
            }
            catch
            {
                return 0;
            }
        }

        public int GradeSyntax(string response, TestCase testCase)
        {
            var format = testCase.Format?.ToLower();

            if (format == "json")
            {
                return ValidateJson(response);
            }
            else if (format == "python")
            {
                return ValidatePython(response);
            }
            else
            {
                return ValidateRegex(response);
            }
        }

        public async Task<List<Result>> RunEvalAsync(List<TestCase> dataset)
        {
            //"Loads the dataset and calls run test case with each case"
            List<Result> results = new List<Result>();
            foreach (var testCase in dataset)
            {
                var result = await RunTestCase(testCase);
                results.Add(result);
            }
            var averageScore = results.Average(r => r.Score);
            Console.WriteLine($"Average Score: {averageScore}");
            //var tasks = dataset.Select(testCase => RunTestCase(testCase));
            //var results = await Task.WhenAll(tasks);
            return results;

        }

        public async Task<string> GenerateDataset()
        {
            List<Message> messages = new List<Message>();

            var prompt = "Generate a evaluation dataset for a prompt evaluation. The dataset will be used to evaluate prompts" +
                "that generate Python, JSON, or Regex specifically for AWS-related tasks. Generate an array of JSON objects," +
                "each representing task that requires Python, JSON, or a Regex to complete." +
                "   Example output:" +
                "       ```json" +
                "               [" +
                "                   {" +
                "                       \"task\": \"Description of task\"," +
                "                       \"format\": \"python|json|regex\"," +
                "                       \"solution_criteria\": \"Key criteria for evaluating the solution\"" +         
                "                   },    ...additional" +
                "               ]" +
                "        ```" +
                "   * Focus on tasks that can be solved by writing a single Python function, a single JSON object, or a regular expression." +
                "   * Focus on tasks that do not require writing much code" +
                "   Please generate 3 objects.";

            _claudeService.AddUserMessage(messages, prompt);
            _claudeService.AddAssistantMessage(messages, "```bash");
            var text = await _claudeService.Chat(messages, new[] { "```" });
            return text;
        }
    }

    public class Result
    {
        public string Output { get; set; }
        public object TestCase { get; set; }
        public double Score { get; set; }
        public string Reasoning { get; set; }
    }

    public class TestCase
    {
        [JsonPropertyName("task")]
        public string Task { get; set; }
        [JsonPropertyName("format")]
        public string Format { get; set; }
        [JsonPropertyName("solution_criteria")]
        public string Solution_Criteria { get; set; }
    }

    public class EvaluationResult
    {
        public List<string> strengths { get; set; }
        public List<string> weaknesses { get; set; }
        public string reasoning { get; set; }
        public int score { get; set; }
    }
}
