using rdStrikeClone.Data;

namespace rdStrikeClone.States;

using Godot;

public class HitState : BaseState
{
    private int _hitstun;
    private float _pull;
    private AttackData.HitHeight _height;
    private AttackData.AttackStrength _strength;
    private bool _hasReachedImpactPose;

    public HitState(
        Fighter fighter,
        int stunFrames,
        float pushback,
        float pull,
        AttackData.AttackStrength strength,
        AttackData.HitHeight height
    ) 
        : base(fighter) 
    {
        _hitstun = stunFrames;
        _pull = pull;
        _height = height;
        _strength = strength;
    }

    public override void Enter()
    {
        _fighter.Anim.Stop();
            
        string height = (_height == AttackData.HitHeight.Low) ? "low" : "low";
        string strength = _strength.ToString().ToLower();
        string animation = $"hit_stand_medium_{height}";
        
        _fighter.Anim.Play(animation);
        _fighter.Anim.Seek(0, true); 
        _fighter.Anim.Pause(); 

        _hasReachedImpactPose = false;
        
        Vector2 vel = _fighter.Velocity;
        vel.X = - _pull; 
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {   
        // HITSTOP
        if (_fighter.Combat.HitStopTimer > 0) return;
        
        // IMPACT POSE
        if (!_hasReachedImpactPose)
        {
            _hasReachedImpactPose = true;
            
            float impactFrame = 2.0f; 
            _fighter.Anim.Seek(impactFrame / 60.0f, true);
            
            _fighter.Anim.Pause(); 
        }
        
        _fighter.TurnToFaceOpponent();
        
        // SLIDING / GRAVITY
        Vector2 vel = _fighter.Velocity;
        
        vel.X = Mathf.MoveToward(vel.X, 0, 1500 * (float)delta);
        
        if (!_fighter.IsOnFloor())
        {
            vel.Y += _fighter.Physics.Gravity * (float)delta;
        }
        
        _fighter.Velocity = vel;
        _fighter.Physics.ApplyMovementAndPush();
        
        // DYNAMIC ANIMATION
        int hitRecoveryFrames = 17;
        
        if (_hitstun <= hitRecoveryFrames && !_fighter.Anim.IsPlaying())
        {
            _fighter.Anim.Play();
        }
        
        // HITSTUN
        _hitstun--;
        if (_hitstun <= 0)
        { 
            _fighter.StateMachine.ChangeState(new IdleState(_fighter));
        }
    }
}
