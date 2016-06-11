namespace PROProtocol
{
    public class ChatChannel
    {
        public string Id { get; private set; }
        public string Name { get; private set; }

        public ChatChannel(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
