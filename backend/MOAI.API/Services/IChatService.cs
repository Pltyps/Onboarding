using System.Collections.Generic;
using System.Threading;
using MOAI.API.Models;

namespace MOAI.API.Services
{
    public interface IChatService
    {
        IAsyncEnumerable<string> StreamChatAsync(AppUser user, string userMessage, CancellationToken ct);
        Task<string> GenerateResponseAsync(string message, AppUser user, CancellationToken ct);

    }

}
