namespace rdStrikeClone.States;

using Godot;

public class AttackState : BaseState
{
    protected NormalAttack _active;

    public AttackState(Fighter fighter, NormalAttack triggeredMove) : base(fighter)
    {
        _active = triggeredMove;
    }

    public override void Enter()
    {
        _active.Initialize(_fighter);
        
        Vector2 vel = _fighter.Velocity;
        vel.X = 0; 
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        bool isMoveFinished = _active.ProcessMove();
        bool isAirborneState = false; 
        
        _fighter.Velocity = _active.ProcessPhysics(_fighter.Velocity, _fighter.FacingDirection, delta, _fighter.Gravity, isAirborneState);
        
        bool isFalling = _fighter.Velocity.Y > 0; 
        
        _fighter.ApplyMovementAndPush();
        
        if (_active.HasYProfile && _active.HasLaunched)
        {
            if (isMoveFinished && _fighter.Anim.IsPlaying())
            {
                _fighter.Anim.Pause();
            }
            
            if (isFalling && _fighter.IsOnFloor())
            {
                _fighter.ChangeState(new IdleState(_fighter, true));
            }
            return; 
        }

        if (isMoveFinished)
        {
            _fighter.ChangeState(new IdleState(_fighter)); 
        }
    }

    public override void CheckForCancels()
    {
        if (_active.IsSpecialCancelable && _active.HasHit && _active.IsInsideCancelWindow())
        {
            NormalAttack triggeredMove = _fighter.Moves.EvaluateAvailableMoves(_fighter.Buffer, false);
            
            if (triggeredMove is SpecialAttack)
            {
                _fighter.ChangeState(new SpecialAttackState(_fighter, triggeredMove));
                return;
            }
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