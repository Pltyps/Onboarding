using MOAI.API.Models;

namespace MOAI.API.Services
{
    public interface IChatService
    {
        Task<string> GenerateResponseAsync(string message, AppUser user, int chatSessionId, CancellationToken ct);
        IAsyncEnumerable<string> StreamChatAsync(AppUser user, string message, int chatSessionId, CancellationToken ct);
    }

}
