namespace rdStrikeClone.States;

using Godot;

public class StandUpState : BaseState
{
    public StandUpState(Fighter fighter) : base(fighter) { }

    public override void Enter()
    {
        _fighter.Anim.Play("crouch_out");
    }

    public override void PhysicsUpdate(double delta)
    {
        // 1. Cancel instantly back into a crouch
        if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down))
        {
            _fighter.ChangeState(new CrouchState(_fighter));
            return;
        }

        // 2. Cancel instantly into walking
        if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Left) || 
            _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Right))
        {
            _fighter.ChangeState(new IdleState(_fighter));
            return;
        }

        // 3. One line checks EVERY standing attack!
        if (CheckStandingAttacks()) return;
        
        if (!_fighter.IsOnFloor())
        {
            Vector2 vel = _fighter.Velocity;
            vel.Y += _fighter.Gravity * (float)delta;
            _fighter.Velocity = vel;
        }
        _fighter.ApplyMovementAndPush();
        
        if (!_fighter.Anim.IsPlaying() || _fighter.Anim.CurrentAnimation != "crouch_out")
        {
            _fighter.ChangeState(new IdleState(_fighter));
        }
    }
}