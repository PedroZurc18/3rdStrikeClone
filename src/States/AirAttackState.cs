namespace rdStrikeClone.States;

using Godot;

public class AirAttackState : BaseState
{
    private PackedScene _movePrefab;
    private NormalAttack _active;
    private bool _isMoveFinished = false;

    // We pass the prefab in just like the grounded AttackState
    public AirAttackState(Fighter fighter, PackedScene movePrefab)
        : base(fighter)
    {
        _movePrefab = movePrefab;
    }

    public override void Enter()
    {
        // Spawn the attack prefab
        _active = _movePrefab.Instantiate<NormalAttack>();
        _fighter.AttackContainer.AddChild(_active);
        _active.Initialize(_fighter);
        _isMoveFinished = false;
        
        // Notice we do NOT set vel.X = 0 here! 
        // We let them keep their jumping momentum.
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 vel = _fighter.Velocity;

        // 1. Always apply gravity so they keep falling
        vel.Y += _fighter.Gravity * (float)delta;
        _fighter.Velocity = vel;

        // 2. Move the character
        _fighter.ApplyMovementAndPush();

        // 3. THE LANDING INTERRUPT
        if (_fighter.IsOnFloor())
        {
            // The millisecond their feet touch the ground, the attack state is destroyed.
            _fighter.ChangeState(new IdleState(_fighter, true));
            return;
        }

        // 4. Process the attack frame data
        if (!_isMoveFinished)
        {
            // UpdateBoxes is handled internally by your awesome NormalAttack class
            _isMoveFinished = _active.ProcessMove();
            
            // Note: If _isMoveFinished becomes true, we do NOT change state.
            // In traditional fighting games, if a jump kick finishes early, 
            // the character holds the final frame of the kick all the way to the ground!
        }
    }

    public override void CheckForCancels()
    {
        // Allow for special cancels in the air (e.g., Air Fireball or Divekick)
        if (_active != null && _active.IsSpecialCancelable && _active.HasHit && _active.IsInsideCancelWindow())
        {
            if (CheckSpecialAttacks())
                return;
        }
    }

    public override void Exit()
    {
        // 5. THE CLEANUP
        // Whether they landed or got hit out of the air, deleting the prefab 
        // guarantees no phantom hitboxes are left behind.
        if (_active != null)
        {
            _active.QueueFree();
        }
    }
}