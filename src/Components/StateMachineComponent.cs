using Godot;
using rdStrikeClone.States;

namespace rdStrikeClone.Components;

public partial class StateMachineComponent : Node
{
    public BaseState CurrentState { get; private set; }
    
    private Fighter _fighter;

    public override void _Ready()
    {
        _fighter = GetParent<Fighter>();
    }

    public void ChangeState(BaseState newState)
    {
        if (CurrentState != null)
            CurrentState.Exit();
            
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Tick(double delta)
    {
        if (CurrentState != null)
        {
            CurrentState.PhysicsUpdate(delta);
            CurrentState.CheckForCancels();
        }
    }
}