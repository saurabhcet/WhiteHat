using WhiteHat.API.Model;

namespace WhiteHat.API.DataContext
{
    public interface INoteRepository
    {
        bool AddNote(string sessionId, string note);
        bool DeleteNote(string sessionId, int noteIndex);        
        Minute? GetMoM(string sessionId);
        bool EmailMoM(string sessionId);
        public bool UpdateMom(Minute mom);
        IEnumerable<Minute> GetNotes();
        bool ClearSessions();// Test
    }
}