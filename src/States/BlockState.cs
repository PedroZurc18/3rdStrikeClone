namespace rdStrikeClone.States;

using Godot;

public class BlockState : BaseState
{
    private int _blockStunTimer;
    private float _pushback;
    private bool _isCrouching;
    public override bool CanBlock => true;

    public BlockState(Fighter fighter, int blockStun, float pushback, bool isCrouching) : base(fighter) 
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
        
        // Apply pushback force (pushing them away from the opponent)
        Vector2 vel = _fighter.Velocity;
        int pushDirection = _fighter.Opponent.GlobalPosition.X > _fighter.GlobalPosition.X ? -1 : 1;
        vel.X = pushDirection * _pushback; 
        _fighter.Velocity = vel;
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

        // Use your awesome collision slider so they push the attacker back if trapped in corner!
        _fighter.ApplyMovementAndPush();

        // Tick down the block stun
        _blockStunTimer--;

        if (_blockStunTimer <= 0)
        {
            // When stun ends, check if they are still holding down to return to the correct idle state
            if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down))
            {
                // Return to crouch, but pass 'false' to skip the crouch_in animation!
                _fighter.ChangeState(new CrouchState(_fighter, false)); 
            }
            else
            {
                _fighter.ChangeState(new IdleState(_fighter));
            }
        }
    }
}