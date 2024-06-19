﻿using Microsoft.SemanticKernel;
using System.Text;

namespace BlazorGPT.Pipeline.Interceptors;

public class JsonStateInterceptor : InterceptorBase, IInterceptor, IStateWritingInterceptor
{
    public JsonStateInterceptor(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }


    public override string Name { get; } = "Json Hive State";


    public async Task<Conversation> Send(Kernel kernel, Conversation conversation, CancellationToken cancellationToken = default)
    {

        if (conversation.Messages.Count() == 2)
        {
            await AppendInstruction(conversation);
        }

        return conversation;
    }

    private Task<Conversation> AppendInstruction(Conversation conversation)
    {

        string state = "{\"id\":\"0\"}";


        var contentBuilder = new StringBuilder();
        contentBuilder.Append("[INSTRUCTION]Here is an empty json data as a model. We will iteratively add data to the json on our respective side. If either one has changed structure or values of the model we send over the json data enclosed in a [STATEDATA][/STATEDATA] tag. We will send it to eachother ONLY when data has changed! ");
        contentBuilder.Append(" Here is the base data. Remember from the start not to send it back unless you have changed structure or values.");
        contentBuilder.Append($" [STATEDATA]{state}[/STATEDATA][/INSTRUCTION]");
    
        var outgoing = conversation.Messages.ElementAt(1).Content;
        contentBuilder.AppendLine(outgoing);
        conversation.Messages.ElementAt(1).Content = contentBuilder.ToString();

        conversation.Messages.ElementAt(1).State = new MessageState()
        {
            Type = "JsonStateInterceptor",
            Content = state,
            IsPublished = true,
            Name = "msgstate",
        };

        return Task.FromResult(conversation);
    }

    private string path = @"C:\source\BlazorGPT\BlazorGPT\wwwroot\state\";

    public async Task<Conversation> Receive(Kernel kernel, Conversation conversation, CancellationToken cancellationToken = default)
    {
        var newMessage = conversation.Messages.Last();
        await ParseMessageAndSaveState(newMessage, "JsonStateInterceptor");

        if (File.Exists(path + newMessage.Id))
        {
            File.Move(path + newMessage.Id, path + newMessage.Id + ".json");
        }
        return conversation;
    }
}