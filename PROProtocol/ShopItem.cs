namespace PROProtocol
{
    public class ShopItem
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int Price { get; private set; }

        public ShopItem(string[] data, int index)
        {
            Id = int.Parse(data[index]);
            Name = data[index + 1].Replace("\"", "");
            Price = int.Parse(data[index + 2]);
        }
    }
}
