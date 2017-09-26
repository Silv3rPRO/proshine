namespace PROProtocol
{
    public class ShopItem
    {
        public ShopItem(string[] data, int index)
        {
            Id = int.Parse(data[index]);
            Name = data[index + 1];
            Price = int.Parse(data[index + 2]);
        }

        public int Id { get; }
        public string Name { get; }
        public int Price { get; }
    }
}