using OpenAI.Chat;                     // for ChatClient, ChatMessage, etc.
using OpenAI.Embeddings;               // for EmbeddingClient

namespace MOAI.API.Services
{
    public class OpenAiClientService
    {
        private const int EMBEDDING_SIZE = 1536;  // text-embedding-3-small yields 1536-dim vectors

        private readonly ChatClient _chat;
        private readonly EmbeddingClient _embeddings;
        private readonly PolicyLoaderService _policyLoader;

        public OpenAiClientService(
            IConfiguration config,
            PolicyLoaderService policyLoader)
        {
            var apiKey = config["OPENAI_API_KEY"]!;
            _chat = new ChatClient(model: "gpt-3.5-turbo", apiKey: apiKey);
            _embeddings = new EmbeddingClient(model: "text-embedding-3-small", apiKey: apiKey);
            _policyLoader = policyLoader;
        }

        public async Task<string> GetChatCompletionAsync(string prompt, CancellationToken ct)
        {
            // no CancellationToken overload on LoadSystemPolicyAsync
            var policyText = await _policyLoader.LoadSystemPolicyAsync();  // :contentReference[oaicite:5]{index=5}

            var baseGuidelines = /* ... your guidelines ... */ "";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage($"{baseGuidelines}\n\n{policyText}"),
                new UserChatMessage(prompt)
            };

            var response = await _chat.CompleteChatAsync(messages, cancellationToken: ct);
            var completion = response.Value;
            return completion.Content[0].Text;
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
