using rdStrikeClone.Data;

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
        if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down))
        {
            _fighter.StateMachine.ChangeState(new CrouchState(_fighter));
            return;
        }

        if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Left) || 
            _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Right))
        {
            _fighter.StateMachine.ChangeState(new IdleState(_fighter));
            return;
        }

        AttackData triggeredMove = _fighter.Moves.EvaluateAvailableMoves(_fighter.Buffer, false);
        if (triggeredMove != null)
        {
            _fighter.StateMachine.ChangeState(new AttackState(_fighter, triggeredMove));
            return;
        }
        
        if (!_fighter.IsOnFloor())
        {
            Vector2 vel = _fighter.Velocity;
            vel.Y += _fighter.Physics.Gravity * (float)delta;
            _fighter.Velocity = vel;
        }
        _fighter.Physics.ApplyMovementAndPush();
        
        if (!_fighter.Anim.IsPlaying() || _fighter.Anim.CurrentAnimation != "crouch_out")
        {
            _fighter.StateMachine.ChangeState(new IdleState(_fighter));
        }
    }
}