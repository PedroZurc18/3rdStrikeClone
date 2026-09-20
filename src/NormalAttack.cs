using Godot;
using rdStrikeClone;

public partial class NormalAttack : Node2D
{
    // 1. The Core Frame Data
    [ExportGroup("Frame Data")]
    [Export] public int StartupFrames;
    [Export] public int RecoveryFrames;
    [Export] public int TotalFrames;
    [Export] public string AnimationName;
    
    public enum HitHeight { High, Mid, Low }
    public enum AttackStrength { Light, Medium, Heavy }
    public enum CommandDirection { Neutral, Forward, Back, Down, Up }
    [Export] public AttackStrength Strength = AttackStrength.Medium;
    
    [ExportGroup("Command Execution")]
    [Export] public InputBuffer.InputFlag RequiredButton; 
    [Export] public CommandDirection RequiredDirection = CommandDirection.Neutral;
    [Export] public bool IsAirborneMove = false;
    
    [ExportGroup("Combat Stats")] 
    [Export] public int Damage = 10;
    [Export] public int HitStunFrames = 15;
    [Export] public int BlockStunFrames = 11;
    [Export] public int HitStopFrames = 12;

    [ExportGroup("Cancel Data")]
    [Export] public bool IsSpecialCancelable = true;
    [Export] public int CancelWindowStart = 0;
    [Export] public int CancelWindowEnd = 99;
    
    [ExportGroup("Air Hit Data")]
    [Export] public float AirLaunchY = -1200.0f; 
    [Export] public float AirPushX = 200.0f;
    
    [ExportGroup("Movement")]
    [Export] public Godot.Collections.Array<SpeedKeyframe> XSpeedProfile = new();
    [Export] public Godot.Collections.Array<SpeedKeyframe> YSpeedProfile = new();
    [Export] public float AirDrag = 0.0f;
    
    [ExportGroup("Audio")]
    [Export] public AudioStream WhiffSound;
    [Export] public float WhiffVolumeDb = 0.0f;
    [Export] public int WhiffFrame = 0;
    
    [Export] public AudioStream HitSound;
    [Export] public float HitVolumeDb = 0.0f;
    
    [Export] public AudioStream BlockSound;
    [Export] public float BlockVolumeDb = 0.0f;
    
    [Export] public AudioStream VoiceSound;
    [Export] public float VoiceVolumeDb = 0.0f;
    
    // 3. The Physical Nodes
    public Node2D HitboxesFolder;
    public Node2D HurtboxesFolder;

    public bool HasYProfile => YSpeedProfile != null && YSpeedProfile.Count > 0;
    public bool HasXProfile => XSpeedProfile != null && XSpeedProfile.Count > 0;
    public bool HasLaunched { get; private set; } = false;
    public float CurrentXSpeed { get; private set; } = 0f;
    
    protected int _currentFrame = 0;
    protected Fighter _fighter; 
    
    public bool HasHit = false;
    
    
    public int GetCurrentFrame() => _currentFrame;
    public int GetRemainingFrames() => TotalFrames - _currentFrame;
    public bool IsInsideCancelWindow() => _currentFrame >= CancelWindowStart && _currentFrame <= CancelWindowEnd;

    public override void _Ready()
    {
        HitboxesFolder = GetNodeOrNull<Node2D>("Hitboxes");
        HurtboxesFolder = GetNodeOrNull<Node2D>("Hurtboxes");

        if (HitboxesFolder != null)
        {
            foreach (Node child in HitboxesFolder.GetChildren()) 
            {
                if (child is HitboxData box)
                {
                    box.Parent = this; 
                    
                    box.AreaEntered += (area) => 
                    {
                        if (_fighter != null && !box.HasConnected) 
                        {
                            _fighter.OnHitboxConnected(area, box);
                        }
                    };
                }
            }
        }
    }

    public virtual void Initialize(Fighter fighter)
    {
        _fighter = fighter;
        _currentFrame = 0;
        HasHit = false;                 
        
        if (HitboxesFolder != null)
        {
            foreach (Node child in HitboxesFolder.GetChildren()) 
            {
                if (child is HitboxData box)
                {
                    box.HasConnected = false; 
                }
            }
        }

        SetBoxesActive(false); 
        _fighter.Anim.Stop();
        _fighter.Anim.Play(AnimationName);
        
        CurrentXSpeed = 0f;
        HasLaunched = false;
    }
    
    public virtual bool ProcessMove()
    {
        _currentFrame++;
        
        UpdateBoxes(_currentFrame);
        
        if (VoiceSound != null && _currentFrame == 1)
        {
            _fighter.VoicePlayer.Stream = VoiceSound;
            _fighter.VoicePlayer.VolumeDb = VoiceVolumeDb;
            _fighter.VoicePlayer.Play();
        }
        
        if (_currentFrame == WhiffFrame && WhiffSound != null)
        {
            _fighter.SfxPlayer.Stream = WhiffSound;
            _fighter.SfxPlayer.VolumeDb = WhiffVolumeDb;
            _fighter.SfxPlayer.Play();
        }
        
        if (_currentFrame > TotalFrames)
        {
            return true; 
        }

        return false; 
    }
    
    public Vector2 ProcessPhysics(Vector2 currentVelocity, int facingDirection, double delta, float gravity, bool isStateAirborne)
    {
        Vector2 vel = currentVelocity;
        
        bool xSpeedJustChanged = false;
        if (HasXProfile)
        {
            foreach (var keyframe in XSpeedProfile)
            {
                if (keyframe.Frame == _currentFrame)
                {
                    CurrentXSpeed = keyframe.Speed;
                    xSpeedJustChanged = true;
                    break;
                }
            }
        }
        
        bool ySpeedJustChanged = false;
        if (HasYProfile)
        {
            foreach (var keyframe in YSpeedProfile)
            {
                if (keyframe.Frame == _currentFrame)
                {
                    vel.Y = keyframe.Speed;
                    ySpeedJustChanged = true;
                    HasLaunched = true; 
                    break;
                }
            }
        }
        
        if (HasYProfile) 
        {
            if (!HasLaunched) 
            {
                if (HasXProfile) vel.X = facingDirection * CurrentXSpeed;
                else vel.X = 0;
            }
            else 
            {
                if (xSpeedJustChanged) 
                {
                    vel.X = facingDirection * CurrentXSpeed; 
                }
                
                if (!ySpeedJustChanged) 
                {
                    vel.Y += gravity * (float)delta;
                }
                
                vel.X = Mathf.MoveToward(vel.X, 0, AirDrag * (float)delta);
            }
        }
        else 
        {
            if (HasXProfile) 
            {
                vel.X = facingDirection * CurrentXSpeed; 
            }
            else if (!isStateAirborne) 
            {
                vel.X = 0; 
            }

            if (isStateAirborne) 
            {
                vel.Y += gravity * (float)delta;
            }
        }

        return vel;
    }
    
    private void UpdateBoxes(int frame)
    {
        if (HitboxesFolder != null)
        {
            foreach (Node child in HitboxesFolder.GetChildren()) 
            {
                if (child is HitboxData box)
                {

                    bool shouldBeActive = (frame >= box.StartFrame && frame < (box.StartFrame + box.Duration));

                    foreach (CollisionShape2D shape in child.GetChildren()) {
                        if (shape.Disabled == shouldBeActive) {
                            shape.SetDeferred("disabled", !shouldBeActive);
                        }
                    }
                }
            }
        }
        
        if (HurtboxesFolder != null)
        {
            foreach (Node child in HurtboxesFolder.GetChildren())
            {
                if (child is HurtboxData hurtbox) 
                {
                    bool shouldBeActive = (frame >= hurtbox.StartFrame && frame < (hurtbox.StartFrame + hurtbox.Duration));
                    CollisionShape2D shape = hurtbox.GetChild<CollisionShape2D>(0);
                    
                    if (shape.Disabled == shouldBeActive)
                    {
                        shape.SetDeferred("disabled", !shouldBeActive);
                    }
                }
            }
        }
    }

    public void SetBoxesActive(bool active)
    {
        if (HitboxesFolder != null)
        {
            foreach (Node child in HitboxesFolder.GetChildren()) 
            {
                foreach (CollisionShape2D shape in child.GetChildren())
                {
                    shape.SetDeferred("disabled", !active);
                }
            }
        }
        
        if (HurtboxesFolder != null)
        {
            foreach (Node child in HurtboxesFolder.GetChildren())
            {
                if (child is Area2D area)
                {
                    area.GetChild<CollisionShape2D>(0).SetDeferred("disabled", !active);
                }
            }
        }
    }
}