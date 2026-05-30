namespace rdStrikeClone.States;

using Godot;

public class SpecialAttackState : BaseState
{
    private PackedScene _movePrefab;
    private NormalAttack _active;
    
    // Trackers for our flying moves (like the DP)
    private bool _isAirborneMove = false;
    private bool _hasLaunched = false;
    private float _currentActiveSpeed = 0.0f;

    // Optional: Make the startup invincible ONLY if it's a launching move (like a DP)
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
        
        // 1. Ask the prefab what kind of move it is!
        _isAirborneMove = _active.YAxisFrame> 0;
        _hasLaunched = false;
        
        _currentActiveSpeed = _active.XAxisSpeed;

        // Lock them to the ground during frame 1
        Vector2 vel = _fighter.Velocity;
        vel.X = 0; 
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 vel = _fighter.Velocity;
        int currentFrame = _active.GetCurrentFrame();
        
        bool speedJustChanged = false;
        if (_active.XSpeedProfile != null && _active.XSpeedProfile.Count > 0)
        {
            foreach (var keyframe in _active.XSpeedProfile)
            {
                if (keyframe.Frame == currentFrame)
                {
                    _currentActiveSpeed = keyframe.Speed;
                    speedJustChanged = true;
                    break;
                }
            }
        }
        // 2. THE LAUNCH CHECK (For DPs)
        if (_isAirborneMove && currentFrame == _active.YAxisFrame)
        {
            _hasLaunched = true;
            vel.Y = _active.YAxisSpeed; 
            // FIXED: Now uses the dynamic speed!
            vel.X = _fighter.FacingDirection * _currentActiveSpeed; 
        }
        
        // 3. APPLY HORIZONTAL MOVEMENT
        if (!_isAirborneMove) 
        {
            // GROUNDED: We removed the hardcoded "currentFrame <= 11". 
            // Now, you use your Godot Inspector keyframes to stop the character! (e.g., Frame 12, Speed 0)
            vel.X = _fighter.FacingDirection * _currentActiveSpeed;
        }
        else if (_hasLaunched && speedJustChanged)
        {
            // AIRBORNE IMPULSE: If they are flying, and a keyframe triggers, instantly force the new speed!
            vel.X = _fighter.FacingDirection * _currentActiveSpeed;
        }
        else if (!_hasLaunched)
        {
            // Startup frames before leaving the ground
            vel.X = 0;
        }

        // 4. APPLY GRAVITY & DRAG (FIXED: Only applies if actually flying!)
        if (_hasLaunched)
        {
            vel.Y += _fighter.Gravity * (float)delta;
            vel.X = Mathf.MoveToward(vel.X, 0, _active.AirDrag * (float)delta);
        }
        
        _fighter.Velocity = vel;
        _fighter.ApplyMovementAndPush();
        
        // 5. PROCESS THE ANIMATION
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

        // 6. THE LANDING 
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