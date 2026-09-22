using rdStrikeClone.Data;

namespace rdStrikeClone.States;

public class AirAttackState : AttackState
{
    public AirAttackState(Fighter fighter, AttackData triggeredMove) 
        : base(fighter, triggeredMove, isAirborne: true, isCrouching: false) { }
}