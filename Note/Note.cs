using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace notetoself_mongo
{
    public class Note {
        [BsonId]
        public ulong UserId { get; }
        List<string> Notes = new List<string>();

        [BsonConstructor]
        public Note(ulong UserId) { this.UserId = UserId; }

        public void AddNote(string RawNote) {
            if (!Notes.Contains(RawNote)) Notes.Add(RawNote);
        }

        public List<string> ListNotes() { return Notes; }

        public int CountNotes() { return Notes.Count; }

        public bool EditNote(int Index, string Note) {
            if (Notes[Index] != null) {
                Notes[Index] = Note;
                return true;
            } else return false;
        }

        public bool DeleteNote(int Index) { 
            try {
                Notes.RemoveAt(Index);
                return true;
            } catch {
                return false;
            }
        }
    }
}