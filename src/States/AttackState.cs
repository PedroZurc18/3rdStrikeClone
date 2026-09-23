using rdStrikeClone.Components;

namespace rdStrikeClone.States;

using Godot;
using rdStrikeClone.Data;

public class AttackState : BaseState
{
    // Make this public so HitboxManager can read it
    public AttackData AttackData => _data;
    public HitboxFrameData CurrentHitStats { get; private set; }
    
    protected AttackData _data;
    protected bool _isAirborneState;
    protected bool _isCrouchingState;

    private int _currentHitIndex = -1;
    private int _currentFrame = 0;
    private AttackData _bufferedCancel = null;
    private bool _launcherHanging = false;
    private bool _hasLaunched = false;
    private float _currentXSpeed = 0f;

    public AttackState(Fighter fighter, AttackData triggeredMove, bool isAirborne = false, bool isCrouching = false) : base(fighter)
    {
        _data = triggeredMove;
        _isAirborneState = isAirborne;
        _isCrouchingState = isCrouching;
    }

    public override void Enter()
    {
        _currentFrame = 0;
        _hasLaunched = false;
        _launcherHanging = false;
        _bufferedCancel = null;
        _currentXSpeed = 0f;
        
        _fighter.HitManager.ResetHit();

        _fighter.Anim.Stop();
        _fighter.Anim.Play(_data.AnimationName);

        if (_data.VoiceSound != null)
        {
            _fighter.VoicePlayer.Stream = _data.VoiceSound;
            _fighter.VoicePlayer.VolumeDb = _data.VoiceVolumeDb;
            _fighter.VoicePlayer.Play();
        }

        if (!_isAirborneState)
        {
            Vector2 vel = _fighter.Velocity;
            vel.X = 0; 
            _fighter.Velocity = vel;
        }
        
        if (_data.HitStatsList != null && _data.HitStatsList.Count > 0)
        {
            CurrentHitStats = _data.HitStatsList[0];
            _currentHitIndex = 0;
        }
        
    }

    public override void PhysicsUpdate(double delta)
    {
        if (_fighter.Combat.HitStopTimer > 0) 
        {
            if (_fighter.Anim.IsPlaying()) _fighter.Anim.Pause(); 
            return; 
        }
        
        if (_bufferedCancel != null)
        {
            _fighter.StateMachine.ChangeState(new AttackState(_fighter, _bufferedCancel, _isAirborneState, _isCrouchingState));
            return; 
        }
        
        if (!_fighter.Anim.IsPlaying() && !_launcherHanging)
        {
            _fighter.Anim.Play(); 
        }

        CheckForCancels();
        
        _currentFrame++;

        ProcessAudio();
        
        bool isMoveFinished = _currentFrame > _data.TotalFrames;
        
        ApplyPhysics(delta);
        bool isFalling = _fighter.Velocity.Y > 0; 
        
        _fighter.Physics.ApplyMovementAndPush();
        
        if (HasYProfile() && _hasLaunched)
        {
            if (isMoveFinished)
            {
                _launcherHanging = true;
                if (_fighter.Anim.IsPlaying()) _fighter.Anim.Pause();
            }
            
            if (isFalling && _fighter.IsOnFloor())
            {
                _fighter.StateMachine.ChangeState(new IdleState(_fighter, true));
            }
            return; 
        }

        if (isMoveFinished)
        {
            if (_isAirborneState) _fighter.StateMachine.ChangeState(new AirState(_fighter)); 
            else if (_isCrouchingState) _fighter.StateMachine.ChangeState(new CrouchState(_fighter, false)); 
            else _fighter.StateMachine.ChangeState(new IdleState(_fighter)); 
        }
        else if (_isAirborneState && isFalling && _fighter.IsOnFloor())
        {
            _fighter.StateMachine.ChangeState(new IdleState(_fighter, true));
        }
    }

    private void ProcessAudio()
    {
        if (_currentFrame == _data.WhiffFrame && _data.WhiffSound != null)
        {
            _fighter.SfxPlayer.Stream = _data.WhiffSound;
            _fighter.SfxPlayer.VolumeDb = _data.WhiffVolumeDb;
            _fighter.SfxPlayer.Play();
        }
    }

    private void ApplyPhysics(double delta)
    {
        Vector2 vel = _fighter.Velocity;
        bool xSpeedJustChanged = false;
        bool ySpeedJustChanged = false;

        foreach (var keyframe in _data.XSpeedProfile)
        {
            if (keyframe.Frame == _currentFrame)
            {
                _currentXSpeed = keyframe.Speed;
                xSpeedJustChanged = true;
                break;
            }
        }

        foreach (var keyframe in _data.YSpeedProfile)
        {
            if (keyframe.Frame == _currentFrame)
            {
                vel.Y = keyframe.Speed;
                ySpeedJustChanged = true;
                _hasLaunched = true;
                break;
            }
        }

        if (HasYProfile()) 
        {
            if (!_hasLaunched) 
            {
                vel.X = HasXProfile() ? _fighter.FacingDirection * _currentXSpeed : 0;
            }
            else 
            {
                if (xSpeedJustChanged) vel.X = _fighter.FacingDirection * _currentXSpeed; 
                if (!ySpeedJustChanged) vel.Y += _fighter.Physics.Gravity * (float)delta;
                
                vel.X = Mathf.MoveToward(vel.X, 0, _data.AirDrag * (float)delta);
            }
        }
        else 
        {
            if (HasXProfile()) vel.X = _fighter.FacingDirection * _currentXSpeed; 
            else if (!_isAirborneState) vel.X = 0; 

            if (_isAirborneState) vel.Y += _fighter.Physics.Gravity * (float)delta;
        }

        _fighter.Velocity = vel;
    }

    private bool HasYProfile() => _data.YSpeedProfile != null && _data.YSpeedProfile.Count > 0;
    private bool HasXProfile() => _data.XSpeedProfile != null && _data.XSpeedProfile.Count > 0;

    private void CheckForCancels()
    {
        if (!_data.IsSpecialCancelable) return;
        if (!_fighter.HitManager.HasHit) return;
        
        int cancelWindowEnd = _data.CancelWindowStart + _data.CancelWindow;
        
        if (_currentFrame < _data.CancelWindowStart || _currentFrame > cancelWindowEnd) return;
        
        AttackData triggeredMove = _fighter.Moves.EvaluateAvailableMoves(_fighter.Buffer, _isAirborneState);
        
        if (triggeredMove != null && triggeredMove.RequiredMotion != AttackData.MotionType.None && triggeredMove != _data)
        {
            _fighter.StateMachine.ChangeState(new AttackState(_fighter, triggeredMove, _isAirborneState, _isCrouchingState));
        }
    }
    
    public void SetHitIndex(int index)
    {   
        if (index == _currentHitIndex) return;
        
        if (_data.HitStatsList != null && index >= 0 && index < _data.HitStatsList.Count)
        {
            _currentHitIndex = index;
            CurrentHitStats = _data.HitStatsList[index];
            _fighter.HitManager.ResetHit();
        }
    }
    
    public void SpawnProjectile()
    {
        if (_data.ProjectilePrefab == null) return;

        Projectile fireball = _data.ProjectilePrefab.Instantiate<Projectile>();
        
        fireball.OwnerFighter = _fighter;
        fireball.MoveData = _data;
        fireball.HitStats = CurrentHitStats;
        fireball.Direction = _fighter.FacingDirection;
        
        Vector2 spawnOffset = _data.ProjectileSpawnOffset;
        spawnOffset.X *= _fighter.FacingDirection; 
        
        fireball.GlobalPosition = _fighter.GlobalPosition + spawnOffset;
        
        _fighter.ActiveProjectile = fireball;
        _fighter.GetTree().CurrentScene.AddChild(fireball);
    }
    
    public override void Exit()
    {
        foreach (Node child in _fighter.HitManager.GetChildren())
        {
            if (child is CollisionShape2D shape)
            {
                shape.SetDeferred("disabled", true);
            }
        }
        
        if (_fighter.ExtendedHurtbox != null)
        {
            foreach (Node child in _fighter.ExtendedHurtbox.GetChildren())
            {
                if (child is CollisionShape2D shape)
                {
                    shape.SetDeferred("disabled", true);
                }
            }
        }
    }
}