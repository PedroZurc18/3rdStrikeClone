using Godot;
namespace rdStrikeClone;

public partial class Fireball : Node2D
{
    [Export] public int Speed;
    private int _direction;
    private Fighter _owner;

    public void Initialize(Fighter owner,int direction, Vector2 spawnPosition, NormalAttack attackStats)
    {
        _owner = owner;
        _direction = direction;
        GlobalPosition = spawnPosition;

        if (_direction == -1) Scale = new Vector2(-1, 1);

        HitboxData myHitBox = GetNode<HitboxData>("HitboxData");
        myHitBox.Parent = attackStats;

    }

    public override void _Ready()
    {
        HitboxData myHitbox = GetNode<HitboxData>("HitboxData");
        myHitbox.AreaEntered += OnProjectileHit;
        
        VisibleOnScreenNotifier2D screenNotifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
        screenNotifier.ScreenExited += QueueFree;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        float step = Speed * _direction * (float)delta;
        
        Vector2 currentPosition = GlobalPosition;
        currentPosition.X += step;
        GlobalPosition = currentPosition;
    }

    private void OnProjectileHit(Area2D area)
    {
        Fighter hitFighter = null;
        Node current = area;
        
        while (current != null && !(current is Fighter))
        {
            current = current.GetParent();
        }
        hitFighter = current as Fighter;

        if (hitFighter == null || hitFighter == _owner) return;
        
        HitboxData myHitbox = GetNode<HitboxData>("HitboxData");

        hitFighter.ReceiveHit(myHitbox.Parent, myHitbox);
        
        QueueFree();
    }
}