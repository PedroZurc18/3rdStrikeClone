using Godot;

namespace rdStrikeClone.Data;

[GlobalClass]
public partial class HitboxFrameData : Resource
{
    [ExportGroup("Combat Stats")]
    [Export] public int Damage = 10;
    [Export] public int HitStunFrames = 15;
    [Export] public int BlockStunFrames = 11;
    [Export] public int HitStopFrames = 12;
    [Export] public float PushbackForce = 300.0f;
    [Export] public float Pull = 0.0f;
    [Export] public float Scoop = 0.0f;

    [ExportGroup("Hit Properties")]
    [Export] public AttackData.HitHeight Height = AttackData.HitHeight.Mid;
    [Export] public AttackData.AttackStrength Strength = AttackData.AttackStrength.Medium;
    [Export] public bool Juggle = false;
    [Export] public float AirLaunchY = -1200.0f;
    [Export] public float AirPushX = 200.0f;
}