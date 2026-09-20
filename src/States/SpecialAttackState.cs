namespace rdStrikeClone.States;

using Godot;

public class SpecialAttackState : BaseState
{
    private NormalAttack _active;
    
    private bool _isAirborneMove = false;
    private bool _hasLaunched = false;
    private float _currentActiveXSpeed = 0.0f;

    public override bool IsInvincible => _isAirborneMove && !_hasLaunched;

    public SpecialAttackState(Fighter fighter, NormalAttack triggeredMove) : base(fighter)
    {
        _active = triggeredMove;
    }

    public override void Enter()
    {
        // 1. Wake up the pre-existing node from the MoveManager
        _active.Initialize(_fighter);
        
        // 2. Setup the profile variables
        _isAirborneMove = _active.YSpeedProfile != null && _active.YSpeedProfile.Count > 0;
        _hasLaunched = false;

        Vector2 vel = _fighter.Velocity;
        vel.X = 0; 
        _fighter.Velocity = vel;
    }

    public override void PhysicsUpdate(double delta)
    {
        // 1. Pass 'false' for grounded states. Pass 'true' if doing this in AirAttackState!
        bool isAirborneState = false; 

        // 2. The magic line: The attack handles ALL of its own movement logic now
        _fighter.Velocity = _active.ProcessPhysics(_fighter.Velocity, _fighter.FacingDirection, delta, _fighter.Gravity, isAirborneState);
        
        _fighter.ApplyMovementAndPush();
        
        // 3. Process the frame data and hitboxes
        bool isMoveFinished = _active.ProcessMove();

        // 4. Check for landing if the move was a launcher (like a DP)
        if (_active.HasYProfile && _active.HasLaunched && _fighter.Velocity.Y > 0 && _fighter.IsOnFloor())
        {
            _fighter.ChangeState(new IdleState(_fighter, true));
            return;
        }

        // 5. Standard finish
        if (isMoveFinished)
        {
            // If it's a crouching attack, change to CrouchState. Otherwise, IdleState.
            _fighter.ChangeState(new IdleState(_fighter)); 
        }
    }

    public override void Exit()
    {
        if (_active != null)
        {
            _active.SetBoxesActive(false);
        }
    }
}