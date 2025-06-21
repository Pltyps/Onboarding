using System.Runtime.CompilerServices;
using System.Text;
using MOAI.API.Models;

namespace MOAI.API.Services
{
    public class ChatService : IChatService
    {
        private readonly IDocumentIndexService _index;
        private readonly OpenAiClientService _openAi;

        public ChatService(IDocumentIndexService index, OpenAiClientService openAi)
        {
            _index = index;
            _openAi = openAi;
        }

        // 🧠 Policy-aware instruction block
        private const string SystemInstruction = """
        You are an Assistant Chatbot in the BYU Marriott onboarding app.
        Your job is to:
        - Analyze the user's question carefully.
        - Search for answers using ONLY documents that match the user's department.
        - Quote directly from the document content, citing document names and relevant sections.
        - Do NOT make up any information or speculate beyond provided content.
        - Ensure your answers comply with university policy before responding.
        If unsure or not found, say you couldn't find the info and recommend reaching out to support.
        """;

        public async IAsyncEnumerable<string> StreamChatAsync(AppUser user, string message, [EnumeratorCancellation] CancellationToken ct)
        {
            var relevantDocs = await _index.GetRelevantDocumentsAsync(message, user.Role, user.Department);

            var context = new StringBuilder();
            foreach (var doc in relevantDocs)
            {
                context.AppendLine($"[{doc.FileName}]\n{doc.Content}");
            }

            var prompt = $"""
                {SystemInstruction}

                Context:
                {context}

                User Question: {message}
                """;



            var reply = await _openAi.GetChatCompletionAsync(prompt, ct);
            yield return reply;
        }

        public async Task<string> GenerateResponseAsync(string message, AppUser user, CancellationToken ct)
        {
            var docs = await _index.GetRelevantDocumentsAsync(message, user.Role, user.Department);

            var sb = new StringBuilder();
            foreach (var doc in docs)
            {
                sb.AppendLine($"[{doc.FileName}]\n{doc.Content}");
            }

            var prompt = $"""
                {SystemInstruction}

                Context:
                {sb}

                Question: {message}
                """;

            Console.WriteLine($"[Prompt]\n{prompt}\n---");

            return await _openAi.GetChatCompletionAsync(prompt, ct);
        }
    }
}
