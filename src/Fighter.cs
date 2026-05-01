using Godot;
using rdStrikeClone.States;

public partial class Fighter : CharacterBody2D
{

    public int Health = 1000;
    
    public float WalkSpeed = 500.0f; 
    public float JumpForce = -2000.0f; 
    public float Gravity = 5500.0f;
    
    private float _maxPushboxForce = 12.0f;
    private float _minPushboxForce = 2.0f;  
    private float _pushboxWidth = 50.0f; 
    
    public BaseState _currentState;
    public int FacingDirection = 1;
    private int _hitStopTimer = 0;
    
    public InputBuffer Buffer;
    public Node2D Visuals;
    public AnimationPlayer Anim;
    public Label DebugLabel;
    public Node2D AttackContainer;
    public Area2D Pushbox;
    
    [Export] public Fighter Opponent;
    
    [ExportGroup("Move List")]
    [Export] public PackedScene sMpPrefab; 
    [Export] public PackedScene cMkPrefab;
    [Export] public PackedScene qcfPrefab;
    
    [Signal]
    public delegate void HealthChangedEventHandler(int newHealth);
    
    public override void _Ready()
    {
        Buffer = GetNode<InputBuffer>("InputBuffer");
        Anim = GetNode<AnimationPlayer>("AnimationPlayer");
        Visuals = GetNode<Node2D>("Visuals");
        Pushbox = GetNodeOrNull<Area2D>("Pushbox");
        AttackContainer = GetNode<Node2D>("Visuals/AttackContainer");
        
        ChangeState(new IdleState(this));
        
        DebugLabel = GetNode<Label>("UI/BufferRing");
    }
    
    public void ChangeState(BaseState newState)
    {
        if (_currentState != null) _currentState.Exit();
        _currentState = newState;
        _currentState.Enter();
    }   
    
    public override void _PhysicsProcess(double delta)
    {
        if (_hitStopTimer > 0)
        {
            _hitStopTimer--;
            
            _currentState?.CheckForCancels();
            
            if (_hitStopTimer == 0)
            {
                Anim.Play(); 
            }
            return; 
        }
        
        if (_currentState != null) 
        {
            _currentState.PhysicsUpdate(delta);
            _currentState?.CheckForCancels();
        }
        
        if (DebugLabel != null)
        {
            DebugLabel.Text = Buffer.GetDebugHistory();
        }
    }
    
    public void ReceiveHit(NormalAttack attack, HitboxData hitbox) 
    {
        bool successfullyBlocked = CheckIfBlocked(hitbox.Height);
        bool isCrouching = Buffer.IsInputActive(InputBuffer.InputFlag.Down);

        int stunFrames = successfullyBlocked ? attack.BlockStunFrames : attack.HitStunFrames;
        int frameAdvantage = stunFrames - attack.GetRemainingFrames();
        GD.Print($"{(successfullyBlocked ? "Block" : "Hit")}! Frame Advantage: {(frameAdvantage >= 0 ? "+" : "")}{frameAdvantage}");

        if (successfullyBlocked)
        {
            //CHIP DAMAGE TALVEZ
            ChangeState(new BlockState(this, attack.BlockStunFrames, hitbox.PushbackForce, isCrouching));
        }
        else
        {
            Health -= attack.Damage;
            if (Health < 0) Health = 0; 
            EmitSignal(SignalName.HealthChanged, Health);
            
            ChangeState(new HitState(this, attack.HitStunFrames, hitbox.PushbackForce, attack.Strength, hitbox.Height)); 
        }
    }
    
    public void ApplyHitStop(int frames)
    {
        _hitStopTimer = frames;
        
        Anim.Seek(Anim.CurrentAnimationPosition, true);
        
        if (Anim.IsPlaying())
        {
            Anim.Pause(); 
        }
    }
    
    public void OnHitboxConnected(Area2D area, HitboxData attackingHitbox)
    {
        Fighter hitFighter = null;
        
        Node current = area;
        while (current != null && !(current is Fighter))
        {
            current = current.GetParent();
        }
        hitFighter = current as Fighter;
        
        if (hitFighter != null && hitFighter == Opponent)
        {
            NormalAttack normalAttackStats = attackingHitbox.Parent;
    
            if (normalAttackStats != null)
            {
                normalAttackStats.HasHit = true; 
                attackingHitbox.HasConnected = true; 
                
                hitFighter.ReceiveHit(normalAttackStats, attackingHitbox);
                
                this.ApplyHitStop(normalAttackStats.HitStopFrames);       
                hitFighter.ApplyHitStop(normalAttackStats.HitStopFrames);   
            }
        }
    }   
    
    public bool IsHoldingBack()
    {
        if (FacingDirection == 1 && Buffer.IsInputActive(InputBuffer.InputFlag.Left)) return true;
        if (FacingDirection == -1 && Buffer.IsInputActive(InputBuffer.InputFlag.Right)) return true;
        return false;
    }
    
    public bool CheckIfBlocked(NormalAttack.HitHeight Height)
    {
        
        if (_currentState == null || !_currentState.CanBlock) return false;
        
        if (!IsHoldingBack()) return false;

        bool isHoldingDown = Buffer.IsInputActive(InputBuffer.InputFlag.Down);
        
        if (Height == NormalAttack.HitHeight.High && isHoldingDown) 
            return false;
            
        if (Height == NormalAttack.HitHeight.Low && !isHoldingDown) 
            return false;
        
        return true; 
    }
    
    public void TurnToFaceOpponent()
    {
        if (Opponent == null) return;
        
        if (Opponent.GlobalPosition.X < GlobalPosition.X && FacingDirection == 1)
        {
            FacingDirection = -1;
            Visuals.Scale = new Vector2(-1, 1);
        }
        else if (Opponent.GlobalPosition.X > GlobalPosition.X && FacingDirection == -1)
        {
            FacingDirection = 1;
            Visuals.Scale = new Vector2(1, 1);
        }
    }
    public void ApplyMovementAndPush()
    {
        MoveAndSlide();
        
        if (Pushbox != null && Opponent != null && Opponent.Pushbox != null)
        {
            if (Pushbox.GetOverlappingAreas().Contains(Opponent.Pushbox))
            {
                float distance = Mathf.Abs(GlobalPosition.X - Opponent.GlobalPosition.X);
                
                float overlapRatio = 1.0f - (distance / _pushboxWidth);
                overlapRatio = Mathf.Clamp(overlapRatio, 0.0f, 1.0f);
                
                float currentForce = Mathf.Lerp(_minPushboxForce, _maxPushboxForce, overlapRatio);
                float pushDirection = 0;
                
                if (GlobalPosition.X < Opponent.GlobalPosition.X)
                {
                    pushDirection = -currentForce; 
                }
                else if (GlobalPosition.X > Opponent.GlobalPosition.X)
                {
                    pushDirection = currentForce; 
                }
                else 
                {
                    pushDirection = FacingDirection == 1 ? -_maxPushboxForce : _maxPushboxForce;
                }

                Vector2 pushVector = new Vector2(pushDirection, 0);
                this.MoveAndCollide(pushVector);
            }
        }
    }
    
}
