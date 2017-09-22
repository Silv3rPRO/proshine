namespace PROProtocol
{
    public class ChatChannel
    {
        public ChatChannel(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }
    }
}