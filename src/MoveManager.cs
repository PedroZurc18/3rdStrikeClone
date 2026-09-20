using System.Collections.Generic;
using Godot;

namespace rdStrikeClone;

public partial class MoveManager : Node2D
{
    private Fighter _fighter;
    
    private List<NormalAttack> _cachedMoves = new List<NormalAttack>();
    
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
        
        CacheMovesRecursively(this);
    }
    
    private void CacheMovesRecursively(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            // If it's an attack, add it to our list
            if (child is NormalAttack attackNode)
            {
                _cachedMoves.Add(attackNode);
            }
            
            // If this node has children (like your SpecialAttacks folder), dig inside it!
            if (child.GetChildCount() > 0)
            {
                CacheMovesRecursively(child);
            }
        }
    }
    
    public NormalAttack EvaluateAvailableMoves(InputBuffer buffer, bool isAirborne)
    {
        foreach (NormalAttack attackNode in _cachedMoves)
        {
            if (attackNode.IsAirborneMove != isAirborne) continue;

            if (attackNode is SpecialAttack specialNode)
            {
                int[][] requiredSequence = GetMotionSequence(specialNode.RequiredMotion);
    
                bool motionCompleted = buffer.CheckMotion(
                    requiredSequence, 
                    specialNode.RequiredButton, 
                    _fighter.FacingDirection, 
                    30
                );

                if (motionCompleted)
                {
                    return specialNode;
                }
            }
            else 
            {
                if (!buffer.IsInputPressed(attackNode.RequiredButton)) continue;

                bool directionMatches = CheckCommandDirection(attackNode.RequiredDirection, buffer);
                if (directionMatches)
                {
                    return attackNode;
                }
            }
        }
        
        return null;
    }
    
    private int[][] GetMotionSequence(SpecialAttack.MotionType motion)
    {
        if (motion == SpecialAttack.MotionType.DP) return _motionDP;
        if (motion == SpecialAttack.MotionType.QCF) return _motionQCF;
        if (motion == SpecialAttack.MotionType.QCB) return _motionQCB;
        
        return new int[0][];
    }
    
    private bool CheckCommandDirection(NormalAttack.CommandDirection requiredDir, InputBuffer buffer)
    {
        if (requiredDir == NormalAttack.CommandDirection.Neutral)
        {
            return !buffer.IsInputActive(InputBuffer.InputFlag.Left) && 
                   !buffer.IsInputActive(InputBuffer.InputFlag.Right) && 
                   !buffer.IsInputActive(InputBuffer.InputFlag.Down) && 
                   !buffer.IsInputActive(InputBuffer.InputFlag.Up);
        }

        InputBuffer.InputFlag forwardFlag = (_fighter.FacingDirection == 1) ? InputBuffer.InputFlag.Right : InputBuffer.InputFlag.Left;
        InputBuffer.InputFlag backFlag = (_fighter.FacingDirection == 1) ? InputBuffer.InputFlag.Left : InputBuffer.InputFlag.Right;

        if (requiredDir == NormalAttack.CommandDirection.Forward) return buffer.IsInputActive(forwardFlag);
        if (requiredDir == NormalAttack.CommandDirection.Back) return buffer.IsInputActive(backFlag);
        if (requiredDir == NormalAttack.CommandDirection.Down) return buffer.IsInputActive(InputBuffer.InputFlag.Down);
        if (requiredDir == NormalAttack.CommandDirection.Up) return buffer.IsInputActive(InputBuffer.InputFlag.Up);

        return false;
    }
}