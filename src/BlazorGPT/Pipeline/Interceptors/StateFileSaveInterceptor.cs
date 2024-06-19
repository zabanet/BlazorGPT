﻿using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace BlazorGPT.Pipeline.Interceptors;

public class StateFileSaveInterceptor : InterceptorBase, IInterceptor
{
    private readonly string _path;


    public StateFileSaveInterceptor(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<PipelineOptions>>().Value;
        _path = options.StateFileSaveInterceptorPath ?? string.Empty;
    }

    public override string Name { get; } = "Save file";
    public override bool Internal { get; } = true;

    public override async Task<Conversation> Receive(Kernel kernel, Conversation conversation, Func<string, Task<string>>? onComplete = null,
        CancellationToken cancellationToken = default)
    {
        await ParseMessageAndSaveStateToDisk(conversation.Messages.Last());
        return conversation;
    }

    public Task<Conversation> Send(Kernel kernel, Conversation conversation, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(conversation);
    }

    async Task ParseMessageAndSaveStateToDisk(ConversationMessage lastMsg)
    {
        var pattern = @"\[STATEDATA\](.*?)\[/STATEDATA\]";
        var matches = Regex.Matches(lastMsg.Content, pattern,
            RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        if (matches.Any())
        {
            var state = matches.First().Groups[1].Value;
            
                await File.WriteAllTextAsync( Path.Join(_path, lastMsg.Id.ToString()), state);
          
        }
    }
}