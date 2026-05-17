namespace rdStrikeClone.States;

using Godot;

public class AirState : BaseState
{
    public AirState(Fighter fighter) : base(fighter) { }

    public override void Enter()
    {
        Vector2 vel = _fighter.Velocity;
        vel.Y = _fighter.JumpForce;
        
        string animationToPlay = "jump_neutral";
        
        if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Right))
        {
            vel.X = _fighter.WalkSpeed;
            animationToPlay = (_fighter.FacingDirection == 1) ? "jump_forward" : "jump_backward";
        }
        else if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Left))
        {
            vel.X = -_fighter.WalkSpeed;
            animationToPlay = (_fighter.FacingDirection == -1) ? "jump_forward" : "jump_backward";
        }
        else
        {
            vel.X = 0; 
            animationToPlay = "jump_neutral";
        }
        
        _fighter.Anim.Play(animationToPlay);
        
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        // Check for air attacks FIRST
        if (_fighter.Buffer.WasInputPressedWithin(InputBuffer.InputFlag.Kick, 8))
        {
            // Make sure you define jHkPrefab in Fighter.cs, just like cMkPrefab!
            _fighter.ChangeState(new AirAttackState(_fighter, _fighter.jHkPrefab));
            return;
        }

        Vector2 vel = _fighter.Velocity;
        vel.Y += _fighter.Gravity * (float)delta;
        _fighter.Velocity = vel;
        
        _fighter.ApplyMovementAndPush();
        
        if (_fighter.IsOnFloor() && vel.Y > 0)
        {
            _fighter.ChangeState(new IdleState(_fighter, true));
        }
    }
}