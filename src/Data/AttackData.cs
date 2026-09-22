using Godot;

namespace rdStrikeClone.Data;

[GlobalClass]
public partial class AttackData : Resource
{
    public enum HitHeight { High, Mid, Low }
    public enum AttackStrength { Light, Medium, Heavy }
    public enum CommandDirection { Neutral, Forward, Back, Down, Up }
    public enum MotionType { None, QCF, QCB, DP, HalfCircleBack }

    [ExportGroup("Execution Requirements")]
    [Export] public InputBuffer.InputFlag RequiredButton;
    [Export] public CommandDirection RequiredDirection = CommandDirection.Neutral;
    [Export] public MotionType RequiredMotion = MotionType.None;
    [Export] public bool IsAirborneMove = false;

    [ExportGroup("Frame Data")]
    [Export] public string AnimationName;
    [Export] public int TotalFrames;

    [ExportGroup("Cancel Data")]
    [Export] public bool IsSpecialCancelable = false;
    [Export] public int CancelWindowStart = 0;
    [Export] public int CancelWindowEnd = 99;

    [ExportGroup("Physics & Movement")]
    [Export] public Godot.Collections.Array<SpeedKeyframe> XSpeedProfile = new();
    [Export] public Godot.Collections.Array<SpeedKeyframe> YSpeedProfile = new();
    [Export] public float AirDrag = 0.0f;

    [ExportGroup("Hit Stats")]
    [Export] public HitboxFrameData HitStats;

    [ExportGroup("Audio")]
    [Export] public AudioStream WhiffSound;
    [Export] public float WhiffVolumeDb = 0.0f;
    [Export] public int WhiffFrame = 0;
    [Export] public AudioStream HitSound;
    [Export] public float HitVolumeDb = 0.0f;
    [Export] public AudioStream BlockSound;
    [Export] public float BlockVolumeDb = 0.0f;
    [Export] public AudioStream VoiceSound;
    [Export] public float VoiceVolumeDb = 0.0f;
}