using rdStrikeClone.Data;

namespace rdStrikeClone.States;

using Godot;

public class ParryState : BaseState
{
    private AttackData.HitHeight _parryHeight;
    private bool _isAirborne;
    
    public override bool CanBlock => true; 

    public ParryState(Fighter fighter, AttackData.HitHeight parryHeight, bool isAirborne)
        : base(fighter)
    {
        _parryHeight = parryHeight;
        _isAirborne = isAirborne;
    }

    public override void Enter()
    {
        _fighter.Anim.Stop();
        
        if (_isAirborne)
        {
            _fighter.Anim.Play("jump_parry");
        }
        else if (_parryHeight == AttackData.HitHeight.Low)
        {
            _fighter.Anim.Play("crouch_parry");
        }
        else
        {
            _fighter.Anim.Play("stand_parry");
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        if (!_fighter.Anim.IsPlaying())
        {
            if (!_isAirborne && _parryHeight == AttackData.HitHeight.Low)
            {
                _fighter.StateMachine.ChangeState(new CrouchState(_fighter, false));
            }
            else
            {
                _fighter.StateMachine.ChangeState(new IdleState(_fighter, _isAirborne));
            }
        }
    }

    public override void CheckForCancels()
    {
        AttackData triggeredMove = _fighter.Moves.EvaluateAvailableMoves(_fighter.Buffer, _isAirborne);
        
        if (triggeredMove != null)
        {
            if (_isAirborne)
            {
                _fighter.StateMachine.ChangeState(new AirAttackState(_fighter, triggeredMove));
            }
            else
            {
                _fighter.StateMachine.ChangeState(new AttackState(_fighter, triggeredMove));
            }
            return;
        }
    }
}