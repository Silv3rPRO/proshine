namespace PROProtocol
{
    public static class ChatEmotes
    {
        private static readonly string[] _emotes =
        {
            "surprised",
            "confused",
            "in love",
            "thinking...",
            "having an idea",
            "expressionless",
            "scared",
            "embarassed",
            "jaded",
            "apprehending something",
            "ignoring you",
            "whistling",
            "waving at someone",
            "angry",
            "laughing",
            "blushing",
            "sleeping",
            "enthusiastic",
            "smiling",
            "crying",
            "dizzying",
            "smiling with style",
            "sending a kiss",
            "demonic",
            "angelic",
            "falling in love",
            "anguished",
            "disagreeing",
            "agreeing"
        };

        public static string GetDescription(int emoteId)
        {
            if (emoteId <= 0 || emoteId > _emotes.Length)
                return "EMOTE(" + emoteId + ")";
            return _emotes[emoteId - 1];
        }
    }
}