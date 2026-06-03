using System.Collections.Generic;

namespace Fortifid.Core;

public class Inventory
{
    private readonly Dictionary<string, int> _items = new();

    public IReadOnlyDictionary<string, int> Items => _items;

    public void Add(string itemName, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemName) || amount <= 0)
            return;

        _items.TryGetValue(itemName, out int currentAmount);
        _items[itemName] = currentAmount + amount;
    }

    public int Count(string itemName)
    {
        return _items.TryGetValue(itemName, out int amount) ? amount : 0;
    }
}
