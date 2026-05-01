using Godot;

public partial class HitboxData : Area2D
{
    public NormalAttack Parent; 
    [Export] public int StartFrame = 6;
    [Export] public int Duration = 8;
    
    [Export] public NormalAttack.HitHeight Height = NormalAttack.HitHeight.Mid;
    [Export] public float PushbackForce = 300.0f;
    
    public bool HasConnected = false;
}