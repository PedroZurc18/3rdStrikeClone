namespace rdStrikeClone.States;
using Godot;

public class IdleState : BaseState
{
    public override bool CanBlock => true;
    private bool _isLanding;
    private int _landingFrames = 13;
    private int _currentFrame = 0;

    public IdleState(Fighter fighter, bool isLanding = false) : base(fighter)
    {
        _isLanding = isLanding;
    }

    public override void Enter() {}

    public override void PhysicsUpdate(double delta)
    {
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
        
        if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Up))
        {
            _fighter.ChangeState(new AirState(_fighter));
            return;
        }
        
        if (CheckSpecialAttacks()) return;
        
        if (CheckStandingAttacks()) return; 
        
        if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down))
        {
            _fighter.ChangeState(new CrouchState(_fighter));
            return;
        }
        
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
        
        currentVelocity.X = direction * _fighter.WalkSpeed;
        
        // 3. Apply gravity
        if (!_fighter.IsOnFloor())
        {
            currentVelocity.Y += _fighter.Gravity * (float)delta;
        }
        
        _fighter.Velocity = currentVelocity;
        _fighter.ApplyMovementAndPush();
        // _fighter.MoveAndSlide();
    }
}