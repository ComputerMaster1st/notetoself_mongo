using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace notetoself_mongo
{
    class Program {
        NotesManager Notes;
        DiscordShardedClient Client;
        CommandService Command;
        
        static void Main(string[] args) => new Program().Execute().GetAwaiter().GetResult();

        async Task Execute() {
            Console.WriteLine("Starting Note to Self Bot v1.1 ...");

            MasterConfig Config = MasterConfig.Load();

            if (Config == null) Config = MasterConfig.Setup();

            Config.Save();

            Notes = new NotesManager(Config.MongoUser, Config.MongoPass);

            DiscordSocketConfig ClientConfig = new DiscordSocketConfig() {
                DefaultRetryMode = RetryMode.AlwaysFail,
                LogLevel = LogSeverity.Info
            };
            CommandServiceConfig CommandConfig = new CommandServiceConfig() { DefaultRunMode = RunMode.Async };
            Client = new DiscordShardedClient(ClientConfig);
            Command = new CommandService(CommandConfig);

            await InitializeCommands();

            Client.Log += (msg) => Log(msg);

            await Client.LoginAsync(TokenType.Bot, Config.Token);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        async Task InitializeCommands() {
            Client.MessageReceived += ExecuteCommandHandler;
            await Command.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly());
        }

        async Task ExecuteCommandHandler(SocketMessage MessageParam) {
            await Task.Factory.StartNew(() => ExecuteCommand(MessageParam));
        }

        async Task ExecuteCommand(SocketMessage MessageParam) {
            SocketUserMessage Message = (SocketUserMessage)MessageParam;
            if (Message == null) return;
            int ArgPos = 0;

            if (!(Message.HasStringPrefix("notetoself! ", ref ArgPos) || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos))) return;
            if (Message.Author.IsBot || Message.Author.IsWebhook) return;

            IGuildUser User = (IGuildUser)Message.Author;
            IGuildUser Self = await User.Guild.GetCurrentUserAsync();
            IGuildChannel Channel = (IGuildChannel)Message.Channel;
            ChannelPermissions Perms = Self.GetPermissions(Channel);
            
            if (!Perms.EmbedLinks) {
                await Message.Channel.SendMessageAsync("I'm sorry but I require embeds to be enabled before I can start posting to chat.");
                return;
            }

            NoteContext Context = new NoteContext(Client, Message, Notes);
            var Result = await Command.ExecuteAsync(Context, ArgPos);

            if (!Result.IsSuccess) {
                string Error = $"Command Error: {Result.ErrorReason}";
                Console.WriteLine(Error);

                switch (Result.Error) {
                    case CommandError.BadArgCount:
                        await Message.Channel.SendMessageAsync(Error);
                        break;
                    case CommandError.UnmetPrecondition:
                        await Message.Channel.SendMessageAsync(Error);
                        break;
                    case CommandError.Unsuccessful:
                        await Message.Channel.SendMessageAsync(Error);
                        break;
                    default:
                        Console.WriteLine(Error);
                        break;
                }
            }
        }

        async Task Log(LogMessage Message) {
            await Task.Factory.StartNew(() => Console.WriteLine($"[ {DateTime.Now.ToString("HH:mm:ss")} ] <=> {Message.ToString()}"));
        }
    }
}
