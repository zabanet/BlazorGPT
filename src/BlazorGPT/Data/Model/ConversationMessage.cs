using Azure.AI.OpenAI;
using SharpToken;


namespace BlazorGPT.Data.Model;

public class ConversationMessage
{
    private GptEncoding tokenizer = GptEncoding.GetEncoding("cl100k_base");

    public ConversationMessage(string role, string content)  
    {
        Role = role;
        Content = content;

        if (Role == "user")
            PromptTokens = tokenizer.Encode(Content).Count;
    }

    //public ConversationMessage(ChatMessage msg)  
    //{
    //    Role = msg.Role == ChatRole.User ? "user" : "assistant";
    //    Content = msg.Content;
    //}

    public Guid? Id { get; set; }
    public string Role { get; set; }
    public string Content { get; set; }
    public Conversation? Conversation { get; set; }
    public Guid? ConversationId { get; set; }

    public MessageState? State { get; set; }

    public ICollection<Conversation> BranchedConversations { get; set; } = new List<Conversation>();

    public int? PromptTokens { get; set; } = 0;
    public int? CompletionTokens { get; set; } = 0;

    public DateTime Date { get; set; } = DateTime.UtcNow;

}