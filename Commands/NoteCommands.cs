using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace notetoself_mongo
{
    public class NoteCommands : ModuleBase<NoteContext> {
        [Command("add")]
        public async Task Add([Remainder]string RawNote) {
            if (RawNote.Length > 500) await ReplyAsync("", embed: SimpleEmbed("Notes can only be 500 characters maximum.", false));
            else {
                Note FetchNote = await Context.NotesManager.GetNote(Context.User.Id);
                Note Note = (FetchNote != null) ? FetchNote : await Context.NotesManager.CreateNote(Context.User.Id);

                Note.AddNote(RawNote);

                bool Result = await Context.NotesManager.UpdateNote(Context.User.Id, Note);

                if (Result) await ReplyAsync("", embed: SimpleEmbed($"New Self Note Created: `{RawNote}`"));
                else await ReplyAsync("", embed: SimpleEmbed("Note Creation Failed! Contact Developer!"));
            }
        }

        [Command("edit")]
        public async Task Edit(int Index, [Remainder]string RawNote) {
            if (RawNote.Length > 500) await ReplyAsync("", embed: SimpleEmbed("Notes can only be 500 characters maximum.", false));
            else {
                Note FetchNote = await Context.NotesManager.GetNote(Context.User.Id);
                Note Note = (FetchNote != null) ? FetchNote : await Context.NotesManager.CreateNote(Context.User.Id);

                bool EditResult = Note.EditNote(Index, RawNote);
                bool UpdateResult = await Context.NotesManager.UpdateNote(Context.User.Id, Note);

                if (!UpdateResult) await ReplyAsync("", embed: SimpleEmbed("Note Updating Failed! Contact Developer!"));
                else
                    if (EditResult) await ReplyAsync("", embed: SimpleEmbed($"Self Note **(ID: {Index})** Updated: `{RawNote}`"));
                    else await ReplyAsync("", embed: SimpleEmbed($"Self Note **(ID: {Index})** doesn't exist! Please create it as a new note!", false));
            }
        }

        [Command("delete")]
        public async Task Delete(int Index) {
            Note FetchNote = await Context.NotesManager.GetNote(Context.User.Id);
            Note Note = (FetchNote != null) ? FetchNote : await Context.NotesManager.CreateNote(Context.User.Id);

            bool DeleteNoteResult = Note.DeleteNote(Index);

            if (DeleteNoteResult) {
                bool DeleteDocumentResult = false;
                bool UpdatedDocument = false;

                if (Note.CountNotes() == 0)
                    DeleteDocumentResult = await Context.NotesManager.DeleteNote(Context.User.Id);
                else
                    UpdatedDocument = await Context.NotesManager.UpdateNote(Context.User.Id, Note);

                if (UpdatedDocument) await ReplyAsync("", embed: SimpleEmbed($"**(ID: {Index})** Deleted!"));
                else
                    if (DeleteDocumentResult) await ReplyAsync("", embed: SimpleEmbed($"**(ID: {Index})** Deleted! You no longer have any notes!"));
                    else await ReplyAsync("", embed: SimpleEmbed("Note Delete-Update Failed! Contact Developer!"));
            } else await ReplyAsync("", embed: SimpleEmbed($"**(ID: {Index})** Doesn't Exist!", false));
        }

        [Command("list")]
        public async Task List() {
            Note FetchNote = await Context.NotesManager.GetNote(Context.User.Id);
            Note Note = (FetchNote != null) ? FetchNote : await Context.NotesManager.CreateNote(Context.User.Id);

            if (Note.CountNotes() > 0) {
                List<string> Notes = Note.ListNotes();
                List<EmbedBuilder> Embeds = new List<EmbedBuilder>();

                int CharacterCount = 0;
                EmbedBuilder Builder = new EmbedBuilder();

                Builder.Title = "Your Self Notes!";
                Builder.Color = Color.Blue;
                    
                foreach (string RawNote in Notes) {
                    if (CharacterCount > 3400) {
                        Embeds.Add(Builder);
                        Builder = new EmbedBuilder();
                        Builder.Color = Color.Blue;
                    }

                    int Index = Notes.IndexOf(RawNote);                    
                    string FieldName = $"ID: {Index}";
                    int SubCharCount = FieldName.Length + RawNote.Length;

                    Builder.AddField(FieldName, RawNote);
                    CharacterCount = CharacterCount + SubCharCount;
                }

                if (CharacterCount <= 3400) Embeds.Add(Builder);

                foreach (EmbedBuilder Build in Embeds)
                    await ReplyAsync("", embed: Build.Build());
            } else {
                await ReplyAsync("", embed: SimpleEmbed("You have no self notes!", false));
            }
        }

        [Command("info")]
        public async Task Info() {
            EmbedBuilder Builder = new EmbedBuilder();
            var Guilds = await Context.Client.GetGuildsAsync();
            int Channels = 0;
            int Users = 0;

            foreach (IGuild Guild in Guilds) { 
                Channels = Channels + (await Guild.GetChannelsAsync()).Count;
                Users = Users + (await Guild.GetUsersAsync()).Count;
            }

            Builder.Title = "Note to Self Info!";
            Builder.Color = Color.Blue;
            Builder.AddInlineField("Guilds: ", Guilds.Count);
            Builder.AddInlineField("Channels: ", Channels);
            Builder.AddInlineField("Users: ", Users);

            Builder.AddInlineField("Users With Notes: ", Context.NotesManager.CountUserNotes());
            Builder.AddInlineField("Total Notes: ", Context.NotesManager.CountAllNotes());

            await ReplyAsync("", embed: Builder.Build());
        }

        [Command("help")]
        public async Task Help() {
            EmbedBuilder Builder = new EmbedBuilder();

            Builder.Title = "Note to Self Help!";
            Builder.Color = Color.Blue;
            Builder.Description = "Note to You... Remember to memorize all these commands! This is all I do!";
            Builder.AddField("notetoself! add <note>", "Enter your note into the note parameter to create it.");
            Builder.AddField("notetoself! list", "List all notes you have. Warning, this might spam many embeds if you have so many notes!");
            Builder.AddField("notetoself! edit <note id> <note>", "Edit the specified note by ID with the new note.");
            Builder.AddField("notetoself! delete <note id>", "Delete this note by ID.");
            Builder.AddField("notetoself! info", "Get bot statistics. Nothing more, nothing less.");
            Builder.AddField("notetoself! invite", "Get bot invite.");
            Builder.AddField("notetoself! help", "You are dumb if you want to know what this does.");

            await ReplyAsync("", embed: Builder.Build());
        }

        [Command("invite")]
        public async Task Invite() {
            await ReplyAsync("", embed: SimpleEmbed("https://discordapp.com/oauth2/authorize?client_id=350385352986591235&scope=bot&permissions=0"));
        }

        Embed SimpleEmbed(string Msg, bool Success = true) {
            EmbedBuilder Builder = new EmbedBuilder();

            Builder.Title = "Note to Self!";
                
            if (Success) Builder.Color = Color.Green;
            else Builder.Color = Color.Red;

            Builder.Description = Msg;

            Embed Embed = Builder.Build();

            return Embed;
        }
    }
}