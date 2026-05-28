namespace rdStrikeClone.States;

using Godot;

public class GetUpState : BaseState
{
    // CRITICAL: They must remain completely immune to attacks while standing up!
    public override bool IsInvincible => true;

    public GetUpState(Fighter fighter) : base(fighter)
    {
    }

    public override void Enter()
    {
        // Play your specific standing up animation
        _fighter.Anim.Play("get_up"); 
        
        // Guarantee they have stopped sliding
        Vector2 vel = _fighter.Velocity;
        vel.X = 0;
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 vel = _fighter.Velocity;

        // Apply gravity just in case
        if (!_fighter.IsOnFloor()) 
        {
            vel.Y += _fighter.Gravity * (float)delta;
        }

        _fighter.Velocity = vel;
        _fighter.ApplyMovementAndPush();

        // Check if the animation has finished naturally
        if (!_fighter.Anim.IsPlaying())
        {
            // Optional Polish: Check if they are holding down to immediately crouch-block!
            if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down))
            {
                // Assuming you can pass 'false' to skip the crouch_in transition animation
                _fighter.ChangeState(new CrouchState(_fighter)); 
            }
            else
            {
                _fighter.ChangeState(new IdleState(_fighter)); 
            }
        }
    }

}