using Godot;
using rdStrikeClone.Data;
using rdStrikeClone.States;

namespace rdStrikeClone.Components;

public partial class CombatComponent : Node
{
    [Signal]
    public delegate void HealthChangedEventHandler(int newHealth);

    [ExportGroup("Combat Stats")]
    [Export] public int Health = 1000;

    [ExportGroup("Parry System")] 
    [Export] private AudioStream ParrySound;
    [Export] public int GroundTapParryWindow = 10;
    [Export] public int GroundHoldParryWindow = 6;
    [Export] public int AirParryWindow = 7;
    [Export] public int AntiAirParryWindow = 5;
    [Export] public int ParryCooldown = 23;

    public int HitStopTimer = 0;
    
    private int _parryTimer = 0;
    private int _maxParryTimer = 0;
    private int _parryCooldownTimer = 0;
    private bool _wasForwardPressed = false;
    private bool _wasDownPressed = false;
    private AttackData.HitHeight _parryTypeReady = AttackData.HitHeight.Mid;

    private Fighter _fighter;

    public override void _Ready()
    {
        _fighter = GetParent<Fighter>();
    }

    public void Tick()
    {
        UpdateParryTimers();
        
        if (HitStopTimer > 0)
        {
            HitStopTimer--;
        }
    }

    public bool ReceiveHit(AttackData attack, rdStrikeClone.Data.HitboxFrameData hitbox)
    {   
        bool successfullyParried = CheckIfParried(hitbox.Height);

        if (successfullyParried)
        {
            _parryTimer = 0; 
            _parryCooldownTimer = 0; 
            
            if (ParrySound != null) { _fighter.HitPlayer.Stream = ParrySound; _fighter.HitPlayer.Play(); }
            _fighter.Visuals.Modulate = new Color(0.5f, 0.8f, 1.0f);
            GetTree().CreateTimer(0.15f).Timeout += () => _fighter.Visuals.Modulate = Colors.White;

            _fighter.StateMachine.ChangeState(new ParryState(_fighter, hitbox.Height, !_fighter.IsOnFloor()));
            return true; 
        }

        bool successfullyBlocked = CheckIfBlocked(hitbox.Height);
        bool isCrouching = _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down);
        float actualPushbackForce = hitbox.PushbackForce;
        float pushAwayFromAttackerDirection;
        
        if (!_fighter.IsOnFloor() && Mathf.Abs(_fighter.Velocity.X) > 0.01f)
        {
            pushAwayFromAttackerDirection = -Mathf.Sign(_fighter.Velocity.X);
        }
        else
        {
            pushAwayFromAttackerDirection = Mathf.Sign(_fighter.GlobalPosition.X - _fighter.Opponent.GlobalPosition.X);
            if (pushAwayFromAttackerDirection == 0) pushAwayFromAttackerDirection = -_fighter.Opponent.FacingDirection;
        }
        
        var collision = _fighter.TestMove(_fighter.GlobalTransform, new Vector2(pushAwayFromAttackerDirection * 5.0f, 0));
        
        if (hitbox.Scoop != 0 && !successfullyBlocked)
        {
            float idealXPosition = _fighter.Opponent.GlobalPosition.X + (_fighter.Opponent.FacingDirection * hitbox.Scoop);
            Vector2 snappedPosition = _fighter.GlobalPosition;
            snappedPosition.X = idealXPosition;
            _fighter.GlobalPosition = snappedPosition;
        }
        
        if (collision) 
        {
            _fighter.Opponent.Physics.ApplyUniversalPushback(hitbox.PushbackForce, -pushAwayFromAttackerDirection);
            actualPushbackForce = 0; 
        }
        
        if (successfullyBlocked)
        {
            if (attack.BlockSound != null)
            {
                _fighter.HitPlayer.Stream = attack.BlockSound;
                _fighter.HitPlayer.VolumeDb = attack.BlockVolumeDb;
                _fighter.HitPlayer.Play();
            }
            _fighter.StateMachine.ChangeState(new BlockState(_fighter, hitbox.BlockStunFrames, actualPushbackForce, isCrouching, hitbox.Height));
            if (actualPushbackForce > 0) _fighter.Physics.ApplyUniversalPushback(actualPushbackForce, pushAwayFromAttackerDirection);
        }
        else
        {
            if (attack.HitSound != null)
            {
                _fighter.HitPlayer.Stream = attack.HitSound;
                _fighter.HitPlayer.VolumeDb = attack.HitVolumeDb;
                _fighter.HitPlayer.Play();
            }
            
            Health -= hitbox.Damage;
            if (Health < 0) Health = 0;
            EmitSignal(SignalName.HealthChanged, Health);

            if (!_fighter.IsOnFloor() || hitbox.Juggle)
            {
                float appliedAirPushX = hitbox.AirPushX * pushAwayFromAttackerDirection;
                _fighter.StateMachine.ChangeState(new AirHitState(_fighter, hitbox.HitStunFrames, appliedAirPushX, hitbox.AirLaunchY, hitbox.Juggle));
            }
            else
            {
                _fighter.StateMachine.ChangeState(new HitState(_fighter, hitbox.HitStunFrames, actualPushbackForce, hitbox.Pull, hitbox.Strength, hitbox.Height));
                if (actualPushbackForce > 0) _fighter.Physics.ApplyUniversalPushback(actualPushbackForce, pushAwayFromAttackerDirection);
            }
        }
        return false;
    }

    public void ApplyHitStop(int frames, bool pauseAnimation = true)
    {
        HitStopTimer = frames;
        
        if (pauseAnimation)
        {
            _fighter.Anim.CallDeferred(AnimationPlayer.MethodName.Seek,_fighter.Anim.CurrentAnimationPosition, true);
            if (_fighter.Anim.IsPlaying()) _fighter.Anim.CallDeferred(AnimationPlayer.MethodName.Pause);
        }
    }

    public bool CheckIfBlocked(AttackData.HitHeight height)
    {
        bool isInBlockstun = _fighter.StateMachine.CurrentState is BlockState;
        
        if (!isInBlockstun && (_fighter.StateMachine.CurrentState == null || !_fighter.StateMachine.CurrentState.CanBlock)) return false;
        if (!isInBlockstun && !IsHoldingBack()) return false;
        
        bool isHoldingDown = _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down);

        if (height == AttackData.HitHeight.High && isHoldingDown) return false;
        if (height == AttackData.HitHeight.Low && !isHoldingDown) return false; 

        return true;
    }
    
    public bool CheckIfParried(AttackData.HitHeight attackHeight)
    {
        if (_parryTimer > 0)
        {
            int framesActive = _maxParryTimer - _parryTimer;

            if (!_fighter.Opponent.IsOnFloor() && framesActive >= AntiAirParryWindow) return false;

            if (_parryTypeReady == AttackData.HitHeight.High && 
                (attackHeight == AttackData.HitHeight.High || attackHeight == AttackData.HitHeight.Mid)) return true;
            
            if (_parryTypeReady == AttackData.HitHeight.Low && attackHeight == AttackData.HitHeight.Low) return true;
        }
        return false;
    }
    
    private void UpdateParryTimers()
    {
        if (_parryTimer > 0) _parryTimer--;
        if (_parryCooldownTimer > 0) _parryCooldownTimer--;

        bool isForwardActive = IsPureForward();
        bool isDownActive = IsPureDown();
        
        if (_parryCooldownTimer > 0)
        {
            if ((isForwardActive && !_wasForwardPressed) || (isDownActive && !_wasDownPressed))
            {
                _parryTimer = 0;
                _parryCooldownTimer = ParryCooldown;
            }
            _wasForwardPressed = isForwardActive;
            _wasDownPressed = isDownActive;
            return;
        }
        
        if (_fighter.IsOnFloor() && _parryTimer > 0)
        {
            int framesActive = _maxParryTimer - _parryTimer;
            if (framesActive >= GroundHoldParryWindow)
            {
                if ((_parryTypeReady == AttackData.HitHeight.High && isForwardActive) ||
                    (_parryTypeReady == AttackData.HitHeight.Low && isDownActive))
                {
                    _parryTimer = 0;
                }
            }
        }
        
        bool isBusy = _fighter.StateMachine.CurrentState is HitState || _fighter.StateMachine.CurrentState is AirHitState || 
                      _fighter.StateMachine.CurrentState is AttackState || _fighter.StateMachine.CurrentState is BlockState;
                      
        if (isBusy || _parryCooldownTimer > 0) 
        {
            _wasForwardPressed = isForwardActive;
            _wasDownPressed = isDownActive;
            return;
        }
        
        bool triggeredParry = false;

        if (isForwardActive && !_wasForwardPressed)
        {
            _parryTimer = _fighter.IsOnFloor() ? GroundTapParryWindow : AirParryWindow;
            _maxParryTimer = _parryTimer;
            _parryTypeReady = AttackData.HitHeight.High; 
            triggeredParry = true;
        }
        else if (isDownActive && !_wasDownPressed && _fighter.IsOnFloor())
        {
            _parryTimer = GroundTapParryWindow;
            _maxParryTimer = _parryTimer;
            _parryTypeReady = AttackData.HitHeight.Low; 
            triggeredParry = true;
        }

        if (triggeredParry) _parryCooldownTimer = ParryCooldown; 

        _wasForwardPressed = isForwardActive;
        _wasDownPressed = isDownActive;
    }
    
    public bool IsPureForward()
    {
        bool forward = (_fighter.FacingDirection == 1 && _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Right)) ||
                       (_fighter.FacingDirection == -1 && _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Left));
        bool up = _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Up);
        bool down = _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down);
        return forward && !up && !down;
    }

    public bool IsPureDown()
    {
        bool down = _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Down);
        bool left = _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Left);
        bool right = _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Right);
        return down && !left && !right;
    }

    public bool IsHoldingBack()
    {
        if (_fighter.FacingDirection == 1 && _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Left)) return true;
        if (_fighter.FacingDirection == -1 && _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Right)) return true;
        return false;
    }
    
    public bool IsHoldingForward()
    {
        if (_fighter.FacingDirection == 1 && _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Right)) return true;
        if (_fighter.FacingDirection == -1 && _fighter.Buffer.IsInputActive(InputBuffer.InputFlag.Left)) return true;
        return false;
    }
}