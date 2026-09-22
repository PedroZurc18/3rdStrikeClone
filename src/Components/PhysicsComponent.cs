using Godot;

namespace rdStrikeClone.Components;

public partial class PhysicsComponent : Node
{
    [ExportGroup("Physics Stats")]
    [Export] public float WalkSpeed = 500.0f;
    [Export] public float JumpForce = -2000.0f;
    [Export] public float Gravity = 5500.0f;
    [Export] public float CoreOverlapLimit = 25.0f;

    // --- Pushbox ---
    private float _maxPushboxForce = 15.0f;
    private float _minPushboxForce = 2.0f;
    private float _pushboxWidth = 80.0f;

    // --- Pushback ---
    private bool _isPushed;
    private float _pushSpeed;
    private const float PushDecel = 1500.0f;

    private Fighter _fighter;

    public override void _Ready()
    {
        _fighter = GetParent<Fighter>();
    }

    public void Tick(double delta)
    {
        if (_isPushed)
        {
            _pushSpeed = Mathf.MoveToward(_pushSpeed, 0, PushDecel * (float)delta);
            
            if (Mathf.IsZeroApprox(_pushSpeed))
            {
                _isPushed = false;
            }
        }
    }

    public void ApplyUniversalPushback(float force, float direction)
    {
        _isPushed = true;
        _pushSpeed = force * direction;
    }

    public void ApplyMovementAndPush()
    {
        if (_fighter.Opponent == null)
        {
            _fighter.MoveAndSlide();
            return;
        }
        
        bool wasOnLeft = _fighter.GlobalPosition.X <= _fighter.Opponent.GlobalPosition.X;
        Vector2 originalVel = _fighter.Velocity;
        
        if (_isPushed)
        {
            _fighter.Velocity = new Vector2(originalVel.X + _pushSpeed, originalVel.Y);
        }
        
        _fighter.MoveAndSlide();
        KineticBounce();
        
        if (_isPushed)
        {
            _fighter.Velocity = originalVel;
        }
        
        CrossoverLock(wasOnLeft);
        Pushboxes(wasOnLeft);
    }

    private void KineticBounce()
    {
        if (_isPushed && _fighter.IsOnWall())
        {
            float remainingSpeed = _pushSpeed;
            _isPushed = false;
            _pushSpeed = 0;

            _fighter.Opponent.Physics.ApplyUniversalPushback(Mathf.Abs(remainingSpeed), -Mathf.Sign(remainingSpeed));
        }
    }

    private void CrossoverLock(bool wasOnLeft)
    {
        if (_fighter.IsOnFloor() && _fighter.Opponent.IsOnFloor())
        {
            bool isNowOnLeft = _fighter.GlobalPosition.X <= _fighter.Opponent.GlobalPosition.X;
            
            if (wasOnLeft != isNowOnLeft)
            {
                Vector2 fixedPos = _fighter.GlobalPosition;
                fixedPos.X = _fighter.Opponent.GlobalPosition.X;
                _fighter.GlobalPosition = fixedPos;
                
                Vector2 vel = _fighter.Velocity;
                vel.X = 0;
                _fighter.Velocity = vel;
            }
        }
    }

    private void Pushboxes(bool wasOnLeft)
    {
        if (_fighter.Pushbox != null && _fighter.Opponent.Pushbox != null)
        {
            if (_fighter.Pushbox.GetOverlappingAreas().Contains(_fighter.Opponent.Pushbox))
            {
                float distance = Mathf.Abs(_fighter.GlobalPosition.X - _fighter.Opponent.GlobalPosition.X);
                float pushDirection = 0;
                
                if (_fighter.IsOnFloor() && _fighter.Opponent.IsOnFloor() && distance < CoreOverlapLimit)
                {
                    float separation = CoreOverlapLimit - distance;
                    pushDirection = wasOnLeft ? -separation : separation;
                }
                else
                {
                    float overlapRatio = 1.0f - (distance / _pushboxWidth);
                    overlapRatio = Mathf.Clamp(overlapRatio, 0.0f, 1.0f);
                    float currentForce = Mathf.Lerp(_minPushboxForce, _maxPushboxForce, overlapRatio);

                    if (_fighter.GlobalPosition.X < _fighter.Opponent.GlobalPosition.X)
                        pushDirection = -currentForce;
                    else if (_fighter.GlobalPosition.X > _fighter.Opponent.GlobalPosition.X)
                        pushDirection = currentForce;
                    else
                        pushDirection = wasOnLeft ? -currentForce : currentForce;
                }
                
                _fighter.MoveAndCollide(new Vector2(pushDirection, 0));
            }
        }
    }
}