namespace rdStrikeClone.States;

using Godot;

public class ParryState : BaseState
{
    private NormalAttack.HitHeight _parryHeight;
    private bool _isAirborne;
    
    public override bool CanBlock => true; 

    public ParryState(Fighter fighter, NormalAttack.HitHeight parryHeight, bool isAirborne)
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
        else if (_parryHeight == NormalAttack.HitHeight.Low)
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
            if (!_isAirborne && _parryHeight == NormalAttack.HitHeight.Low)
            {
                _fighter.ChangeState(new CrouchState(_fighter, false));
            }
            else
            {
                _fighter.ChangeState(new IdleState(_fighter, _isAirborne));
            }
        }
    }

    public override void CheckForCancels()
    {
        if (CheckSpecialAttacks() || CheckStandingAttacks() || CheckCrouchingAttacks())
        {
            return;
        }
    }
}