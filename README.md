#  prep<span style="color:#E30B5C;">AI</span>dopen

A Blazor Server chat Razor class library and application that uses <a href="https://learn.microsoft.com/en-us/semantic-kernel/overview">Semantic Kernel</a> together with the GPT chat completion and embeddings endpoints available from both OpenAI and MS Azure OpenAI. 
Local model support is provided through [Ollama](https://github.com/jmorganca/ollama).

- Create and manage multiple chat sessions with history. 
- Define your own custom system prompts and switch easily between them.
- QuickProfiles for quick access to your favorite text snippet shortcuts. 
- Scripts with multiple steps for automating a sequence of steps in a conversation. 
- Branch conversations into side conversations sharing the previous context. Inspired by Git.
- Restart a conversation from a previous step. 
- Customize the chat experience with middleware and filters.
- Select Semantic Kernel plugins from GUI with the PluginsInterceptor. Mix semantic (text) and native (code) functions.

## Features
- Chat with GPT-3.5 or GPT-4, Ollama. Edit and restart chats from previous steps.
  ![](docs/images/chat_toolbox.png)
  
- Define your own custom system prompts
![System prompt dropdown grid](docs/images/syspromptgrid.png)

- QuickProfiles for quick access to your favorite text snippet shortcuts
  ![](docs/images/QP.png)

- Scripts with mutiple steps for automating a conversation
  ![](docs/images/editscript.png)
  
- Branching of conversations into side conversations with the same context
  ![](docs/images/hasbranch.png)
  ![](docs/images/branched.png)

- Restart a conversation from a previous step
- Select which interceptors to run in pipeline
![](docs/images/config_interceptors.png)

- Select which plugins to run in pipeline
  ![](docs/images/config_plugins.png)

- Manage chat history
  ![](docs/images/history.png)




