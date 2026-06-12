using Fortifid.Core.Items;

namespace Fortifid.Core;

public interface IInventory : IItemContainer
{
    int GetResourceBonus(IItem resource);
}
