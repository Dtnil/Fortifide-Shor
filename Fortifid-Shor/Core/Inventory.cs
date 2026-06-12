using System;
using System.Collections.Generic;
using System.Linq;
using Fortifid.Core.Items;
using Fortifid.Core.Tools;

namespace Fortifid.Core;

public class Inventory : IInventory
{
    private readonly Dictionary<IItem, int> _items = new();

    public IReadOnlyCollection<ItemStack> Items =>
        _items.Select(entry => new ItemStack(entry.Key, entry.Value)).ToArray();

    public void Add(IItem item, int amount = 1)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (amount <= 0)
            return;

        _items.TryGetValue(item, out int currentAmount);
        _items[item] = currentAmount + amount;
    }

    public int GetResourceBonus(IItem resource)
    {
        return _items.Keys
            .OfType<IResourceTool>()
            .Select(tool => tool.GetBonus(resource))
            .DefaultIfEmpty(0)
            .Max();
    }

    public bool Contains(IItem item, int amount = 1)
    {
        return amount <= 0 || Count(item) >= amount;
    }

    public bool Remove(IItem item, int amount = 1)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (amount <= 0)
            return false;

        if (!Contains(item, amount))
            return false;

        int newAmount = _items[item] - amount;
        if (newAmount <= 0)
            _items.Remove(item);
        else
            _items[item] = newAmount;

        return true;
    }

    public int Count(IItem item)
    {
        return _items.TryGetValue(item, out int amount) ? amount : 0;
    }
}
