using Godot;
using rdStrikeClone.Data;

namespace rdStrikeClone.Components;

public partial class Projectile : Area2D
{
    public Fighter OwnerFighter;
    public AttackData MoveData;
    public HitboxFrameData HitStats;
    public float Speed = 600f;
    public int Direction = 1;

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
        
        Vector2 currentScale = Scale;
        currentScale.X *= Direction; 
        Scale = currentScale;
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += new Vector2(Speed * Direction * (float)delta, 0);
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.GetParent() is Fighter hitFighter && hitFighter == OwnerFighter.Opponent)
        {
            hitFighter.Combat.ReceiveHit(MoveData, HitStats);
            
            QueueFree();
        }
    }
    
    public void OnVisibilityNotifierScreenExited()
    {
        QueueFree();
    }
}