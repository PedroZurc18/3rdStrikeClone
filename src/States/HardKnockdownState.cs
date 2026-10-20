namespace rdStrikeClone.States;

using Godot;

public class HardKnockdownState : BaseState
{
    private int _timer;
    private float _slideFriction = 150.0f;
    
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
        vel.X = pushDirection * 100.0f;
        
        vel.Y = 0;
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 vel = _fighter.Velocity;
        
        vel.X = Mathf.MoveToward(vel.X, 0, _slideFriction * (float)delta);
        
        if (!_fighter.IsOnFloor()) 
        {
            vel.Y += _fighter.Physics.Gravity * (float)delta;
        }

        _fighter.Velocity = vel;
        _fighter.Physics.ApplyMovementAndPush();
        
        _timer--;
        if (_timer <= 0)
        {
            _fighter.StateMachine.ChangeState(new GetUpState(_fighter));
        }
    }
}