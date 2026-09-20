namespace rdStrikeClone.States;

using Godot;

public class AirAttackState : BaseState
{
    private NormalAttack _active;
    private bool _isMoveFinished = false;

    public AirAttackState(Fighter fighter, NormalAttack triggeredMove) : base(fighter)
    {
        _active = triggeredMove;
    }

    public override void Enter()
    {
        _active.Initialize(_fighter);
        _isMoveFinished = false;
    }

    public override void PhysicsUpdate(double delta)
    {
        bool isAirborneState = true; 
        
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