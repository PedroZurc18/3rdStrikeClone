using Godot;

namespace rdStrikeClone;

[GlobalClass]
public partial class SpeedKeyframe : Resource
{
    [Export] public int Frame = 0;
    [Export] public float Speed = 0.0f;
}