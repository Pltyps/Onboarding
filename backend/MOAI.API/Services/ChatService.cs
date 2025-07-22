﻿using System.Runtime.CompilerServices;
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

        // --------------------------------------------------------------------------------
        // 🧭 ENUMS & SUPPORT STRUCTS
        // --------------------------------------------------------------------------------

        private enum UserIntent
        {
            Acknowledgment,  // "thanks", "got it"
            Transition,      // "what else", "what now"
            Question         // Default
        }

        private class TextInput
        {
            public string Text { get; set; } = string.Empty;
        }

        private class TransformedText
        {
            public string[] Tokens { get; set; } = Array.Empty<string>();
        }

        // --------------------------------------------------------------------------------
        // 🧠 INTENT + QUERY CLASSIFIERS
        // --------------------------------------------------------------------------------

        private UserIntent ClassifyIntent(string input)
        {
            var msg = input.Trim().ToLowerInvariant();

            var acknowledgments = new[] { "thanks", "thank you", "got it", "ok", "okay", "cool", "perfect", "great", "awesome", "that helps" };
            if (acknowledgments.Any(a => msg == a || msg.StartsWith(a)))
                return UserIntent.Acknowledgment;

            if (msg.Contains("what else") || msg.Contains("what now") || msg.Contains("next") || msg.Contains("anything more"))
                return UserIntent.Transition;

            return UserIntent.Question;
        }

        private bool IsGeneralOnboardingQuery(string msg)
        {
            var lower = msg.ToLowerInvariant();
            return lower.Contains("i am new") ||
                   lower.Contains("i just started") ||
                   lower.Contains("what should i do") ||
                   lower.Contains("getting started") ||
                   lower.Contains("first steps");
        }

        private bool IsPolicyQuestion(string msg)
        {
            return msg.ToLowerInvariant().Contains("policy") ||
                   msg.ToLowerInvariant().Contains("procedure") ||
                   msg.ToLowerInvariant().Contains("compliance");
        }

        private string GetPolicyReference()
        {
            return "For official university policy, refer to the BYU Policy Portal at https://policy.byu.edu/ and check your department’s SharePoint site for specific onboarding materials.";
        }

        private string? MatchPolicyLink(string message)
        {
            var lower = message.ToLowerInvariant();

            if (lower.Contains("copyright"))
                return "https://copyright.byu.edu/copyright-faqs-basics";

            if (lower.Contains("vehicle rental") || lower.Contains("car rental"))
                return "https://pf.byu.edu/vehicle-rental";

            if (lower.Contains("room policy") || lower.Contains("event") || lower.Contains("scheduling"))
                return "https://scheduling.byu.edu/scheduling-policies";

            if (lower.Contains("speakers") || lower.Contains("speaker policy"))
                return "https://policy.byu.edu/view/speakers-and-events-policy?s=s706";

            if (lower.Contains("policy") || lower.Contains("procedure") || lower.Contains("compliance"))
                return "https://policy.byu.edu/";

            return null;
        }


        // --------------------------------------------------------------------------------
        // 📊 DATA EXTRACTION HELPERS
        // --------------------------------------------------------------------------------

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
                .Where(t => t.Length > 2)
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
                        var snippet = GetParagraphAround(lines, i, 4);
                        var entry = $"[{doc.FileName}]\n{snippet.Trim()}\n";
                        approxTokens += entry.Length / 4;
                        if (approxTokens >= tokenLimit) break;
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

        private string SummarizeOnboardingInfo(List<StoredDocument> docs)
        {
            var sb = new StringBuilder();
            int tokenBudget = 3000;
            int estimatedTokens = 0;

            foreach (var doc in docs)
            {
                var chunk = $"[{doc.FileName}]\n{doc.Content[..Math.Min(1000, doc.Content.Length)]}\n";
                estimatedTokens += chunk.Length / 4;
                if (estimatedTokens >= tokenBudget) break;
                sb.AppendLine(chunk);
            }

            return sb.ToString();
        }

        private string SummarizeAllDocuments(List<StoredDocument> docs)
        {
            var sb = new StringBuilder();
            int tokenLimit = 3000;
            int tokensUsed = 0;

            foreach (var doc in docs)
            {
                if (string.IsNullOrWhiteSpace(doc.Content)) continue;

                var summary = doc.Content.Length > 1000
                    ? doc.Content[..1000] + "..."
                    : doc.Content;

                var entry = $"[{doc.FileName}]\n{summary.Trim()}\n";
                tokensUsed += entry.Length / 4;
                if (tokensUsed >= tokenLimit) break;

                sb.AppendLine(entry);
            }

            return sb.ToString();
        }

        // --------------------------------------------------------------------------------
        // 🧱 PROMPT TEMPLATE
        // --------------------------------------------------------------------------------

        private string BuildPrompt(string userQuery, string department, string context, string priorHistory, string? policyLink)
        {
            return $"""
            You are an Assistant Chatbot in the BYU Marriott onboarding app.
            Your job is to:
            - Analyze the user's question carefully.
            - Search for answers using ONLY documents that match the user's department.
            - Quote directly from the document content, citing document names and relevant sections.
            - Do NOT make up any information or speculate beyond provided content.
            - Ensure your answers comply with university policies before responding.
            {(policyLink != null ? $"Refer to the following official university policy link: {policyLink}" : "")}



            Current User Question:
            "{userQuery}"

            Relevant Information from {department} Department Documents:
            {context}

            Previous Chat (context only, not to be replied to):
            {priorHistory}

            Instructions:
            - Respond ONLY to the current question.
            - If the user’s question matches instructions in the documents (e.g., steps, procedures, login guides), extract and list those exact steps.
            - Preserve exact wording and steps from the source if they are found.
            - DO NOT summarize—reproduce specific instructions if available.
            - Format them clearly as a numbered list.
            - Always cite the source file in brackets, e.g., [Business Manager Handbook.docx].
            - Do not speculate. If no data is found, suggest contacting the supervisor.
            - Complete your response in full.
            """;
        }

        // --------------------------------------------------------------------------------
        // 🛠 SPECIAL ADMIN HANDLING
        // --------------------------------------------------------------------------------

        private async Task<string?> TryHandleAdminQuestion(string message, AppUser user, int chatId, CancellationToken ct)
        {
            if (user.Role != "Admin") return null;

            if (message.Contains("stats") || message.Contains("diagnostic") || message.Contains("health"))
            {
                var stats = await _db.SystemStats.OrderByDescending(s => s.Timestamp).FirstOrDefaultAsync(ct);
                if (stats == null) return "No system statistics available at the moment.";

                var explanation = $"System Stats:\n" +
                                  $"- Active Users: {stats.ActiveUserCount}\n" +
                                  $"- Total Requests: {stats.TotalRequests}\n" +
                                  $"- Uptime: {stats.UptimeMinutes} minutes\n";

                await _history.AddMessageAsync(chatId, "assistant", explanation);
                return explanation;
            }

            return null;
        }

        // --------------------------------------------------------------------------------
        // 💬 MAIN CHAT RESPONSE HANDLER
        // --------------------------------------------------------------------------------

        public async Task<string> GenerateResponseAsync(string message, AppUser user, int chatSessionId, CancellationToken ct)
        {
            await _history.AddMessageAsync(chatSessionId, "user", message);
            var intent = ClassifyIntent(message);
            string reply;

            if (intent == UserIntent.Acknowledgment)
            {
                reply = "You're welcome! Let me know if you have any other questions.";
                var id = await _history.AddMessageAsync(chatSessionId, "assistant", reply);
                return JsonSerializer.Serialize(new { reply = reply, messageId = id });
            }

            if (intent == UserIntent.Transition)
            {
                reply = $"Since you're in the {user.Department} department, you might want to explore areas like scheduling, curriculum changes, or financial reporting next. Let me know which one you'd like to dive into.";
                var id = await _history.AddMessageAsync(chatSessionId, "assistant", reply);
                return JsonSerializer.Serialize(new { reply = reply, messageId = id });
            }

            if (user.Role == "Admin" && await TryHandleAdminQuestion(message, user, chatSessionId, ct) is string adminReply)
            {
                return JsonSerializer.Serialize(new { reply = adminReply });
            }

            // Document collection + content summarization
            var docs = await _db.Documents.Where(d => d.Department == user.Department && d.IsActive).ToListAsync(ct);
            var context = string.Empty;

            if (IsGeneralOnboardingQuery(message))
            {
                context = SummarizeOnboardingInfo(docs);
            }
            else
            {
                var keywords = ExtractKeywords(message);
                context = ExtractRelevantText(docs, keywords);
                if (string.IsNullOrWhiteSpace(context))
                    context = SummarizeAllDocuments(docs);
            }

            // Build prior message context
            var priorMessages = await _history.GetMessagesAsync(chatSessionId);
            var historyBlock = new StringBuilder();
            foreach (var msg in priorMessages.TakeLast(3))
            {
                historyBlock.AppendLine($"{msg.Role.ToUpperInvariant()}: {msg.Message}");
            }

            var policyLink = MatchPolicyLink(message);
            var prompt = BuildPrompt(message, user.Department, context, historyBlock.ToString(), policyLink);

            reply = await _openAi.GetChatCompletionAsync(prompt, ct);

            if (string.IsNullOrWhiteSpace(reply))
                reply = "I'm sorry, I wasn't able to find any helpful information for that. Please check with your department supervisor.";

            var botMsg = await _history.AddMessageAsync(chatSessionId, "assistant", reply);
            return JsonSerializer.Serialize(new { reply = reply, messageId = botMsg.Id });
        }

        // --------------------------------------------------------------------------------
        // 🔄 STREAMING VERSION
        // --------------------------------------------------------------------------------

        public async IAsyncEnumerable<string> StreamChatAsync(AppUser user, string message, int chatSessionId, [EnumeratorCancellation] CancellationToken ct)
        {
            var docs = await _db.Documents.Where(d => d.Department == user.Department && d.IsActive).ToListAsync(ct);
            var keywords = ExtractKeywords(message);
            var context = ExtractRelevantText(docs, keywords);

            var priorMessages = await _history.GetMessagesAsync(chatSessionId);
            var historyBlock = new StringBuilder();
            foreach (var msg in priorMessages.TakeLast(3))
            {
                historyBlock.AppendLine($"{msg.Role.ToUpperInvariant()}: {msg.Message}");
            }

            var policyLink = MatchPolicyLink(message);
            var prompt = BuildPrompt(message, user.Department, context, historyBlock.ToString(), policyLink);

            var reply = await _openAi.GetChatCompletionAsync(prompt, ct);

            await _history.AddMessageAsync(chatSessionId, "user", message);
            await _history.AddMessageAsync(chatSessionId, "assistant", reply);

            yield return reply;
        }
    }
}
