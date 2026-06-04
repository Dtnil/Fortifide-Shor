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

    public bool Has(string itemName, int amount)
    {
        return amount <= 0 || Count(itemName) >= amount;
    }

    public bool Remove(string itemName, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemName) || amount <= 0)
            return false;

        if (!Has(itemName, amount))
            return false;

        int newAmount = _items[itemName] - amount;
        if (newAmount <= 0)
            _items.Remove(itemName);
        else
            _items[itemName] = newAmount;

        return true;
    }

    public int Count(string itemName)
    {
        return _items.TryGetValue(itemName, out int amount) ? amount : 0;
    }
}
