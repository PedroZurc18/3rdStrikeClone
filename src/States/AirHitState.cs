namespace rdStrikeClone.States;

using Godot;

public class AirHitState : BaseState
{
    public const float AIR_HIT_DECELERATION = 0f;
    public const int HARD_KNOCKDOWN_DURATION_FRAMES = 60;
    
    private float _initialVelX;
    private float _initialVelY;
    private bool _isJuggle;
    
    public override bool IsInvincible => !_isJuggle;

    public AirHitState(Fighter fighter, int stunFrames, float initialVelX, float initialVelY, bool isJuggle)
        : base(fighter)
    {
        _initialVelX = initialVelX;
        _initialVelY = initialVelY;
        _isJuggle = isJuggle;
    }

    public override void Enter()
    {
        Vector2 vel = _fighter.Velocity;
        vel.X = _initialVelX;
        vel.Y = _initialVelY;
        _fighter.Velocity = vel;
        
        _fighter.Anim.Stop();

        _fighter.Anim.Play(_isJuggle ? "juggle_air" : "hit_air");
    }

    public override void PhysicsUpdate(double delta)
    {
        if (_fighter.Combat.HitStopTimer > 0) return;
        
        _fighter.Anim.Play();
        
        Vector2 vel = _fighter.Velocity;

        // GRAVITY
        vel.Y += _fighter.Physics.Gravity * (float)delta;

        // HORIZONTAL DECEL
        vel.X = Mathf.MoveToward(vel.X, 0, AIR_HIT_DECELERATION * (float)delta);
        
        _fighter.Velocity = vel;
        _fighter.Physics.ApplyMovementAndPush();

        // WAIT BEFORE TOUCHING GROUND
        if (!_fighter.Anim.IsPlaying())
        {
            _fighter.Anim.Pause();
        }
        
        if (_fighter.IsOnFloor() && vel.Y > 0) 
        {
            if (_isJuggle)
            {
                _fighter.StateMachine.ChangeState(new HardKnockdownState(_fighter, HARD_KNOCKDOWN_DURATION_FRAMES));
            }
            else 
            {
                // SOFT KNOCKDOWN
                _fighter.StateMachine.ChangeState(new IdleState(_fighter, true)); 
            }
        }
    }

    public override void Exit()
    {

    }
}
