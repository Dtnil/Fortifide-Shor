using Fortifid.Core.Items;

namespace Fortifid.Core.Tools;

public sealed class CopperPickaxe : IResourceTool
{
    public string Name => "Мідна кирка";

    public int GetBonus(IItem resource)
    {
        return resource == GameItems.Stone || resource == GameItems.CopperOre ? 1 : 0;
    }
}
