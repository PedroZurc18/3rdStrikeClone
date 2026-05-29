namespace rdStrikeClone.States;
using Godot;

public abstract class BaseState
{
    protected Fighter _fighter;
    
    public virtual bool CanBlock => false;
    public virtual bool IsInvincible => false;

    public BaseState(Fighter fighter)
    {
        _fighter = fighter;
    }

    public virtual void Enter() { }
    public virtual void PhysicsUpdate(double delta) { }
    
    public virtual void Exit() { } 
    
    
    protected bool CheckSpecialAttacks()
    {
        int[] qcf = { 2, 3, 6 };
        int[] dp = { 6, 2, 3 };
        
        if (_fighter.Buffer.CheckMotion(dp, InputBuffer.InputFlag.Kick, _fighter.FacingDirection, 18))
        {
            if (_fighter.aaPrefab != null)
            {
                _fighter.ChangeState(new SpecialAttackState(_fighter, _fighter.aaPrefab));
                return true;
            }
        }
        
        if (_fighter.Buffer.CheckMotion(qcf, InputBuffer.InputFlag.Kick, _fighter.FacingDirection, 30))
        {
            if (_fighter.qcfPrefab != null)
            {
                _fighter.ChangeState(new SpecialAttackState(_fighter, _fighter.qcfPrefab));
                return true;
            }
        }
        return false;
    }

    protected bool CheckStandingAttacks()
    {
        if (_fighter.Buffer.WasInputPressedWithin(InputBuffer.InputFlag.Punch, 8))
        {
            // Pass the prefab instead of the data card
            _fighter.ChangeState(new AttackState(_fighter, _fighter.sMpPrefab));
            return true;
        }
        
        return false; 
    }
    
    protected bool CheckCrouchingAttacks()
    {
        if (_fighter.Buffer.WasInputPressedWithin(InputBuffer.InputFlag.Kick, 8))
        {
            // Pass the prefab to the CrouchAttackState
            _fighter.ChangeState(new CrouchAttackState(_fighter, _fighter.cMkPrefab));
            return true;
        }
        
        return false;
    }
    
    public virtual void CheckForCancels()
     {
         
     }
    
}