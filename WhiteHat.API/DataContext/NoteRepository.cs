using WhiteHat.API.Model;

namespace WhiteHat.API.DataContext
{
    public class NoteRepository : INoteRepository
    {
        public bool AddNote(string sessionId, string note)
        {
           if(string.IsNullOrEmpty(sessionId))
                throw new ArgumentNullException(nameof(sessionId));

           if(string.IsNullOrEmpty(note))
                throw new ArgumentNullException(nameof(note));

            try
            {
                using var context = new ApiContext();
                var minute = context.Minutes?.FirstOrDefault(x => x.SessionId == sessionId);

                //Create SessionId if doesn't exists
                if(minute == null)
                {
                    minute = new Minute(sessionId);
                    context.Minutes?.Add(minute);
                    context.SaveChanges();
                }

                if(!string.IsNullOrEmpty(note))
                {
                    minute.Notes.Add(note);
                    context.Minutes?.Update(minute);
                    context.SaveChanges();
                }

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public bool DeleteNote(string sessionId, int noteIndex)
        {
            if (string.IsNullOrEmpty(sessionId))
                throw new ArgumentNullException(nameof(sessionId));

            if(noteIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(noteIndex));

            using var context = new ApiContext();
            var minute = context.Minutes?.FirstOrDefault(x => x.SessionId == sessionId);

            if (minute == null)
                throw new Exception($"Unable to find Minute of meetings for given session id {sessionId}");

            if(minute.Notes.Count < noteIndex)
                throw new ArgumentOutOfRangeException(nameof(noteIndex));
                       
            
            minute.Notes.RemoveAt(noteIndex);
            context.Minutes?.Update(minute);
            context.SaveChanges();           

            return true;
        }

        public Minute? GetMoM(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                throw new ArgumentNullException(nameof(sessionId));

            using var context = new ApiContext();
            var mom = context.Minutes?.FirstOrDefault(x => x.SessionId == sessionId);

            if (mom == null)
                throw new Exception($"Unable to find Minute of meetings for given session id {sessionId}");

            return mom;
        }

        public bool EmailMoM(string sessionId)
        {
            using var context = new ApiContext();
            var note = context.Minutes?.FirstOrDefault(x => x.SessionId == sessionId);
            // Call Email Service            

            return true;
        }

        public bool UpdateMom(Minute mom)
        {
            using var context = new ApiContext();
            var minute = context.Minutes?.FirstOrDefault(x => x.SessionId == mom.SessionId);

            if (minute == null)
                throw new Exception($"Unable to find Minute of meetings for given session id {mom.SessionId}");

            context.Minutes?.Remove(minute);
            context.Minutes?.Add(mom);
            context.SaveChanges();

            return true;
        }

        #region Extra functions
        public IEnumerable<Minute> GetNotes()
        {
            using var context = new ApiContext();
            var list = context.Minutes
                .ToList();
            return list;
        }

        public bool ClearSessions()
        {
            using var context = new ApiContext();       
            return true;
        }

        #endregion
    }
}