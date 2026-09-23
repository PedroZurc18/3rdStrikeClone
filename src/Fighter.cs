using Godot;
using rdStrikeClone;
using rdStrikeClone.Components;
using rdStrikeClone.States;
using rdStrikeClone.Data;

public partial class Fighter : CharacterBody2D
{
    public int FacingDirection = 1;

    public InputBuffer Buffer;
    public Node2D Visuals;
    public AnimationPlayer Anim;
    public AudioStreamPlayer2D SfxPlayer;
    public AudioStreamPlayer2D HitPlayer;
    public AudioStreamPlayer2D VoicePlayer;
    public Label DebugLabel;
    public Area2D Pushbox;
    [Export] public CollisionShape2D PillboxShape;
    private float _pillboxOffset = 3.0f;
    public Area2D ExtendedHurtbox { get; private set; }

    public Projectile ActiveProjectile { get; set; }
    
    [Export] public Fighter Opponent;

    public MoveManager Moves { get; private set; }
    
    // --- Components ---
    public PhysicsComponent Physics { get; private set; }
    public CombatComponent Combat { get; private set; }
    public StateMachineComponent StateMachine { get; private set; }
    public HitboxManager HitManager { get; private set; }

    public override void _Ready()
    {
        Buffer = GetNode<InputBuffer>("InputBuffer");
        Anim = GetNode<AnimationPlayer>("AnimationPlayer");
        Visuals = GetNode<Node2D>("Visuals");
        Pushbox = GetNodeOrNull<Area2D>("Pushbox");
        SfxPlayer = GetNode<AudioStreamPlayer2D>("SfxPlayer");
        HitPlayer = GetNode<AudioStreamPlayer2D>("HitPlayer");
        VoicePlayer = GetNode<AudioStreamPlayer2D>("VoicePlayer");
        Moves = GetNode<MoveManager>("MoveManager");
        ExtendedHurtbox = GetNode<Area2D>("Visuals/ExtendedHurtbox");
        
        Physics = GetNode<PhysicsComponent>("PhysicsComponent");
        Combat = GetNode<CombatComponent>("CombatComponent");
        StateMachine = GetNode<StateMachineComponent>("StateMachineComponent");
        HitManager = GetNode<HitboxManager>("Visuals/ActiveHitbox");
        
        StateMachine.ChangeState(new IdleState(this));

        DebugLabel = GetNode<Label>("UI/BufferRing");
    }

    public void Tick(double delta)
    {
        Buffer.Tick();
        Combat.Tick();
        StateMachine.Tick(delta);
        Physics.Tick(delta); 

        if (DebugLabel != null)
        {
            DebugLabel.Text = Buffer.GetDebugHistory();
        }
    }

    public void TurnToFaceOpponent()
    {
        if (Opponent == null) return;

        float distance = Mathf.Abs(Opponent.GlobalPosition.X - GlobalPosition.X);
        if (distance < 15.0f && IsOnFloor() && Opponent.IsOnFloor()) return;

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
    
    public void SpawnProjectile()
    {
        if (StateMachine.CurrentState is rdStrikeClone.States.AttackState attackState)
        {
            attackState.SpawnProjectile();
        }
    }
}
