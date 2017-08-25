using System.Threading.Tasks;
using MongoDB.Driver;

namespace notetoself_mongodb
{
    public class NotesManager {
        MongoClient Mongo = new MongoClient("mongodb://notetoself:cmaster1st@localhost:27017");
        IMongoCollection<Note> Collection;
        
        public NotesManager() {
            IMongoDatabase Data = Mongo.GetDatabase("notetoself");
            Collection = Data.GetCollection<Note>("notes");
        }

        public async Task<Note> CreateNote(ulong UserId) {
            Note Note = new Note(UserId);
            await Collection.InsertOneAsync(Note);
            return Note;
        }
    }
}