
namespace CoachFrank.Commands.Utils;

//https://github.com/RPCS3/discord-bot/blob/master/CompatBot/Utils/AsciiColumn.cs
public sealed class AsciiColumn
{
    public AsciiColumn(string name = null, bool disabled = false, bool alignToRight = false, int maxWidth = 80)
    {
        Name = name;
        Disabled = disabled;
        AlignToRight = alignToRight;
        MaxWidth = maxWidth;
    }

    public readonly string Name;
    public readonly bool Disabled;
    public readonly bool AlignToRight;
    public readonly int MaxWidth;
}