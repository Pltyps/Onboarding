using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MOAI.API.Models;

namespace MOAI.API.Services
{
    public interface IDocumentIndexService
    {
        Task InitializeAsync(CancellationToken ct = default);
        Task<IReadOnlyList<(StoredDocument doc, float[] embedding)>> GetIndexedDocsAsync();
        Task<List<StoredDocument>> GetRelevantDocumentsAsync(string query, string role, string department);
        Task IndexNewDocumentAsync(StoredDocument doc);

    }
}
