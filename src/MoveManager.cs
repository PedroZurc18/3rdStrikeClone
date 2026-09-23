using Godot;
using rdStrikeClone.Data;

namespace rdStrikeClone;

public partial class MoveManager : Node
{
    [Export] public Godot.Collections.Array<AttackData> AvailableMoves = new();

    private Fighter _fighter;

    private readonly int[][] _motionQCF = { 
        new[] { 2, 3, 6 }, 
        new[] { 2, 6 }     
    }; 
    
    private readonly int[][] _motionQCB = { 
        new[] { 2, 1, 4 }, 
        new[] { 2, 4 } 
    }; 
    
    private readonly int[][] _motionDP  = { 
        new[] { 6, 2, 3 }, 
        new[] { 6, 2, 6 }, 
        new[] { 6, 3, 6 }  
    };

    public override void _Ready()
    {
        _fighter = GetParent<Fighter>();
    }

    public AttackData EvaluateAvailableMoves(InputBuffer buffer, bool isAirborne)
    {
        bool fireballExists = IsInstanceValid(_fighter.ActiveProjectile);
        foreach (AttackData attack in AvailableMoves)
        { 
            if (fireballExists && attack.ProjectilePrefab != null)
            {
                continue; 
            }
            
            if (attack.IsAirborneMove != isAirborne) continue;

            if (attack.RequiredMotion != AttackData.MotionType.None)
            {
                int[][] requiredSequence = GetMotionSequence(attack.RequiredMotion);
    
                bool motionCompleted = buffer.CheckMotion(
                    requiredSequence, 
                    attack.RequiredButton, 
                    _fighter.FacingDirection, 
                    10
                );

                if (motionCompleted)
                {
                    return attack;
                }
            }
            else 
            {
                if (!buffer.IsInputPressed(attack.RequiredButton)) continue;

                if (CheckCommandDirection(attack.RequiredDirection, buffer))
                {
                    return attack;
                }
            }
        }
        
        return null;
    }

    private int[][] GetMotionSequence(AttackData.MotionType motion)
    {
        if (motion == AttackData.MotionType.DP) return _motionDP;
        if (motion == AttackData.MotionType.QCF) return _motionQCF;
        if (motion == AttackData.MotionType.QCB) return _motionQCB;
        
        return new int[0][];
    }
    
    private bool CheckCommandDirection(AttackData.CommandDirection requiredDir, InputBuffer buffer)
    {
        if (requiredDir == AttackData.CommandDirection.Neutral)
        {
            return !buffer.IsInputActive(InputBuffer.InputFlag.Left) && 
                   !buffer.IsInputActive(InputBuffer.InputFlag.Right) && 
                   !buffer.IsInputActive(InputBuffer.InputFlag.Down) && 
                   !buffer.IsInputActive(InputBuffer.InputFlag.Up);
        }

        InputBuffer.InputFlag forwardFlag = (_fighter.FacingDirection == 1) ? InputBuffer.InputFlag.Right : InputBuffer.InputFlag.Left;
        InputBuffer.InputFlag backFlag = (_fighter.FacingDirection == 1) ? InputBuffer.InputFlag.Left : InputBuffer.InputFlag.Right;

        if (requiredDir == AttackData.CommandDirection.Forward) return buffer.IsInputActive(forwardFlag);
        if (requiredDir == AttackData.CommandDirection.Back) return buffer.IsInputActive(backFlag);
        if (requiredDir == AttackData.CommandDirection.Down) return buffer.IsInputActive(InputBuffer.InputFlag.Down);
        if (requiredDir == AttackData.CommandDirection.Up) return buffer.IsInputActive(InputBuffer.InputFlag.Up);

        return false;
    }
}