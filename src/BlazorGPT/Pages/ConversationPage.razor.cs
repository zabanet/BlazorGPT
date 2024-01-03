﻿using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Blazored.LocalStorage;
using BlazorGPT.Pipeline;
using BlazorGPT.Pipeline.Interceptors;
using BlazorGPT.Settings;
using BlazorGPT.Shared;
using BlazorPro.BlazorSize;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Radzen;
using Radzen.Blazor;
using SharpToken;

namespace BlazorGPT.Pages
{
    public partial class ConversationPage : IDisposable
    {
        public class PromptModel
        {
            [Required]
            public string? Prompt { get; set; }
        }

        [Parameter]
        public bool UseFileUpload { get; set; }


        [Parameter]
        public string? NewDestinationPrefix { get; set; }

        [Parameter]
        public string? Class { get; set; }

        [Parameter]
        public string? Style { get; set; }
        [Parameter]
        public RenderFragment? ChildContent{ get; set; }

        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationState { get; set; }

        [Parameter]
        [SupplyParameterFromQuery]
        public bool BotMode { get; set; }

        [Parameter] public string? BotSystemInstruction { get; set; } = null;

        [Parameter]
        public string? UserId { get; set; } = null!;

        [Parameter]
        public Guid? ConversationId { get; set; }

        [Parameter]
        public Guid? MessageId { get; set; }

        PromptModel Model = new();

        [Inject]
        public IOptions<PipelineOptions> PipelineOptions { get; set; } = null!;

        [Inject]
        public QuickProfileRepository QuickProfileRepository { get; set; }

        [Inject]
        public IResizeListener ResizeListener { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;
        
        [Inject]
        public KernelService KernelService{ get; set; }


        [Inject]
        public IDbContextFactory<BlazorGptDBContext> DbContextFactory { get; set; } = null!;
        [Inject]
        public ScriptRepository ScriptRepository { get; set; } = null!;

        [Inject]
        public ConversationsRepository ConversationsRepository { get; set; }
        [Inject]
        public DialogService DialogService { get; set; } = null!;

        [Inject]
        public IInterceptorHandler  InterceptorHandler{ get; set; }

        [Inject]
        public ConversationInterop? Interop { get; set; }
       
        private Conversation Conversation = new();

        private ModelConfiguration? _modelConfiguration;

        bool promptIsReady;
        string scriptInput;
        bool showTokens = false;

        private int controlHeight { get; set; }
        private int initialControlHeight = 0;

        private Kernel _kernel = null!;
        private CancellationTokenSource _cancellationTokenSource;
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        private GptEncoding tokenizer;

        [Inject] private ModelConfigurationService _modelConfigurationService { get; set; }

         protected override async Task OnInitializedAsync()
         {


            if (UserId == null && AuthenticationState != null)
            {
                var authState = await AuthenticationState;
                var user = authState?.User;
                if (user?.Identity is not null && user.Identity.IsAuthenticated)
                {
                    UserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                }
            }

            BotSystemInstruction ??= PipelineOptions.Value.Bot.BotSystemInstruction;
             

            InterceptorHandler.OnUpdate += UpdateAndRedraw;
            tokenizer = GptEncoding.GetEncoding("cl100k_base");//gpt-4 and above
        }

        private async Task  UpdateAndRedraw()
        {
            await InvokeAsync(StateHasChanged);
        }

        protected override async Task OnParametersSetAsync()
        {
            UseFileUpload = BotMode ? PipelineOptions!.Value.Bot.FileUpload.Enabled :
                    PipelineOptions!.Value.FileUpload.Enabled;

            await SetupConversation();
            StateHasChanged();
        }



        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ResizeListener.OnResized += WindowResized;
                _browserIsSmall = await ResizeListener.MatchMedia(Breakpoints.SmallDown);


                initialControlHeight = _browserIsSmall ? 290 : 290;

                initialControlHeight = BotMode ? 150 : initialControlHeight;

                controlHeight = initialControlHeight;


                await Interop.SetupCopyButtons();

            }

            if (BotMode || selectedTabIndex == 0)
            {
                await Interop.FocusElement(_promptField2.Element);
            }

            await Interop.ScrollToBottom("message-pane");
        }

        async Task SetupConversation()
        {
            if (ConversationId != null)
            {
                var ctx = DbContextFactory.CreateDbContext();
                var loaded  = await ConversationsRepository.GetConversation(ConversationId);
                if (loaded != null)
                {
                    if (loaded.UserId != UserId)
                    {
                        throw new UnauthorizedAccessException();
                    }
                    if (loaded.BranchedFromMessageId != null)
                    {
                        var hive = ConversationsRepository.GetMergedBranchRootConversation((Guid)loaded.BranchedFromMessageId);

                        if (hive != null)
                        {
                            loaded.HiveConversation = hive;

                        }
                    }
                    Conversation = loaded;
                }
                else
                {
                    NavigationManager.NavigateTo("/conversation");
                }
            }
            else
            {
                Conversation = CreateDefaultConversation();
            }
            StateHasChanged();
        } 


        private async Task Summarize()
        {
            IsBusy = true;

            Conversation.AddMessage(new ConversationMessage("user", "Summarize all in 10 words"));
            await Send();

            await using var ctx = await DbContextFactory.CreateDbContextAsync();
            ctx.Attach(Conversation);
            Conversation.Summary = Conversation.Messages.Last(m => m.Role == ConversationRole.Assistant).Content;
            await ctx.SaveChangesAsync();

            await _conversations.LoadConversations();
            IsBusy = false;

        }


        private async Task SendConversation()
        {
            IsBusy = true;

            Model.Prompt = Model.Prompt?.TrimEnd('\n');

            if (!Conversation.HasStarted())
            {
                var selected = _profileSelectorStart != null ? _profileSelectorStart.SelectedProfiles : new List<QuickProfile>();
                string startMsg = string.Join(" ", selected.Select(p => p.Content));
                if (!string.IsNullOrEmpty(startMsg))
                    startMsg += "\n\n";

                Conversation.AddMessage(new ConversationMessage("user", startMsg + Model.Prompt));

                Conversation.DateStarted = DateTime.UtcNow;
                StateHasChanged();

            }
            else
            {
                Conversation.AddMessage(new ConversationMessage("user", Model.Prompt!));
                StateHasChanged();  

            }

            await semaphoreSlim.WaitAsync();

            _cancellationTokenSource = new CancellationTokenSource(2*60*1000);


            if ( BotMode)
            {
                _modelConfiguration =   _modelConfigurationService.GetDefaultConfig();
            }
            else
            {
                _modelConfiguration = await _modelConfigurationService.GetConfig();
            }
            _kernel = await KernelService.CreateKernelAsync( provider: _modelConfiguration.Provider, model: _modelConfiguration!.Model);

            try
            {
                var strs = await LocalStorageService.GetItemAsync<List<string>>("bgpt_interceptors");

                Conversation = await InterceptorHandler.Send(_kernel,
                    Conversation, strs
                    ,
                    _cancellationTokenSource.Token);


                await Send();


                if (Conversation.InitStage())
                {
                    var selectedEnd = _profileSelectorEnd != null
                        ? _profileSelectorEnd.SelectedProfiles
                        : new List<QuickProfile>();
                    if (selectedEnd.Any())
                        foreach (var profile in selectedEnd)
                        {
                            Conversation.AddMessage(new ConversationMessage("user", profile.Content));


                            StateHasChanged();
                            await Send();
                        }
                }
            }
            catch (InvalidOperationException ioe)
            {
                // todo: handle this
            }
            catch (Exception e)
            {
                // todo: handle this

            }
            finally
            {
                _cancellationTokenSource.TryReset();
                semaphoreSlim.Release();
                StateHasChanged();
            }

            IsBusy = false;
            if (selectedTabIndex == 0)
            {
                await Interop.FocusElement(_promptField2.Element);
            }
        }

        private int contextTokens = 0;

        private async Task Send()
        {
            promptIsReady = false;

            try
            {
                if (!Conversation.StopRequested)
                {
                    Conversation.AddMessage("assistant", "");

                    StateHasChanged();

                    var chatRequestSettings = new ChatRequestSettings();
                    chatRequestSettings.ExtensionData["MaxTokens"] = _modelConfiguration!.MaxTokens;
                    chatRequestSettings.ExtensionData["Temperature"] = _modelConfiguration!.Temperature;

                    Conversation = await
                        KernelService.ChatCompletionAsStreamAsync(_kernel, Conversation, chatRequestSettings, OnStreamCompletion, cancellationToken: _cancellationTokenSource.Token);

                }


                await using var ctx = await DbContextFactory.CreateDbContextAsync();

                bool isNew = false;
                bool wasSummarized = false;

                if (Conversation.Id == null || Conversation.Id == default(Guid))
                {
                    Conversation.UserId = UserId;
                    Conversation.Model = _modelConfiguration!.Model!;
                    isNew = true;

                    if (_profileSelectorStart != null)
                    {
                        foreach (var p in _profileSelectorStart.SelectedProfiles)
                        {
                            ctx.Attach(p);
                            Conversation.QuickProfiles.Add(p);
                        }
                    }

                    ctx.Conversations.Add(Conversation);
                }
                else
                {
                    ctx.Attach(Conversation);
                }


                if (Conversation.Summary == null)
                {
                    var last = Conversation.Messages.First(m => m.Role == ConversationRole.User).Content;
                    Conversation.Summary =
                        Model.Prompt?.Substring(0, Model.Prompt.Length >= 75 ? 75 : Model.Prompt.Length);
                    wasSummarized = true;
                }

                var tokens = ctx.UserTokens.SingleOrDefault(u => u.UserId == Conversation.UserId);

                if (tokens == null)
                {
                    var result = await ctx.UserTokens.AddAsync(new UserToken()
                        { UserId = Conversation.UserId, PromptTokens = 0, CompletionTokens = 0 });

                    tokens = result.Entity;
                }

                if (Conversation.Messages.Count > 1)
                {
                    var msg = Conversation.Messages.TakeLast(2).ToList();
                    contextTokens = CalculateContextTokens(Conversation);
                    tokens.PromptTokens += contextTokens;
                    tokens.CompletionTokens += msg[1].CompletionTokens.Value;
                    contextTokens += msg[0].PromptTokens.Value + msg[1].CompletionTokens.Value;

                }

                await ctx.SaveChangesAsync();

                Conversation =
                    await InterceptorHandler.Receive(_kernel, Conversation,
                        await LocalStorageService.GetItemAsync<List<string>>("bgtpc_interceptors"));

                if (!BotMode  && wasSummarized)
                {
                    await _conversations.LoadConversations();
                    StateHasChanged();
                }
                

                if (isNew)
                {
                   
                    NavigationManager.NavigateTo(
                        BotMode ? NewDestinationPrefix + "/" + Conversation.Id
                                : "/conversation/" + Conversation.Id, 
                        false);
                }

                StateHasChanged();

            }
            catch (TaskCanceledException)
            {
                var res = await DialogService.Alert("The operation was cancelled");
                Conversation.Messages.RemoveAt(Conversation.Messages.Count - 1);

                semaphoreSlim.Release();
                StateHasChanged();
                return;
            }
            catch (Exception e)
            {
                var res = await DialogService.Alert(e.StackTrace,
                    "An error occurred. Please try again/later. " + e.Message);
                Conversation.Messages.RemoveAt(Conversation.Messages.Count - 1);
                Console.WriteLine(e.StackTrace);
                StateHasChanged();
                return;
            }
            finally
            {
               
                    _cancellationTokenSource?.TryReset();
                    semaphoreSlim.Release();

            }

            //Conversation = conv;
            Model.Prompt = "";
        }

        private async Task<string> OnStreamCompletion(string s)
        {
    

            Conversation.Messages.Last().Content += s; 
            await Interop.ScrollToBottom("message-pane");

            StateHasChanged();
            return s;
        }

        private int CalculateContextTokens(Conversation conversation)
        {
            int tokens = 0;

            foreach (var message in conversation.Messages)
                tokens += tokenizer.Encode(message.Content).Count;

            return tokens;
        }

        private Conversations _conversations;

        private void GoToNew()
        {
            NavigationManager.NavigateTo("/conversation", false);
        }


        BrowserWindowSize browser = new BrowserWindowSize();

        void IDisposable.Dispose()
        {
            ResizeListener.OnResized -= WindowResized;

            if (InterceptorHandler != null)
                InterceptorHandler.OnUpdate -= UpdateAndRedraw;
        }

        async void WindowResized(object _, BrowserWindowSize window)
        {
            browser = window;
            StateHasChanged();
        }


        Script loadedScript;


        private async Task RunScript(Guid guid)
        {
            IsBusy = true;
            Conversation.Messages.Clear();

            loadedScript = await ScriptRepository.GetScript(guid);

            Conversation.Messages.Clear();
            Conversation.AddMessage(new ConversationMessage("system", loadedScript.SystemMessage));


            string formatted = string.Format(loadedScript.Steps.First().Message, new[] { scriptInput });
            Conversation.AddMessage(new ConversationMessage("user", formatted));
            StateHasChanged();
            await Send();

            foreach (var step in loadedScript.Steps.Skip(1))
            {
                Conversation.AddMessage(new ConversationMessage("user", step.Message));
                await Send();
                StateHasChanged();

            }
            IsBusy = false;
        }

        private async Task<string> RenderType()
        {
            var enabled = await LocalStorageService.GetItemAsync<List<string>>("bgtpc_interceptors");
            if (enabled.Any(i => i == "Structurizr Hive DSL"))
            {
                return "StructurizrDslInterceptor";
            }
            return "JsonInterceptor";
        }

        ScriptsDropdown? ScriptsDdn { get; set; }

        public bool IsBusy { get; set; }

        private bool preventDefaultKey = false;
        private async Task OnPromptKeyUp(KeyboardEventArgs obj)
        {

            if (!string.IsNullOrWhiteSpace(obj.Key))
            {
                promptIsReady = true;
            }

            if (obj.Key == "Enter" && obj.ShiftKey == false)
            {
                // move mouse focus from prompt field
                await Interop.Blurelement(_promptField2.Element);
                IsBusy = true;
                StateHasChanged();
                await SendConversation();
            }
            else
            {
                
            }
        }

        private void PromptChanged()
        {
            if (Model.Prompt?.Length > 0)
            {
                promptIsReady = true;
            }
            else
            {
                promptIsReady = false;
            }
        }


        private async Task ApplyEndProfile(QuickProfile profile)
        {
            IsBusy = true;

            Conversation.AddMessage(new ConversationMessage("user", profile.Content));
            await Send();
            IsBusy = false;

        }

        [Inject]
        public TooltipService? TooltipService { get; set; }

        void ShowTooltip(ElementReference elementReference, string content, TooltipOptions? options = null)
        {
            TooltipService!.Open(elementReference, content, options);
        }


        private RadzenTextArea? _promptField2;
        private QuickProfileSelector? _profileSelectorStart;

        private QuickProfileSelector? _profileSelectorEnd;
        private bool useState;

        [Inject]
        ILocalStorageService LocalStorageService { get; set; } = null!;
        
        //IEnumerable<string> enabledInterceptors = Array.Empty<string>();

        private int selectedTabIndex;

 

        private async Task HiveStateClicked()
        {
            if (Interop != null)
            {
                await Interop.OpenStateViewer("hive", Conversation.Id!.ToString() ?? string.Empty, await RenderType());
            }
        }


        private Conversation CreateDefaultConversation()
        {
            var c = new Conversation
            {
                Model = !string.IsNullOrEmpty(_modelConfiguration?.Model) ? _modelConfiguration!.Model : PipelineOptions.Value.Providers.OpenAI.ChatModel!,
                UserId = UserId
            };

            var message = new ConversationMessage("system", "You are a helpful assistant.");
            if (BotMode) message.Content = BotSystemInstruction!;

            c.AddMessage(message);
            return c;
        }

        private async Task FileAreaSynced(string folderId, IEnumerable<string> files)
        {
            Conversation.FileUrls = files.ToList();

            bool uploadIsCreatingNewConveration = Conversation.Id == null;
            if (uploadIsCreatingNewConveration)
            {
                Conversation = CreateDefaultConversation();

                Conversation.Id = Guid.Parse(folderId);
                await ConversationsRepository.SaveConversation(Conversation);
            }
            else
            {
                await ConversationsRepository.UpdateConversation(Conversation);
            }

            if (uploadIsCreatingNewConveration)
            {
                NavigationManager.NavigateTo(
                    BotMode ? NewDestinationPrefix + "/" + Conversation.Id
                        : "/conversation/" + Conversation.Id,
                    false);
            }

        }
    }
}
