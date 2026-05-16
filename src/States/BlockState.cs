namespace rdStrikeClone.States;

using Godot;

public class BlockState : BaseState
{
    private int _blockStunTimer;
    private float _pushback;
    private bool _isCrouching;
    public override bool CanBlock => true;

    public BlockState(Fighter fighter, int blockStun, float pushback, bool isCrouching)
        : base(fighter)
    {
        _blockStunTimer = blockStun;
        _pushback = pushback;
        _isCrouching = isCrouching;
    }

    public override void Enter()
    {
        _fighter.Anim.Stop();
        
        // Play the correct block animation based on their stance
        if (_isCrouching)
        {
            _fighter.Anim.Play("block_low");
        }
        else
        {
            _fighter.Anim.Play("block_mid");
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        _fighter.TurnToFaceOpponent();

        if (!_fighter.IsOnFloor())
        {
            Vector2 vel = _fighter.Velocity;
            vel.Y += _fighter.Gravity * (float)delta;
            _fighter.Velocity = vel;

        }
        _fighter.ApplyMovementAndPush();
        _blockStunTimer--;
        if (_blockStunTimer <= 0)
        {
            if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down))
            {
                _fighter.ChangeState(new CrouchState(_fighter, false));
            }
            else
            {
                _fighter.ChangeState(new IdleState(_fighter));
            }
        }
    }
}
