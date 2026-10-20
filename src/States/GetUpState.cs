namespace rdStrikeClone.States;

using Godot;

public class GetUpState : BaseState
{
    public override bool IsInvincible => true;

    public GetUpState(Fighter fighter) : base(fighter)
    {
    }

    public override void Enter()
    {
        _fighter.Anim.Play("get_up"); 
        
        Vector2 vel = _fighter.Velocity;
        vel.X = 0;
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 vel = _fighter.Velocity;
        
        if (!_fighter.IsOnFloor()) 
        {
            vel.Y += _fighter.Physics.Gravity * (float)delta;
        }

        _fighter.Velocity = vel;
        _fighter.Physics.ApplyMovementAndPush();
        
        if (!_fighter.Anim.IsPlaying())
        {
            // TRANSITION CROUCH-BLOCK
            if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down))
            {
                _fighter.StateMachine.ChangeState(new CrouchState(_fighter)); 
            }
            else
            {
                _fighter.StateMachine.ChangeState(new IdleState(_fighter)); 
            }
        }
    }

}