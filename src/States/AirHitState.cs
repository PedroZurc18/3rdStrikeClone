namespace rdStrikeClone.States;

using Godot;

public class AirHitState : BaseState
{
    // Constants for air hit parameters
    public const float AIR_HIT_DECELERATION = 0f;
    public const int HARD_KNOCKDOWN_DURATION_FRAMES = 45; // From user request
    
    private float _initialVelX;
    private float _initialVelY;
    private bool _isHardKnockdown;
    
    public override bool IsInvincible => !_isHardKnockdown;

    public AirHitState(Fighter fighter, int stunFrames, float initialVelX, float initialVelY, bool isHardKnockdown)
        : base(fighter)
    {
        _initialVelX = initialVelX;
        _initialVelY = initialVelY;
        _isHardKnockdown = isHardKnockdown;
    }

    public override void Enter()
    {
        Vector2 vel = _fighter.Velocity;
        vel.X = _initialVelX;
        vel.Y = _initialVelY;
        _fighter.Velocity = vel;

        _fighter.Anim.Play(_isHardKnockdown ? "juggle_air" : "hit_air"); // Placeholder animations
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 vel = _fighter.Velocity;

        // 1. Apply gravity so they constantly fall
        vel.Y += _fighter.Gravity * (float)delta;

        // 2. Decelerate horizontal pushback blast
        vel.X = Mathf.MoveToward(vel.X, 0, AIR_HIT_DECELERATION * (float)delta);
        
        _fighter.Velocity = vel;
        _fighter.ApplyMovementAndPush();

        // 3. Freeze the animation on its final frame so they don't awkwardly return to idle mid-air
        if (!_fighter.Anim.IsPlaying())
        {
            _fighter.Anim.Pause();
        }

        // 4. THE ONLY WAY OUT: Touching the floor!
        if (_fighter.IsOnFloor() && vel.Y > 0) 
        {
            if (_isHardKnockdown)
            {
                _fighter.ChangeState(new HardKnockdownState(_fighter, HARD_KNOCKDOWN_DURATION_FRAMES));
            }
            else 
            {
                // Soft knockdown / standard landing
                _fighter.ChangeState(new IdleState(_fighter, true)); 
            }
        }
    }

    public override void Exit()
    {
        // Ensure horizontal velocity is handled by the next state, if transitioning out before landing
        // If landing, the landing state will handle it.
        // If transitioning to AirState, AirState will apply gravity/input movement.
    }
}
