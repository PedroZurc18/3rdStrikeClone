using Godot;
using rdStrikeClone.States;

namespace rdStrikeClone.Components;

public partial class HitboxManager : Area2D
{
    private Fighter _fighter;
    public bool HasHit => _hasHit;
    private bool _hasHit = false;

    public override void _Ready()
    {
        _fighter = Owner as Fighter; 
        AreaEntered += OnHitboxConnected;
    }

    public void ResetHit() => _hasHit = false;

    private void OnHitboxConnected(Area2D area)
    {
        if (_hasHit) return;

        Node current = area;
        while (current != null && !(current is Fighter))
        {
            current = current.GetParent();
        }
        
        if (current is Fighter hitFighter && hitFighter == _fighter.Opponent)
        {
            if (hitFighter.StateMachine.CurrentState != null && hitFighter.StateMachine.CurrentState.IsInvincible) return; 

            if (_fighter.StateMachine.CurrentState is AttackState attackState)
            {
                _hasHit = true;
                _fighter.SfxPlayer.Stop();
                
                bool wasParried = hitFighter.Combat.ReceiveHit(attackState.AttackData, attackState.AttackData.HitStats);
                
                if (wasParried)
                {
                    _fighter.Combat.ApplyHitStop(16, true);
                    hitFighter.Combat.ApplyHitStop(16, false);
                }
                else
                {
                    _fighter.Combat.ApplyHitStop(attackState.AttackData.HitStats.HitStopFrames, true);
                    hitFighter.Combat.ApplyHitStop(attackState.AttackData.HitStats.HitStopFrames, false); 
                }
            }
        }
    }
}