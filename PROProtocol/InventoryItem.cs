namespace PROProtocol
{
    public class InventoryItem
    {
        public string Name { get; }
        public int Id { get; }
        public int Quantity { get; }
        public int Scope { get; }

        public InventoryItem(int id, int quantity, int scope)
        {
            Name = ItemsDatabase.Instance.GetItemName(id);
            Id = id;
            Quantity = quantity;
            Scope = scope;
        }

        public bool CanBeHeld => Scope != 6 && Scope != 7 && Scope != 10 && (Scope != 9 || !Name.StartsWith("HM"));

        public bool CanBeUsedOutsideOfBattle => Scope == 8 || Scope == 10 || Scope == 15;

        public bool CanBeUsedOnPokemonOutsideOfBattle => Scope == 2 || Scope == 3 || Scope == 9 || Scope == 13 || Scope == 14;

        public bool CanBeUsedInBattle => Scope == 5;

        public bool CanBeUsedOnPokemonInBattle => Scope == 2;
    }
}
