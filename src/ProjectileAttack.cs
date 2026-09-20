using Godot;
using rdStrikeClone;

public partial class ProjectileAttack : SpecialAttack
{
    [ExportGroup("Projectile Data")]
    [Export] public PackedScene ProjectilePrefab;
    [Export] public int SpawnFrame = 12;
    
    public override bool ProcessMove()
    {
        bool isMoveFinished = base.ProcessMove();

        if (_currentFrame == SpawnFrame)
        {
            if (ProjectilePrefab != null)
            {
                Fireball newFireball = ProjectilePrefab.Instantiate<Fireball>();
                   
                _fighter.GetTree().Root.AddChild(newFireball);
                
                newFireball.Initialize(_fighter, _fighter.FacingDirection, GlobalPosition, this);
            }
        }
        return isMoveFinished;
    }
}