using rdStrikeClone.Data;

namespace rdStrikeClone.States;

using Godot;

public class AirState : BaseState
{
    private bool _isJumping;
    
    public AirState(Fighter fighter, bool isJumping = true) : base(fighter)
    {
        _isJumping = isJumping;
    }

    public override void Enter()
    {
        Vector2 vel = _fighter.Velocity;
        
        if (_isJumping)
        {
            vel.Y = _fighter.Physics.JumpForce;
            
            if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Right))
            {
                vel.X = _fighter.Physics.WalkSpeed;
                _fighter.Anim.Play((_fighter.FacingDirection == 1) ? "jump_forward" : "jump_backward");
            }
            else if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Left))
            {
                vel.X = -_fighter.Physics.WalkSpeed;
                _fighter.Anim.Play((_fighter.FacingDirection == -1) ? "jump_forward" : "jump_backward");
            }
            else
            {
                vel.X = 0; 
                _fighter.Anim.Play("jump_neutral");
            }
        }
        else
        {
            _fighter.Anim.Play("jump_neutral"); 
        }
        
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        AttackData triggeredMove = _fighter.Moves.EvaluateAvailableMoves(_fighter.Buffer, true);
        
        if (triggeredMove != null)
        {
            _fighter.StateMachine.ChangeState(new AttackState(_fighter, triggeredMove, true));
            return;
        }

        Vector2 vel = _fighter.Velocity;
        vel.Y += _fighter.Physics.Gravity * (float)delta;
        _fighter.Velocity = vel;
        
        _fighter.Physics.ApplyMovementAndPush();
        
        if (_fighter.IsOnFloor() && vel.Y > 0)
        {
            _fighter.StateMachine.ChangeState(new IdleState(_fighter, true));
        }
    }
}