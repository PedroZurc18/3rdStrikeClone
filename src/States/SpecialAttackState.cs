namespace rdStrikeClone.States;

using Godot;

public class SpecialAttackState : BaseState
{
    private NormalAttack _active;
    
    private bool _isAirborneMove = false;
    private bool _hasLaunched = false;
    private float _currentActiveXSpeed = 0.0f;

    public override bool IsInvincible => _isAirborneMove && !_hasLaunched;

    public SpecialAttackState(Fighter fighter, NormalAttack triggeredMove) : base(fighter)
    {
        _active = triggeredMove;
    }

    public override void Enter()
    {
        // 1. Wake up the pre-existing node from the MoveManager
        _active.Initialize(_fighter);
        
        // 2. Setup the profile variables
        _isAirborneMove = _active.YSpeedProfile != null && _active.YSpeedProfile.Count > 0;
        _hasLaunched = false;

        Vector2 vel = _fighter.Velocity;
        vel.X = 0; 
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        bool isAirborneState = false; 

        
        _fighter.Velocity = _active.ProcessPhysics(_fighter.Velocity, _fighter.FacingDirection, delta, _fighter.Gravity, isAirborneState);
        
        _fighter.ApplyMovementAndPush();
        
        bool isMoveFinished = _active.ProcessMove();
        
        if (_active.HasYProfile && _active.HasLaunched && _fighter.Velocity.Y > 0 && _fighter.IsOnFloor())
        {
            _fighter.ChangeState(new IdleState(_fighter, true));
            return;
        }
        
        if (isMoveFinished)
        {
            _fighter.ChangeState(new IdleState(_fighter)); 
        }
    }

    public override void Exit()
    {
        if (_active != null)
        {
            _active.SetBoxesActive(false);
        }
    }
}