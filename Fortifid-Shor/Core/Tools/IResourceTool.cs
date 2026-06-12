using Fortifid.Core.Items;

namespace Fortifid.Core.Tools;

public interface IResourceTool : IItem
{
    int GetBonus(IItem resource);
}
