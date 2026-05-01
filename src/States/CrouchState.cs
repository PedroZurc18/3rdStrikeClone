namespace rdStrikeClone.States;

using Godot;

public class CrouchState : BaseState
{
    private bool _crouchIn;
    public override bool CanBlock => true;

    public CrouchState(Fighter fighter, bool crouchIn = true) : base(fighter)
    {
        _crouchIn = crouchIn;
    }

    public override void Enter()
    {
        if (_crouchIn)
        {
            _fighter.Anim.Play("crouch_in");
        
            _fighter.Anim.Queue("crouch_idle");
        }
        else
        {
            _fighter.Anim.Queue("crouch_idle");
        }
        
        Vector2 vel = _fighter.Velocity;
        vel.X = 0; 
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        _fighter.TurnToFaceOpponent();
        
        if (CheckSpecialAttacks()) return;
        if (CheckCrouchingAttacks()) return;
        
        if (!_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down))
        {
            if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Left) || 
                _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Right))
            {
                _fighter.ChangeState(new IdleState(_fighter));
            }
            else
            {
                _fighter.ChangeState(new StandUpState(_fighter));
            }
            return; 
        }
        
        if (!_fighter.IsOnFloor())
        {
            Vector2 vel = _fighter.Velocity;
            vel.Y += _fighter.Gravity * (float)delta;
            _fighter.Velocity = vel;
        }
        
        _fighter.ApplyMovementAndPush();
    }
}