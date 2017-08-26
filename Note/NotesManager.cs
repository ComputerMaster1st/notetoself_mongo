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

        public async Task<Note> CreateNote(ulong UserId) {
            Note Note = new Note(UserId);
            await Collection.InsertOneAsync(Note);
            return Note;
        }

        public async Task<bool> UpdateNote(ulong UserId, Note Note) {
            ReplaceOneResult Result = await Collection.ReplaceOneAsync((filter) => filter.UserId == UserId, Note);
            return Result.IsAcknowledged;
        }

        public async Task<Note> GetNote(ulong UserId) {
            try {
                var Result = await Collection.FindAsync((filter) => filter.UserId == UserId);
                Note Note = await Result.FirstOrDefaultAsync();
                return Note;
            } catch (System.Exception e) {
                System.Console.WriteLine(e);
                return null;
            }
        }

        public async Task<bool> DeleteNote(ulong UserId) {
            DeleteResult Result = await Collection.DeleteOneAsync((filter) => filter.UserId == UserId);
            return Result.IsAcknowledged;
        }

        public async Task<long> CountUserNotes() {
            return await Collection.CountAsync(null);
        }

        public async Task<int> CountAllNotes() {
            int Count = 0;
            List<Note> Notes = await (await Collection.FindAsync(null)).ToListAsync();

            foreach (Note Note in Notes)
                Count = Count + Note.CountNotes();

            return Count;
        }
    }
}