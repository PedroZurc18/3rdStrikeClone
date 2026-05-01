using Godot;

namespace rdStrikeClone;

public partial class SpecialAttack : NormalAttack
{
    [ExportGroup("Special Properties")]
    [Export] public int InvincibilityFrames = 0;
}