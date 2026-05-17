namespace rdStrikeClone.States;

using Godot;

public class HitState : BaseState
{
    private int _stunTimer = 0;
    private int _stunDurationFrames = 10;
    private float _pull;
    private NormalAttack.HitHeight _height;
    private NormalAttack.AttackStrength _strength;

    public HitState(
        Fighter fighter,
        int stunFrames,
        float pushback, // Keep signature for compatibility
        float pull,
        NormalAttack.AttackStrength strength,
        NormalAttack.HitHeight height
    ) 
        : base(fighter) 
    {
        _stunDurationFrames = stunFrames;
        _pull = pull;
        _height = height;
        _strength = strength;
    }

    public override void Enter()
    {
        _fighter.Anim.Stop();
            
        string height = (_height == NormalAttack.HitHeight.Low) ? "low" : "low";
        string strength = _strength.ToString().ToLower();
        string animation = $"hit_stand_medium_{height}";
        _fighter.Anim.Play(animation);
        
        Vector2 vel = _fighter.Velocity;

        vel.X = - _pull; // Only keep the pull, pushback is handled by Fighter
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 vel = _fighter.Velocity;
        
        vel.X = Mathf.MoveToward(vel.X, 0, 1500 * (float)delta);
        
        if (!_fighter.IsOnFloor())
        {
            vel.Y += _fighter.Gravity * (float)delta;
        }

        _fighter.Velocity = vel;
        _fighter.ApplyMovementAndPush();
        
        _stunTimer++;
        if (_stunTimer >= _stunDurationFrames)
        {
            _fighter.ChangeState(new IdleState(_fighter));
        }
    }
}
