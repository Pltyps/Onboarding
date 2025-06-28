using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using MOAI.API.Data;
using MOAI.API.Models;
using System.Text.Json;

namespace MOAI.API.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _db;
        private readonly OpenAiClientService _openAi;
        private readonly ChatHistoryService _history;

        public ChatService(ApplicationDbContext db, OpenAiClientService openAi, ChatHistoryService history)
        {
            _db = db;
            _openAi = openAi;
            _history = history;
        }

        // 🧠 Policy-aware instruction block
        private const string SystemInstruction = """
            You are an Assistant Chatbot in the BYU Marriott onboarding app.
            Your job is to:
            - Analyze the user's question carefully.
            - Search for answers using ONLY documents that match the user's department.
            - Quote directly from the document content, citing document names and relevant sections.
            - Do NOT make up any information or speculate beyond provided content.
            - Ensure your answers comply with university policies before responding.
            If unsure or not found, say you couldn't find the info and recommend reaching out to support.
            """;

        public async Task<string> GenerateResponseAsync(string message, AppUser user, int chatSessionId, CancellationToken ct)
        {
            var docs = await _db.Documents
                .Where(d => d.Department == user.Department && d.IsActive)
                .ToListAsync(ct);

            var keywords = ExtractKeywords(message);
            var context = ExtractRelevantText(docs, keywords);

            var priorMessages = await _history.GetMessagesAsync(chatSessionId);

            var historyBlock = new StringBuilder();
            foreach (var msg in priorMessages)
            {
                historyBlock.AppendLine($"{msg.Role}: {msg.Message}");
            }

            var prompt = $"""
            {SystemInstruction}

            Prior Conversation:
            {historyBlock}

            Context:
            {context}

            User Question:
            {message}
            """;

            Console.WriteLine($"[🧠 Prompt with Memory]\n{prompt}\n---");

            var response = await _openAi.GetChatCompletionAsync(prompt, ct);

            // Save this exchange
            await _history.AddMessageAsync(chatSessionId, "user", message);
            var savedBotMsg = await _history.AddMessageAsync(chatSessionId, "assistant", response);


            return JsonSerializer.Serialize(new
            {
                reply = response,
                messageId = savedBotMsg.Id
            });

        }



        public async IAsyncEnumerable<string> StreamChatAsync(AppUser user, string message, int chatSessionId, [EnumeratorCancellation] CancellationToken ct)
        {
            var docs = await _db.Documents
                .Where(d => d.Department == user.Department && d.IsActive)
                .ToListAsync(ct);

            var keywords = ExtractKeywords(message);
            var context = ExtractRelevantText(docs, keywords);

            var priorMessages = await _history.GetMessagesAsync(chatSessionId);

            var historyBlock = new StringBuilder();
            foreach (var msg in priorMessages)
            {
                historyBlock.AppendLine($"{msg.Role}: {msg.Message}");
            }

            var prompt = $"""
            {SystemInstruction}

            Prior Conversation:
            {historyBlock}

            Context:
            {context}

            User Question:
            {message}
            """;

            Console.WriteLine($"[🧠 Streaming Prompt with Memory]\n{prompt}\n---");

            var reply = await _openAi.GetChatCompletionAsync(prompt, ct);

            await _history.AddMessageAsync(chatSessionId, "user", message);
            await _history.AddMessageAsync(chatSessionId, "assistant", reply);

            yield return reply;
        }





        private List<string> ExtractKeywords(string input)
        {
            var mlContext = new MLContext();

            var data = new List<TextInput> { new() { Text = input } };
            var dataView = mlContext.Data.LoadFromEnumerable(data);

            var pipeline = mlContext.Transforms.Text.TokenizeIntoWords("Tokens", nameof(TextInput.Text))
                .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Tokens", "Tokens"));

            var model = pipeline.Fit(dataView);
            var transformed = model.Transform(dataView);

            var tokensColumn = mlContext.Data.CreateEnumerable<TransformedText>(transformed, reuseRowObject: false).FirstOrDefault();

            return tokensColumn?.Tokens
                .Where(t => t.Length > 2) // Optional: filter short noise
                .Distinct()
                .ToList()
                ?? new List<string>();
        }


        private string ExtractRelevantText(List<StoredDocument> docs, List<string> keywords)
        {
            var sb = new StringBuilder();
            int tokenLimit = 3000;
            int approxTokens = 0;

            foreach (var doc in docs)
            {
                var lines = doc.Content.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (keywords.Any(k => lines[i].Contains(k, StringComparison.OrdinalIgnoreCase)))
                    {
                        var snippet = GetParagraphAround(lines, i, 2);
                        var entry = $"[{doc.FileName}]\n{snippet.Trim()}\n";

                        approxTokens += entry.Length / 4; // heuristic for tokens
                        if (approxTokens >= tokenLimit)
                            break;

                        sb.AppendLine(entry);

                    }
                }

                if (approxTokens >= tokenLimit)
                    break;
            }
            Console.WriteLine($"[🧪 Extracted Keywords] {string.Join(", ", keywords)}");

            return sb.ToString();
        }

        private string GetParagraphAround(string[] lines, int index, int range)
        {
            int start = Math.Max(0, index - range);
            int end = Math.Min(lines.Length - 1, index + range);
            return string.Join('\n', lines[start..(end + 1)]);
        }
        
        private class TextInput
        {
            public string Text { get; set; } = string.Empty;
        }

        private class TransformedText
        {
            public string[] Tokens { get; set; } = Array.Empty<string>();
        }
        
    }
}
