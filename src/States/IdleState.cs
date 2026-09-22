using rdStrikeClone.Data;

namespace rdStrikeClone.States;

using Godot;

public class IdleState : BaseState
{
    public override bool CanBlock => true;
    private bool _isLanding;
    private int _landingFrames = 13;
    private int _currentFrame = 0;
    
    private bool _fromLanding;
    private bool _wasCrouching;

    public IdleState(Fighter fighter, bool fromLanding = false, bool wasCrouching = false) : base(fighter)
    {
        _fromLanding = fromLanding;
        _wasCrouching = wasCrouching;
    }

    public override void Enter()
    {
        if (_wasCrouching)
        {
            _fighter.Anim.Play("crouch_out");
            _fighter.Anim.Queue("idle");
        }
        else
        {
            _fighter.Anim.Play("idle");
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Up))
        {
            _fighter.StateMachine.ChangeState(new AirState(_fighter));
            return;
        }
        
        AttackData triggeredMove = _fighter.Moves.EvaluateAvailableMoves(_fighter.Buffer, false);
        
        if (triggeredMove != null)
        {
            _fighter.StateMachine.ChangeState(new AttackState(_fighter, triggeredMove));
            return; 
        }
        
        if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down))
        {
            _fighter.StateMachine.ChangeState(new CrouchState(_fighter));
            return;
        }
        
        if (_isLanding && _fighter.Buffer.IsNeutral())
        {
            _fighter.Anim.Play("land");
            _currentFrame++;

            if (_currentFrame >= _landingFrames)
            {
                _isLanding = false;
                _currentFrame = 0;
            }
        }
        else
        {
            _currentFrame = 0;
            _isLanding = false;
        }
        
        // 5. WALK AND TURN LOGIC
        _fighter.TurnToFaceOpponent();
        
        Vector2 currentVelocity = _fighter.Velocity;
        float direction = 0;

        if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Left)) direction -= 1;
        if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Right)) direction += 1;
        
        if (direction == _fighter.FacingDirection)
        {
            _fighter.Anim.Play("walk_forward");
        }
        else if (direction == -_fighter.FacingDirection)
        {
            _fighter.Anim.Play("walk_backward");
        }
        else if (!_isLanding)
        {
            _fighter.Anim.Play("idle");
        }
        
        currentVelocity.X = direction * _fighter.Physics.WalkSpeed;
        
        if (!_fighter.IsOnFloor())
        {
            currentVelocity.Y += _fighter.Physics.Gravity * (float)delta;
        }
        
        _fighter.Velocity = currentVelocity;
        _fighter.Physics.ApplyMovementAndPush();
    }
}