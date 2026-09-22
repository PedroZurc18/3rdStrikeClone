using rdStrikeClone.Data;

namespace rdStrikeClone.States;

public class CrouchAttackState : AttackState
{
    public CrouchAttackState(Fighter fighter, AttackData triggeredMove) 
        : base(fighter, triggeredMove, isAirborne: false, isCrouching: true) { }
}