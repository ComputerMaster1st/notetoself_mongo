using MongoDB.Driver;

namespace notetoself_mongodb
{
    public class NotesManager {
        MongoClient Mongo = new MongoClient("mongodb://notetoself:cmaster1st@localhost:27017");
        IMongoDatabase Data;
        
        public NotesManager() {
            Data = Mongo.GetDatabase("notetoself");
        }

        
    }
}