namespace rdStrikeClone.States;

using Godot;

public class AirState : BaseState
{
    private bool _isJumping;

    // NEW: We added a toggle! It defaults to true so your IdleState doesn't break.
    public AirState(Fighter fighter, bool isJumping = true) : base(fighter) 
    {
        _isJumping = isJumping;
    }

    public override void Enter()
    {
        Vector2 vel = _fighter.Velocity;
        
        if (_isJumping)
        {
            // Apply the upward burst and directional momentum!
            vel.Y = _fighter.JumpForce;
            
            if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Right))
            {
                vel.X = _fighter.WalkSpeed;
                _fighter.Anim.Play((_fighter.FacingDirection == 1) ? "jump_forward" : "jump_backward");
            }
            else if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Left))
            {
                vel.X = -_fighter.WalkSpeed;
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
            // NEW: We are just falling (e.g., recovering from an air hit)
            // DO NOT apply jump force! Let them keep their current velocity.
            _fighter.Anim.Play("jump_neutral"); // Or "fall" if you have a falling animation
        }
        
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        if (_fighter.Buffer.WasInputPressedWithin(InputBuffer.InputFlag.Kick, 8))
        {
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