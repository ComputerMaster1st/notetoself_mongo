using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace notetoself_mongo
{
    public class NoteContext : CommandContext {
        public NotesManager NotesManager { get; }

        public NoteContext(DiscordShardedClient Client, IUserMessage Message, NotesManager Notes) : base(Client, Message) {
             NotesManager = Notes;
        }
    }
}