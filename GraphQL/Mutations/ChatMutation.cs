using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ChatMutation
{
    public Chat CreateChat(AppDbContext context, ChatInput input)
    {
        var chat = new Chat
        {
            ProjectId = input.ProjectId
        };

        context.Chats.Add(chat);
        context.SaveChanges();
        return chat;
    }

    public Chat? UpdateChat(AppDbContext context, UpdateChatInput input)
    {
        var chat = context.Chats.Find(input.Id);
        if (chat == null) return null;

        if (input.NewProjectId.HasValue)
            chat.ProjectId = input.NewProjectId.Value;

        context.SaveChanges();
        return chat;
    }

    public bool DeleteChat(AppDbContext context, int id)
    {
        var chat = context.Chats.Find(id);
        if (chat == null) return false;
        context.Chats.Remove(chat);
        context.SaveChanges();
        return true;
    }
}