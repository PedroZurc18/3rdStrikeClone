namespace rdStrikeClone;

using Godot;

public partial class ProjectileAttack : SpecialAttack
{
    [Export] public PackedScene FireballPrefab;
    [Export] public int FireFrame = 14;
    
    [Export] public Marker2D SpawnPoint; 

    public override bool ProcessMove()
    {
        bool isFinished = base.ProcessMove(); 
        
        if (GetCurrentFrame() == FireFrame && FireballPrefab != null)
        {
            Fireball fireball = FireballPrefab.Instantiate<Fireball>();
            
            GetNode("/root/MainStage").AddChild(fireball);
            
            if (SpawnPoint != null)
                fireball.GlobalPosition = SpawnPoint.GlobalPosition;
            else
                fireball.GlobalPosition = this.GlobalPosition; 

            // Tell it which way to fly
            fireball.Fire(_fighter.FacingDirection);
        }

        return isFinished;
    }
}