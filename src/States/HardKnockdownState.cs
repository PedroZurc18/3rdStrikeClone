namespace rdStrikeClone.States;

using Godot;

public class HardKnockdownState : BaseState
{
    private int _timer;
    private float _slideFriction = 150.0f; // Adjust this to make them slide further or stop shorter
    
    // They must be completely immune to attacks while sliding on the floor!
    public override bool IsInvincible => true; 

    public HardKnockdownState(Fighter fighter, int durationFrames) : base(fighter)
    {
        _timer = durationFrames;
    }

    public override void Enter()
    {
        _fighter.Anim.Play("hard_knockdown");
        
        Vector2 vel = _fighter.Velocity;
        
        float pushDirection = Mathf.Sign(_fighter.GlobalPosition.X - _fighter.Opponent.GlobalPosition.X);
        vel.X = pushDirection * 100.0f; // The burst of slide speed
        
        vel.Y = 0; // Snap cleanly to the floor
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 vel = _fighter.Velocity;

        // 2. The Smooth Brake
        // This naturally slows their X velocity down to 0 over the first few frames of the state
        vel.X = Mathf.MoveToward(vel.X, 0, _slideFriction * (float)delta);
        
        // Re-apply gravity just in case they slide off an edge (if your stages have them!)
        if (!_fighter.IsOnFloor()) 
        {
            vel.Y += _fighter.Gravity * (float)delta;
        }

        _fighter.Velocity = vel;
        _fighter.ApplyMovementAndPush();

        // 3. The Knockdown Timer
        _timer--;
        if (_timer <= 0)
        {
            _fighter.ChangeState(new GetUpState(_fighter));
        }
    }
}