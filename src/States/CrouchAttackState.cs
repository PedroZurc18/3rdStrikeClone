namespace rdStrikeClone.States;

using Godot;

public class CrouchAttackState : BaseState
{
    private NormalAttack _active;

    public CrouchAttackState(Fighter fighter, NormalAttack triggeredMove) : base(fighter) 
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
        _fighter.TurnToFaceOpponent();

        Vector2 vel = _fighter.Velocity;
        if (!_fighter.IsOnFloor())
        {
            vel.Y += _fighter.Gravity * (float)delta;
        }
        
        vel.X = 0;
        
        _fighter.ApplyMovementAndPush();
        
        bool isMoveFinished = _active.ProcessMove();
        
        if (isMoveFinished)
        {
            _fighter.ChangeState(new CrouchState(_fighter, false));
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