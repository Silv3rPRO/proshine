namespace PROProtocol
{
    public class InventoryItem
    {
        public string Name { get; }
        public int Id { get; }
        public int Quantity { get; }
        public int Scope { get; }

        public InventoryItem(string name, int id, int quantity, int scope)
        {
            Name = name;
            Id = id;
            Quantity = quantity;
            Scope = scope;
        }

        public bool CanBeHeld => Scope != 6 && Scope != 7 && Scope != 10 && (Scope != 9 || !Name.StartsWith("HM"));
    }
}
