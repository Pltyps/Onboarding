using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MOAI.API.Data;
using MOAI.API.Models;

namespace MOAI.API.Services
{
    public class DocumentIndexService : IDocumentIndexService
    {
        private readonly ApplicationDbContext _db;
        private readonly OpenAiClientService _openAi;
        private readonly ILogger<DocumentIndexService> _logger;
        private List<(StoredDocument doc, float[] embedding)> _index = new();

        private const int MaxChars = 2000;

        public DocumentIndexService(
            ApplicationDbContext db,
            OpenAiClientService openAi,
            ILogger<DocumentIndexService> logger)
        {
            _db = db;
            _openAi = openAi;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            var allDocs = await _db.Documents.ToListAsync(ct);
            var newIndex = new List<(StoredDocument, float[])>();

            foreach (var doc in allDocs)
            {
                if (string.IsNullOrWhiteSpace(doc.Content))
                {
                    _logger.LogWarning(
                        "Skipping document {DocumentId} because content was empty",
                        doc.Id);
                    continue;
                }

                var txt = doc.Content.Length > MaxChars
                    ? doc.Content.Substring(0, MaxChars)
                    : doc.Content;

                try
                {
                    var emb = await _openAi.GetEmbeddingAsync(txt, ct);
                    if (emb.All(v => v == 0))
                    {
                        _logger.LogWarning(
                            "Doc {DocumentId} produced no meaningful embedding; skipping.",
                            doc.Id);
                        continue;
                    }
                    newIndex.Add((doc, emb));
                }
                catch (Exception ex) when (
                    ex.Message.Contains("max_tokens_per_request", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Skipping document {DocumentId} because truncated chunk still exceeded token limits",
                        doc.Id);
                }
            }

            _index = newIndex;
        }

    public async Task IndexNewDocumentAsync(StoredDocument doc)
    {
        if (string.IsNullOrWhiteSpace(doc.Content))
        {
            Console.WriteLine($"[Index] Content length for {doc.FileName}: {doc.Content?.Length}");

            _logger.LogWarning("Skipping document {DocumentId} because content was empty", doc.Id);
            return;
        }

        var txt = doc.Content.Length > MaxChars ? doc.Content.Substring(0, MaxChars) : doc.Content;

        var emb = await _openAi.GetEmbeddingAsync(txt, CancellationToken.None);

        if (emb.All(v => v == 0))
        {
            _logger.LogWarning("Doc {DocumentId} produced no meaningful embedding; skipping.", doc.Id);
            return;
        }

        _index.Add((doc, emb));
    }

        public Task<IReadOnlyList<(StoredDocument doc, float[] embedding)>> GetIndexedDocsAsync()
            => Task.FromResult((IReadOnlyList<(StoredDocument, float[])>)_index);

        public async Task<List<StoredDocument>> GetRelevantDocumentsAsync(
            string query,
            string role,
            string department)
        {
            // 1) Embed the user’s query
            var qEmb = await _openAi.GetEmbeddingAsync(query, CancellationToken.None);

            var results = _index
                .Where(x => role == "Admin" || x.doc.Department == department)
                .OrderByDescending(x => CosineSimilarity(qEmb, x.embedding))
                .Take(5)
                .Select(x => x.doc)
                .ToList();

            Console.WriteLine($"[RAG] Retrieved {results.Count} documents for query: {query}");
            return results;
        }

        private static float CosineSimilarity(float[] a, float[] b)
        {
            float dot = 0, normA = 0, normB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            return dot / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
        }
    }
}
