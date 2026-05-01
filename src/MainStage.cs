using Godot;

public partial class MainStage : Node2D
{
    public override void _Ready()
    {
        Fighter p1 = GetNode<Fighter>("Player1");
        Fighter p2 = GetNode<Fighter>("Player2");
        
        ProgressBar p1Bar = GetNode<ProgressBar>("HUD/P1HealthBar");
        ProgressBar p2Bar = GetNode<ProgressBar>("HUD/P2HealthBar");
        
        p1.HealthChanged += (newHealth) => p1Bar.Value = newHealth;
        
        p2.HealthChanged += (newHealth) => p2Bar.Value = newHealth;
    }
}