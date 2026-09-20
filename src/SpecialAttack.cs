using Godot;

namespace rdStrikeClone;

public partial class SpecialAttack : NormalAttack
{
    public enum MotionType { QCF, QCB, DP, HalfCircleBack }

    [ExportGroup("Special Execution")]
    [Export] public MotionType RequiredMotion;
    
    [ExportGroup("Special Properties")]
    [Export] public int InvincibilityFrames = 0;
}