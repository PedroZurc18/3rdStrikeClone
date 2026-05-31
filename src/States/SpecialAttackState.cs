namespace rdStrikeClone.States;

using Godot;

public class SpecialAttackState : BaseState
{
    private PackedScene _movePrefab;
    private NormalAttack _active;
    
    private bool _isAirborneMove = false;
    private bool _hasLaunched = false;
    private float _currentActiveXSpeed = 0.0f;

    public override bool IsInvincible => _isAirborneMove && !_hasLaunched;

    public SpecialAttackState(Fighter fighter, PackedScene movePrefab) : base(fighter)
    {
        _movePrefab = movePrefab;
    }

    public override void Enter()
    {
        _active = _movePrefab.Instantiate<NormalAttack>();
        _fighter.AttackContainer.AddChild(_active);
        _active.Initialize(_fighter);
        
        _isAirborneMove = _active.YSpeedProfile != null && _active.YSpeedProfile.Count > 0;
        _hasLaunched = false;

        Vector2 vel = _fighter.Velocity;
        vel.X = 0; 
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 vel = _fighter.Velocity;
        int currentFrame = _active.GetCurrentFrame();
        
        // X PROFILE
        bool xSpeedJustChanged = false;
        if (_active.XSpeedProfile != null && _active.XSpeedProfile.Count > 0)
        {
            foreach (var keyframe in _active.XSpeedProfile)
            {
                if (keyframe.Frame == currentFrame)
                {
                    _currentActiveXSpeed = keyframe.Speed;
                    xSpeedJustChanged = true;
                    break;
                }
            }
        }

        // Y PROFILE
        bool ySpeedJustChanged = false;
        if (_active.YSpeedProfile != null && _active.YSpeedProfile.Count > 0)
        {
            foreach (var keyframe in _active.YSpeedProfile)
            {
                if (keyframe.Frame == currentFrame)
                {
                    vel.Y = keyframe.Speed; 
                    ySpeedJustChanged = true;
                    
                    if (!_hasLaunched) _hasLaunched = true; 
                    break;
                }
            }
        }
        
        // 4. APPLY X MOVEMENT
        if (!_isAirborneMove) 
        {
            // Grounded Special (Fireball, etc.)
            vel.X = _fighter.FacingDirection * _currentActiveXSpeed;
        }
        else if (_hasLaunched && xSpeedJustChanged)
        {
            // Airborne Impulse (Air Dash, Divekick forward momentum)
            vel.X = _fighter.FacingDirection * _currentActiveXSpeed;
        }
        else if (!_hasLaunched)
        {
            // Startup frames before leaving the ground
            vel.X = 0;
        }

        // 5. APPLY GRAVITY & DRAG
        if (_hasLaunched)
        {
            if (!ySpeedJustChanged)
            {
                vel.Y += _fighter.Gravity * (float)delta;
            }
            
            vel.X = Mathf.MoveToward(vel.X, 0, _active.AirDrag * (float)delta);
        }
        
        _fighter.Velocity = vel;
        _fighter.ApplyMovementAndPush();
        
        // 6. PROCESS THE ANIMATION
        bool isMoveFinished = _active.ProcessMove();

        if (isMoveFinished)
        {
            if (!_isAirborneMove)
            {
                _fighter.ChangeState(new IdleState(_fighter));
                return;
            }
            else if (!_fighter.Anim.IsPlaying())
            {
                _fighter.Anim.Pause();
            }
        }

        // 7. THE LANDING 
        if (_isAirborneMove && _hasLaunched && vel.Y > 0 && _fighter.IsOnFloor())
        {
            _fighter.ChangeState(new IdleState(_fighter, true)); 
        }
    }

    public override void Exit()
    {
        if (_active != null)
        {
            _active.QueueFree();
        }
    }
}