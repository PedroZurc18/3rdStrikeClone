namespace rdStrikeClone.States;

using Godot;
using rdStrikeClone.Data;

public class BlockState : BaseState
{
    private int _blockStunTimer;
    private float _pushback;
    private bool _isCrouching;
    private bool _isProximityGuard;
    private bool _hasHitstopEnded;
    private int _flowFramesCounter;
    private int _targetFlowFrames = 4;
    
    public override bool CanBlock => true;
    
    public BlockState(Fighter fighter, bool isCrouching, int blockStun = 0, float pushback = 0f, AttackData.HitHeight blockedHitHeight = AttackData.HitHeight.Mid)
        : base(fighter)
    {
        _isCrouching = isCrouching;
        _blockStunTimer = blockStun;
        _pushback = pushback;
        
        _isProximityGuard = (blockStun <= 0); 
    }

    public override void Enter()
    {   
        _fighter.Anim.Stop();
        PlayBlockAnimation();
        
        Vector2 vel = _fighter.Velocity;
        vel.X = 0;
        _fighter.Velocity = vel;
        
        _hasHitstopEnded = false;
        _flowFramesCounter = 0;
    }

    private void PlayBlockAnimation()
    {
        if (_isCrouching) _fighter.Anim.Play("block_low");
        else _fighter.Anim.Play("block_mid");
        
        if (!_isProximityGuard)
        {
            float impactFrame = 4f; 
            _fighter.Anim.Seek(impactFrame/60.0f, true);
            
            _fighter.Anim.Pause();
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        // HITSTOP
        if (_fighter.Combat.HitStopTimer > 0) return;
        
        // SLIDE / GRAVITY
        _fighter.TurnToFaceOpponent();

        if (!_fighter.IsOnFloor())
        {
            Vector2 vel = _fighter.Velocity;
            vel.Y += _fighter.Physics.Gravity * (float)delta;
            _fighter.Velocity = vel;
        }

        _fighter.Physics.ApplyMovementAndPush();

        // PROXIMITY GUARD
        if (_isProximityGuard)
        {
            // ATTACK
            bool fireballExists = GodotObject.IsInstanceValid(_fighter.ActiveProjectile);
            AttackData triggeredMove = _fighter.Moves.EvaluateAvailableMoves(_fighter.Buffer, false);
            
            if (triggeredMove != null)
            {
                _fighter.StateMachine.ChangeState(new AttackState(_fighter, triggeredMove, false, _isCrouching));
                return;
            }

            // JUMP
            if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Up))
            {
                _fighter.StateMachine.ChangeState(new AirState(_fighter)); 
                return;
            }

            // PROXIMITY LOGIC
            bool isHoldingBack = _fighter.Combat.IsHoldingBack();
            bool inProximity = _fighter.Combat.IsInProximity();
            bool wantsToCrouch = _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down);
            
            if (!isHoldingBack || !inProximity)
            {
                if (wantsToCrouch) _fighter.StateMachine.ChangeState(new CrouchState(_fighter, false));
                else _fighter.StateMachine.ChangeState(new IdleState(_fighter));
                return;
            }

            //  SWAPING HIGH/LOW GUARD
            if (wantsToCrouch != _isCrouching)
            {
                _isCrouching = wantsToCrouch;
                PlayBlockAnimation();
            }
        }
        // ACTIVE BLOCKSTUN
        else 
        {
            if (!_hasHitstopEnded)
            {
                _hasHitstopEnded = true;
                _fighter.Anim.Play(); 
            }
            else if (_flowFramesCounter < _targetFlowFrames)
            {
                _flowFramesCounter++;
                if (_flowFramesCounter >= _targetFlowFrames)
                {
                    _fighter.Anim.Pause();
                }
            }
            
            int recoveryFrames = 7;
            _blockStunTimer--;
            
            if (_blockStunTimer <= recoveryFrames && !_fighter.Anim.IsPlaying())
            {
                _fighter.Anim.Play();
            }
            
            if (_blockStunTimer <= 0)
            {
                if (_fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down))
                {
                    _fighter.StateMachine.ChangeState(new CrouchState(_fighter, false));
                }
                else
                {
                    _fighter.StateMachine.ChangeState(new IdleState(_fighter));
                }
            }
        }
    }
}