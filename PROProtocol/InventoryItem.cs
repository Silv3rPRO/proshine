namespace PROProtocol
{
    public class InventoryItem
    {
        public InventoryItem(string name, int id, int quantity, int scope)
        {
            Name = name;
            Id = id;
            Quantity = quantity;
            Scope = scope;
        }

        public string Name { get; }
        public int Id { get; }
        public int Quantity { get; }
        public int Scope { get; }
    }
}