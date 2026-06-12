using System.Collections.Generic;

namespace Fortifid.Core.Items;

public interface IItemContainer
{
    IReadOnlyCollection<ItemStack> Items { get; }

    void Add(IItem item, int amount = 1);

    bool Contains(IItem item, int amount = 1);

    bool Remove(IItem item, int amount = 1);

    int Count(IItem item);
}
