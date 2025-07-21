using OpenAI.Chat;                     // for ChatClient, ChatMessage, etc.
using OpenAI.Embeddings;               // for EmbeddingClient

namespace MOAI.API.Services
{
    public class OpenAiClientService
    {
        private const int EMBEDDING_SIZE = 1536;  // text-embedding-3-small yields 1536-dim vectors

        private readonly ChatClient _chat;
        private readonly EmbeddingClient _embeddings;

        public OpenAiClientService(
            IConfiguration config)
        {
            var apiKey = config["OPENAI_API_KEY"]!;
            _chat = new ChatClient(model: "gpt-3.5-turbo", apiKey: apiKey);
            _embeddings = new EmbeddingClient(model: "text-embedding-3-small", apiKey: apiKey);
        }

        public async Task<string> GetChatCompletionAsync(string prompt, CancellationToken ct)
        {
        
            // Compact, trimmed system message
            var systemMsg = new SystemChatMessage("");

            // Safe prompt (e.g., ≤ 3000–4000 chars), not just blindly passing long strings
            var safePrompt = prompt.Length > 3500 ? prompt[..3500] + "..." : prompt;

            var messages = new List<ChatMessage>
            {
                systemMsg,
                new UserChatMessage(safePrompt)
            };

            Console.WriteLine($"[🔎 Prompt token length]: {safePrompt.Length / 4} estimated tokens");

            var response = await _chat.CompleteChatAsync(messages, cancellationToken: ct);
            return response.Value.Content[0].Text;
        }


        public async Task<float[]> GetEmbeddingAsync(string input, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new float[EMBEDDING_SIZE];

            var resp = await _embeddings.GenerateEmbeddingAsync(
                input,
                cancellationToken: ct
            );

            // OpenAIEmbedding has no .Embedding property; use ToFloats() instead :contentReference[oaicite:1]{index=1}
            var embedding = resp.Value;
            return embedding.ToFloats().ToArray();
        }
    }
}
