namespace rdStrikeClone.States;

using Godot;

public class SpecialAttackState : BaseState
{
    private PackedScene _movePrefab;
    private NormalAttack _active;
    
    // Trackers for our flying moves (like the DP)
    private bool _isAirborneMove = false;
    private bool _hasLaunched = false;

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

        // Lock them to the ground during frame 1
        Vector2 vel = _fighter.Velocity;
        vel.X = 0; 
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 vel = _fighter.Velocity;
        int currentFrame = _active.GetCurrentFrame();

        // 2. THE LAUNCH CHECK (For DPs)
        if (_isAirborneMove && currentFrame == _active.YAxisFrame)
        {
            _hasLaunched = true;
            vel.Y = _active.YAxisSpeed; 
            vel.X = _fighter.FacingDirection * _active.XAxisSpeed; 
        }
        // 3. GROUNDED MOVEMENT (For QCF / Fireballs)
        else if (!_isAirborneMove && currentFrame <= 11) // Use whatever frame limit you prefer
        {
            vel.X = _fighter.FacingDirection * _active.XAxisSpeed;
        }
        else if (!_hasLaunched)
        {
            vel.X = 0;
        }

        // 4. APPLY GRAVITY (Only if we are actually flying)
        if (_hasLaunched)
        {
            vel.Y += _fighter.Gravity * (float)delta;
        }
        float activeDrag = _active.AirDrag;
        
        vel.X = Mathf.MoveToward(vel.X, 0, activeDrag * (float)delta);
        
        _fighter.Velocity = vel;
        _fighter.ApplyMovementAndPush();
        
        // 5. PROCESS THE ANIMATION
        bool isMoveFinished = _active.ProcessMove();

        if (isMoveFinished)
        {
            // If it's a grounded special, go straight back to Idle!
            if (!_isAirborneMove)
            {
                _fighter.ChangeState(new IdleState(_fighter));
                return;
            }
            // If it's a flying special, freeze the last frame until we hit the floor
            else if (!_fighter.Anim.IsPlaying())
            {
                _fighter.Anim.Pause();
            }
        }

        // 6. THE LANDING (Only for flying specials)
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