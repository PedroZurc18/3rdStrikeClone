using rdStrikeClone.Data;

namespace rdStrikeClone.States;

public class SpecialAttackState : AttackState
{
    public SpecialAttackState(Fighter fighter, AttackData triggeredMove) 
        : base(fighter, triggeredMove, isAirborne: false, isCrouching: false) { }
}