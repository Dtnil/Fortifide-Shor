using Fortifid.Core.Items;

namespace Fortifid.Core.Tools;

public sealed class StoneAxe : IResourceTool
{
    public string Name => "Кам'яна сокира";

    public int GetBonus(IItem resource)
    {
        return resource == GameItems.Wood ? 1 : 0;
    }
}
