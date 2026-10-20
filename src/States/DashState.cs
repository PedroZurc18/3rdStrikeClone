namespace rdStrikeClone.States;

using Godot;

public class DashState : BaseState
{
    private int _dashDirection;
    private int _dashFrames;
    private float _dashSpeed;

    // 1 = RIGHT / -1 = LEFT
    public DashState(Fighter fighter, int dashDirection) : base(fighter)
    {
        _dashDirection = dashDirection;
        if (_dashDirection == _fighter.FacingDirection)
        {
            _dashFrames = 16;
            _dashSpeed = 800.0f;
        }
        else
        {
            _dashFrames = 23;
            _dashSpeed = 700.0f;
        }
    }

    public override void Enter()
    {
        if (_dashDirection == _fighter.FacingDirection)
        {
            _fighter.Anim.Play("dash_forward");
        }
        else
        {
            _fighter.Anim.Play("dash_back");
        }

        Vector2 vel = _fighter.Velocity;
        vel.X = _dashSpeed * _dashDirection;
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 vel = _fighter.Velocity;
        vel.X = _dashSpeed * _dashDirection;
        
        if (!_fighter.IsOnFloor())
        {
            vel.Y += _fighter.Physics.Gravity * (float)delta;
        }
        
        _fighter.Velocity = vel;
        _fighter.Physics.ApplyMovementAndPush();
        
        _dashFrames--;
        
        if (_dashFrames <= 0)
        {
            _fighter.StateMachine.ChangeState(new IdleState(_fighter));
        }
    }
}