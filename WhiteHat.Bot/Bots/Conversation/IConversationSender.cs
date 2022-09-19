using System.Collections.Generic;

namespace WhiteHat.Bot.Bots.Conversation
{
    public interface IConversationSender
    {
        List<string> GetMinutesOfMeeting();
        bool AddNote(string note);
        bool DeleteNode(int index);
        bool AddParticipants(string participants);
        bool SendMail();
    }
}