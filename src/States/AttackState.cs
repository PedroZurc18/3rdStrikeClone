namespace rdStrikeClone.States;

using Godot;

public class AttackState : BaseState
{
    private PackedScene _movePrefab;
    private NormalAttack _active;

    public AttackState(Fighter fighter, PackedScene movePrefab)
        : base(fighter)
    {
        _movePrefab = movePrefab;
    }

    public override void Enter()
    {
        _active = _movePrefab.Instantiate<NormalAttack>();
        _fighter.AttackContainer.AddChild(_active);
        _active.Initialize(_fighter);
        
        Vector2 vel = _fighter.Velocity;
        vel.X = 0; 
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 vel = _fighter.Velocity;
        if (!_fighter.IsOnFloor())
        {
            vel.Y += _fighter.Gravity * (float)delta;
        }

        if (_active.GetCurrentFrame() <= 11)
        {
            vel.X = _fighter.FacingDirection * _active.ForwardSpeed;
        }
        else
        {
            vel.X = 0;
        }
        _fighter.Velocity = vel;

        _fighter.ApplyMovementAndPush();
        
        bool isMoveFinished = _active.ProcessMove();

        if (isMoveFinished)
        {
            _fighter.ChangeState(new IdleState(_fighter));
        }
    }

    public override void CheckForCancels()
    {
        if (_active.IsSpecialCancelable && _active.HasHit && _active.IsInsideCancelWindow())
        {
            if (CheckSpecialAttacks())
                return;
        }
    }

    public override void Exit()
    {
        if (_active != null)
        {
            _active.QueueFree();
        }
    }
}