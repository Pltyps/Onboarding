using Microsoft.EntityFrameworkCore;
using MOAI.API.Data;
using MOAI.API.Models;

namespace MOAI.API.Services;

public class ChatHistoryService
{
    private readonly ApplicationDbContext _db;

    public ChatHistoryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ChatSession> CreateNewChatAsync(string userEmail, string? title = null)
    {
        var existingChats = await _db.ChatSessions
            .Where(c => c.UserEmail == userEmail)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        if (existingChats.Count >= 10)
        {
            throw new InvalidOperationException("Chat limit reached (10). Please delete a session.");
        }

        var chat = new ChatSession
        {
            UserEmail = userEmail,
            Title = string.IsNullOrWhiteSpace(title)
            ? $"Chat {DateTime.UtcNow:yyyy-MM-dd HH:mm}"
            : title
        };

        _db.ChatSessions.Add(chat);
        await _db.SaveChangesAsync();
        return chat;
    }

    public async Task<ChatMessage> AddMessageAsync(int chatId, string role, string message)
    {
        var chat = await _db.ChatSessions.FindAsync(chatId);
        if (chat == null) throw new ArgumentException("Chat not found");

        var msg = new ChatMessage
        {
            ChatSessionId = chatId,
            Role = role,
            Message = message
        };

        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync();
        return msg; // ✅ return the saved message (with Id)
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(int chatId, int maxMessages = 10)
    {
        return await _db.ChatMessages
         .Where(m => m.ChatSessionId == chatId)
         .OrderByDescending(m => m.Timestamp)
         .Take(maxMessages)
         .OrderBy(m => m.Timestamp) // preserve chronological order
         .ToListAsync();
    }



    public async Task<List<ChatSession>> GetChatsForUserAsync(string userEmail)
    {
        return await _db.ChatSessions
            .Where(c => c.UserEmail == userEmail)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> DeleteChatAsync(int chatId, string userEmail)
    {
        var chat = await _db.ChatSessions
            .FirstOrDefaultAsync(c => c.Id == chatId && c.UserEmail == userEmail);

        if (chat == null) return false;

        _db.ChatSessions.Remove(chat);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task UpdateTitleAsync(int chatId, string title)
    {
        var chat = await _db.ChatSessions.FindAsync(chatId);
        if (chat == null) return;

        chat.Title = title;
        await _db.SaveChangesAsync();
    }

    public async Task<ChatMessage?> GetMessageByIdAsync(int id)
    {
        return await _db.ChatMessages.FindAsync(id);
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }


}

