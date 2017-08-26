using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace notetoself_mongo
{
    public class NotesManager {
        IMongoCollection<Note> Collection;
        
        public NotesManager(string MongoUser, string MongoPass) {
            MongoClient Mongo = new MongoClient($"mongodb://{MongoUser}:{MongoPass}@localhost:27017/notetoself");
            IMongoDatabase Data = Mongo.GetDatabase("notetoself");
            Collection = Data.GetCollection<Note>("notes");
        }

        async Task<Note> CreateNote(ulong UserId) {
            Note Note = new Note(UserId);
            await Collection.InsertOneAsync(Note);
            return Note;
        }

        public async Task<bool> UpdateNote(ulong UserId, Note Note) {
            ReplaceOneResult Result = await Collection.ReplaceOneAsync((filter) => filter.UserId == UserId, Note);
            return Result.IsAcknowledged;
        }

        public async Task<Note> GetNote(ulong UserId) {
            var Result = await Collection.FindAsync((filter) => filter.UserId == UserId);
            Note Note = await Result.FirstOrDefaultAsync();

            if (Note != null) return Note;
            else return await CreateNote(UserId);            
        }

        public async Task<bool> DeleteNote(ulong UserId) {
            DeleteResult Result = await Collection.DeleteOneAsync((filter) => filter.UserId == UserId);
            return Result.IsAcknowledged;
        }

        public async Task<long> CountUserNotes() {
            return await Collection.CountAsync(FilterDefinition<Note>.Empty);
        }

        public async Task<int> CountAllNotes() {
            int Count = 0;
            List<Note> Notes = await (await Collection.FindAsync(FilterDefinition<Note>.Empty)).ToListAsync();

            foreach (Note Note in Notes)
                Count = Count + Note.CountNotes();

            return Count;
        }
    }
}