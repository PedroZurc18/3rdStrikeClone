namespace rdStrikeClone.States;

using Godot;

public class HardKnockdownState : BaseState
{
    private int _timer;

    public HardKnockdownState(Fighter fighter, int durationFrames)
        : base(fighter)
    {
        _timer = durationFrames;
    }

    public override void Enter()
    {
        _fighter.Anim.Play("hard_knockdown_grounded"); // Placeholder animation
        // Ensure horizontal velocity is zeroed upon entering knockdown
        _fighter.Velocity = new Vector2(0, _fighter.Velocity.Y);
    }

    public override void PhysicsUpdate(double delta)
    {
        // Apply gravity if somehow not on floor (e.g., knocked off edge)
        if (!_fighter.IsOnFloor())
        {
            Vector2 vel = _fighter.Velocity;
            vel.Y += _fighter.Gravity * (float)delta;
            _fighter.Velocity = vel;
        }
        // Move with any existing velocity (mainly vertical due to gravity if off floor)
        _fighter.ApplyMovementAndPush();

        _timer--;

        if (_timer <= 0)
        {
            _fighter.Anim.Play("get_up"); // Play get up animation
            _fighter.ChangeState(new IdleState(_fighter)); // Transition to Idle after get up anim
        }
    }

    public override void Exit()
    {
        // Ensure horizontal velocity is zeroed out
        Vector2 vel = _fighter.Velocity;
        vel.X = 0;
        _fighter.Velocity = vel;
    }
}
