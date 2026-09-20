using Godot;

public partial class Fireball : Node2D // Changed back to Node2D to match your root!
{
    [Export] public float Speed = 600f;
    [Export] public int LifetimeFrames = 120;
    
    private int _direction = 1;
    private int _currentFrame = 0;

    public void Fire(int facingDirection)
    {
        _direction = facingDirection;
        Scale = new Vector2(_direction, 1); 
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentFrame++;
        
        Vector2 pos = GlobalPosition;
        pos.X += Speed * _direction * (float)delta;
        GlobalPosition = pos;

        if (_currentFrame >= LifetimeFrames)
        {
            QueueFree();
        }
    }
    
    public void OnHitboxEntered(Area2D area)
    {
        // Deal damage logic here...
        
        QueueFree(); // Destroy the fireball on impact
    }
}