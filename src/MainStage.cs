using Godot;

public partial class MainStage : Node2D
{
    private Fighter _p1;
    private Fighter _p2;

    public override void _Ready()
    {
        _p1 = GetNode<Fighter>("Player1");
        _p2 = GetNode<Fighter>("Player2");
        
        ProgressBar p1Bar = GetNode<ProgressBar>("HUD/P1HealthBar");
        ProgressBar p2Bar = GetNode<ProgressBar>("HUD/P2HealthBar");
        
        _p1.Combat.HealthChanged += (newHealth) => p1Bar.Value = newHealth;
        _p2.Combat.HealthChanged += (newHealth) => p2Bar.Value = newHealth;
    }

    public override void _PhysicsProcess(double delta)
    {
        _p1.Tick(delta);
        _p2.Tick(delta);
    }
}