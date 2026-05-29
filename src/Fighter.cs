using Godot;
using rdStrikeClone.States;

public partial class Fighter : CharacterBody2D
{
    public int Health = 1000;

    public float WalkSpeed = 500.0f;
    public float JumpForce = -2000.0f;
    public float Gravity = 5500.0f;

    private float _maxPushboxForce = 15.0f;
    private float _minPushboxForce = 2.0f;
    private float _pushboxWidth = 80.0f;

    // Universal Pushback
    private bool _isPushed;
    private float _pushSpeed;
    private const float PushDecel = 1500.0f;
    
    // [Export] public float UniversalAirLaunchY = -1200.0f; 
    // [Export] public float UniversalAirPushX = 200.0f;
    
    public float CoreOverlapLimit = 25.0f;

    public BaseState CurrentState;
    public int FacingDirection = 1;
    private int _hitStopTimer = 0;

    public InputBuffer Buffer;
    public Node2D Visuals;
    public AnimationPlayer Anim;
    public Label DebugLabel;
    public Node2D AttackContainer;
    public Area2D Pushbox;
    [Export] public CollisionShape2D PillboxShape;
    private float _pillboxOffset = 3.0f;

    [Export]
    public Fighter Opponent;

    [ExportGroup("Move List")]
    [Export]
    public PackedScene sMpPrefab;

    [Export]
    public PackedScene cMkPrefab;

    [Export]
    public PackedScene qcfPrefab;

    [Export]
    public PackedScene jHkPrefab;
    
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
        if (CurrentState != null)
            CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void ApplyUniversalPushback(float force, float direction)
    {
        _isPushed = true;
        _pushSpeed = force * direction;
    }

    public override void _PhysicsProcess(double delta)
    {   
        // Hitstop
        if (_hitStopTimer > 0)
        {
            _hitStopTimer--;

            CurrentState?.CheckForCancels();

            if (_hitStopTimer == 0)
            {
                Anim.Play();
            }
            return;
        }
        // Pushback
        if (_isPushed)
        {
            _pushSpeed = Mathf.MoveToward(_pushSpeed, 0, PushDecel * (float)delta);
            
            if (Mathf.IsZeroApprox(_pushSpeed))
            {
                _isPushed = false;
            }
        }

        if (CurrentState != null)
        {
            CurrentState.PhysicsUpdate(delta);
            CurrentState?.CheckForCancels();
        }

        if (DebugLabel != null)
        {
            DebugLabel.Text = Buffer.GetDebugHistory();
        }

        // if (Name == "Player2")
        // {
        //     GD.Print(CurrentState);
        // }
        
    }

    public void ReceiveHit(NormalAttack attack, HitboxData hitbox)
    {
        bool successfullyBlocked = CheckIfBlocked(hitbox.Height);
        bool isCrouching = Buffer.IsInputActive(InputBuffer.InputFlag.Down);
        float actualPushbackForce = hitbox.PushbackForce;
        
        // int stunFrames = successfullyBlocked ? attack.BlockStunFrames : attack.HitStunFrames;
        // int frameAdvantage = stunFrames - attack.GetRemainingFrames();
        // GD.Print($"--- IMPACT ---");
        // GD.Print($"Stun: {stunFrames}");
        // GD.Print($"CurrentFrame: {attack.GetCurrentFrame()}"); // Assuming you have this method
        // GD.Print($"Remaining: {attack.GetRemainingFrames()}");
        // GD.Print($"Calculated Advantage: {frameAdvantage}");
        // GD.Print("Advantage:" + frameAdvantage);
        
        // Pushback Direction
        float pushAwayFromAttackerDirection = Mathf.Sign(this.GlobalPosition.X - this.Opponent.GlobalPosition.X);
        var collision = this.TestMove(this.GlobalTransform, new Vector2(pushAwayFromAttackerDirection * 5.0f, 0));
        
        if (collision) // Wall
        {
            // Attacker gets pushed back instead
            this.Opponent.ApplyUniversalPushback(hitbox.PushbackForce, -pushAwayFromAttackerDirection);
            actualPushbackForce = 0; 
        }
        // Block
        if (successfullyBlocked)
        {
            ChangeState(
                new BlockState(this, attack.BlockStunFrames, actualPushbackForce, isCrouching, hitbox.Height)
            );
            if (actualPushbackForce > 0)
                this.ApplyUniversalPushback(actualPushbackForce, pushAwayFromAttackerDirection);
        }
        else
        {
            // Damage
            Health -= attack.Damage;
            if (Health < 0) Health = 0;
            EmitSignal(SignalName.HealthChanged, Health);

            // Check Airborne
            if (!IsOnFloor())
            {
                // X-axis push
                float appliedAirPushX = attack.AirPushX * pushAwayFromAttackerDirection;
                
                ChangeState(
                    new AirHitState(
                        this,
                        attack.HitStunFrames,
                        appliedAirPushX,
                        attack.AirLaunchY,
                        hitbox.Juggle
                    )
                );
            }
            else
            {
                // Grounded Hit
                ChangeState(
                    new HitState(
                        this,
                        attack.HitStunFrames,
                        actualPushbackForce,
                        hitbox.Pull,
                        attack.Strength,
                        hitbox.Height
                    )
                );
                
                if (actualPushbackForce > 0)
                    this.ApplyUniversalPushback(actualPushbackForce, pushAwayFromAttackerDirection);
            }
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
            // Invincible
            if (hitFighter.CurrentState != null && hitFighter.CurrentState.IsInvincible)
            {
                return; 
            }

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
        if (FacingDirection == 1 && Buffer.IsInputActive(InputBuffer.InputFlag.Left))
            return true;
        if (FacingDirection == -1 && Buffer.IsInputActive(InputBuffer.InputFlag.Right))
            return true;
        return false;
    }

    public bool CheckIfBlocked(NormalAttack.HitHeight height)
    {
        bool isInBlockstun = CurrentState is BlockState;
        
        if (!isInBlockstun && (CurrentState == null || !CurrentState.CanBlock))
            return false;
        
        if (!isInBlockstun && !IsHoldingBack())
            return false;
        
        bool isHoldingDown = Buffer.IsInputActive(InputBuffer.InputFlag.Down);

        if (height == NormalAttack.HitHeight.High && isHoldingDown)
            return false;

        if (height == NormalAttack.HitHeight.Low && !isHoldingDown)
            return false; 

        return true;
    }

    public void TurnToFaceOpponent()
    {
        if (Opponent == null)
            return;

        float distance = Mathf.Abs(Opponent.GlobalPosition.X - GlobalPosition.X);

        if (distance < 15.0f && IsOnFloor() && Opponent.IsOnFloor())
        {
            return;
        }

        if (Opponent.GlobalPosition.X < GlobalPosition.X && FacingDirection == 1)
        {
            FacingDirection = -1;
            Visuals.Scale = new Vector2(-1, 1);
            UpdatePillboxbox();
        }
        else if (Opponent.GlobalPosition.X > GlobalPosition.X && FacingDirection == -1)
        {
            FacingDirection = 1;
            Visuals.Scale = new Vector2(1, 1);
            UpdatePillboxbox();
        }
    }
    private void UpdatePillboxbox()
    {
        if (PillboxShape != null)
        {
            PillboxShape.Position = new Vector2(_pillboxOffset * FacingDirection, PillboxShape.Position.Y);
        }
    }

    public void ApplyMovementAndPush()
    {
        if (Opponent == null)
        {
            MoveAndSlide();
            return;
        }
        
        bool wasOnLeft = GlobalPosition.X <= Opponent.GlobalPosition.X;
        Vector2 originalVel = Velocity;
        
        if (_isPushed)
        {
            Velocity = new Vector2(originalVel.X + _pushSpeed, originalVel.Y);
        }
        
        MoveAndSlide();
        
        KineticBounce();
        
        if (_isPushed)
        {
            Velocity = originalVel;
        }
        
        CrossoverLock(wasOnLeft);
        Pushboxes(wasOnLeft);
    }
    
    private void KineticBounce()
    {
        if (_isPushed && IsOnWall())
        {
            float remainingSpeed = _pushSpeed;
            _isPushed = false;
            _pushSpeed = 0;

            Opponent.ApplyUniversalPushback(Mathf.Abs(remainingSpeed), -Mathf.Sign(remainingSpeed));
        }
    }

    private void CrossoverLock(bool wasOnLeft)
    {
        if (IsOnFloor() && Opponent.IsOnFloor())
        {
            bool isNowOnLeft = GlobalPosition.X <= Opponent.GlobalPosition.X;
            
            if (wasOnLeft != isNowOnLeft)
            {
                Vector2 fixedPos = GlobalPosition;
                fixedPos.X = Opponent.GlobalPosition.X;
                GlobalPosition = fixedPos;
                
                Vector2 vel = Velocity;
                vel.X = 0;
                Velocity = vel;
            }
        }
    }

    private void Pushboxes(bool wasOnLeft)
    {
        if (Pushbox != null && Opponent.Pushbox != null)
        {
            if (Pushbox.GetOverlappingAreas().Contains(Opponent.Pushbox))
            {
                float distance = Mathf.Abs(GlobalPosition.X - Opponent.GlobalPosition.X);
                float pushDirection = 0;
                
                if (IsOnFloor() && Opponent.IsOnFloor() && distance < CoreOverlapLimit)
                {
                    float separation = CoreOverlapLimit - distance;
                    pushDirection = wasOnLeft ? -separation : separation;
                }
                else
                {
                    float overlapRatio = 1.0f - (distance / _pushboxWidth);
                    overlapRatio = Mathf.Clamp(overlapRatio, 0.0f, 1.0f);
                    float currentForce = Mathf.Lerp(_minPushboxForce, _maxPushboxForce, overlapRatio);

                    if (GlobalPosition.X < Opponent.GlobalPosition.X)
                        pushDirection = -currentForce;
                    else if (GlobalPosition.X > Opponent.GlobalPosition.X)
                        pushDirection = currentForce;
                    else
                        pushDirection = wasOnLeft ? -currentForce : currentForce;
                }
                
                MoveAndCollide(new Vector2(pushDirection, 0));
            }
        }
    }
}
