namespace rdStrikeClone.States;

using Godot;

public class CrouchAttackState : BaseState
{
    private PackedScene _movePrefab;
    private NormalAttack _active;

    // REQUIRES the PackedScene to spawn
    public CrouchAttackState(Fighter fighter, PackedScene movePrefab) : base(fighter) 
    { 
        _movePrefab = movePrefab;
    }

    public override void Enter()
    {
        _active = _movePrefab.Instantiate<NormalAttack>();
        _fighter.AttackContainer.AddChild(_active);
        
        // 3. Initialize it with frame data
        _active.Initialize(_fighter);
        
        Vector2 vel = _fighter.Velocity;
        vel.X = 0; 
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        _fighter.TurnToFaceOpponent();

        Vector2 vel = _fighter.Velocity;
        if (!_fighter.IsOnFloor())
        {
            vel.Y += _fighter.Gravity * (float)delta;
        }
        
        // vel.X = _fighter.FacingDirection * _active.ForwardSpeed;
        // _fighter.Velocity = vel;
        vel.X = 0;
        
        _fighter.ApplyMovementAndPush();
        
        bool isMoveFinished = _active.ProcessMove();
        
        if (isMoveFinished)
        {
            _fighter.ChangeState(new CrouchState(_fighter, false));
        }
    }
    
    public override void CheckForCancels()
    {
        if (_active.IsSpecialCancelable && _active.HasHit && _active.IsInsideCancelWindow())
        {
            if (CheckSpecialAttacks()) return;
        }
    }

    public override void Exit()
    {
        // 5. DESTROY the move and its hitboxes when leaving the state!
        if (_active != null)
        {
            _active.QueueFree();
        }
    }
}