using Fortifid.Core.Tools;

namespace Fortifid.Core.Items;

public static class GameItems
{
    public static IItem Wood { get; } = new Item("Деревина");
    public static IItem Stone { get; } = new Item("Камінь");
    public static IItem CopperOre { get; } = new Item("Мідна руда");
    public static IItem IronOre { get; } = new Item("Залізна руда");
    public static IItem CopperIngot { get; } = new Item("Мідний злиток");
    public static IItem IronIngot { get; } = new Item("Залізний злиток");
    public static IItem IronSword { get; } = new Item("Залізний меч");

    public static IResourceTool StoneAxe { get; } = new StoneAxe();
    public static IResourceTool CopperPickaxe { get; } = new CopperPickaxe();
    public static IResourceTool IronPickaxe { get; } = new IronPickaxe();
}
