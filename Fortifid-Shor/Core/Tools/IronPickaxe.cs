using Fortifid.Core.Items;

namespace Fortifid.Core.Tools;

public sealed class IronPickaxe : IResourceTool
{
    public string Name => "Залізна кирка";

    public int GetBonus(IItem resource)
    {
        return resource == GameItems.Stone
            || resource == GameItems.CopperOre
            || resource == GameItems.IronOre
            ? 2
            : 0;
    }
}
