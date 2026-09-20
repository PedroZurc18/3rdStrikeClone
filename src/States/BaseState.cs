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
    
    public virtual void CheckForCancels()
     {
         
     }
    
}