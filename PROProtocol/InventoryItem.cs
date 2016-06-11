namespace PROProtocol
{
    public class InventoryItem
    {
        public string Name { get; private set; }
        public int Id { get; private set; }
        public int Quantity { get; private set; }
        public int Scope { get; private set; }

        public InventoryItem(string name, int id, int quantity, int scope)
        {
            Name = name;
            Id = id;
            Quantity = quantity;
            Scope = scope;
        }
    }
}
