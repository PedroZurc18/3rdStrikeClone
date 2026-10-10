namespace rdStrikeClone.States;

using Godot;

public class ProximityGuardState : BaseState
{
    public override bool CanBlock => true;

    public ProximityGuardState(Fighter fighter) : base(fighter) { }

    public override void Enter()
    {
        _fighter.Velocity = new Vector2(0, _fighter.Velocity.Y);
    }

    public override void PhysicsUpdate(double delta)
    {
        _fighter.TurnToFaceOpponent();

        bool isHoldingBack = _fighter.Combat.IsHoldingBack();
        bool inProximity = _fighter.Combat.IsInProximity();
        bool isCrouching = _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down);

        
        if (!isHoldingBack || !inProximity)
        {
            if (isCrouching) _fighter.StateMachine.ChangeState(new CrouchState(_fighter, false));
            else _fighter.StateMachine.ChangeState(new IdleState(_fighter));
            return;
        }
        
        if (isCrouching) _fighter.Anim.Play("block_low");
        else _fighter.Anim.Play("block_mid");
        
        if (!_fighter.IsOnFloor())
        {
            Vector2 vel = _fighter.Velocity;
            vel.Y += _fighter.Physics.Gravity * (float)delta;
            _fighter.Velocity = vel;
        }
        else
        {
            Vector2 vel = _fighter.Velocity;
            vel.X = 0; 
            _fighter.Velocity = vel;
        }

        _fighter.Physics.ApplyMovementAndPush();
    }
}