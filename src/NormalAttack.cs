using Godot;
using rdStrikeClone;

public partial class NormalAttack : Node2D
{
    // 1. The Core Frame Data
    [ExportGroup("Frame Data")]
    [Export] public int StartupFrames = 4;
    [Export] public int ActiveFrames = 3;
    [Export] public int RecoveryFrames = 10;
    [Export] public string AnimationName = "punch";
    
    public enum HitHeight { High, Mid, Low }
    public enum AttackStrength { Light, Medium, Heavy }
    [Export] public AttackStrength Strength = AttackStrength.Medium;
    
    // 2. The Combat Stats
    [ExportGroup("Combat Stats")] 
    [Export] public int Damage = 10;
    [Export] public int HitStunFrames = 15;
    [Export] public int BlockStunFrames = 11;
    [Export] public int HitStopFrames = 12;

    [ExportGroup("Cancel Data")]
    [Export] public bool IsSpecialCancelable = true;
    [Export] public int CancelWindowStart = 0;
    [Export] public int CancelWindowEnd = 99;

    [ExportGroup("Movement")]
    [Export] public float ForwardSpeed = 0.0f;

    // 3. The Physical Nodes
    public Node2D HitboxesFolder;
    public Node2D HurtboxesFolder;

    private int _currentFrame = 0;
    private int _totalFrames;
    private Fighter _fighter; 
    
    public bool HasHit = false;

    public int GetCurrentFrame() => _currentFrame;
    public int GetRemainingFrames() => _totalFrames - _currentFrame;
    public bool IsInsideCancelWindow() => _currentFrame >= CancelWindowStart && _currentFrame <= CancelWindowEnd;

    public void Initialize(Fighter fighter)
    {
        _fighter = fighter;
        _totalFrames = StartupFrames + ActiveFrames + RecoveryFrames;

        HitboxesFolder = GetNodeOrNull<Node2D>("Hitboxes");
        HurtboxesFolder = GetNodeOrNull<Node2D>("Hurtboxes");
        
        if (HitboxesFolder != null)
        {
            foreach (Node child in HitboxesFolder.GetChildren()) 
            {
                if (child is HitboxData box)
                {
                    // 1. Tell the box who its boss is
                    box.Parent = this; 

                    // 2. Wire Godot's collision signal dynamically!
                    box.AreaEntered += (area) => 
                    {
                        // 3. Only allow the signal to go through if THIS hitbox hasn't hit yet
                        if (!box.HasConnected) 
                        {
                            _fighter.OnHitboxConnected(area, box);
                        }
                    };
                }
            }
        }

        SetBoxesActive(false); 
        _fighter.Anim.Stop();
        _fighter.Anim.Play(AnimationName);
    }
    
    public bool ProcessMove()
    {
        _currentFrame++;
        
        UpdateBoxes(_currentFrame);
        
        if (_currentFrame >= _totalFrames)
        {
            return true; 
        }

        return false; 
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
                    
                    CollisionShape2D shape = box.GetChild<CollisionShape2D>(0);
                    
                    if (shape.Disabled == shouldBeActive) 
                    {
                        shape.SetDeferred("disabled", !shouldBeActive);
                        
                        
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

    private void SetBoxesActive(bool active)
    {
        if (HitboxesFolder != null)
        {
            foreach (Node child in HitboxesFolder.GetChildren()) 
            {
                if (child is Area2D area)
                {
                    area.GetChild<CollisionShape2D>(0).SetDeferred("disabled", !active);
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