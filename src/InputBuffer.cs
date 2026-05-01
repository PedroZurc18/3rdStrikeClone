using System;
using Godot;

public partial class InputBuffer : Node
{
    [Export]
    public int PlayerId = 1;

    // 1. Possible inputs using flags
    [Flags]
    public enum InputFlag
    {
        Neutral = 0,
        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        Punch = 1 << 4,
        Kick = 1 << 5,
    }

    // 2. Structure for a single frame
    public struct InputFrame
    {
        public InputFlag State;
        public int FrameNumber;
    }

    // 3. The Ring Buffer
    private const int BufferSize = 60;
    private InputFrame[] _buffer;
    private int _headIndex = 0;
    private int _currentFrame = 0;
    private InputFlag _previousInputState = InputFlag.Neutral;

    public override void _Ready()
    {
        _buffer = new InputFrame[BufferSize];
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentFrame++;

        InputFlag currentInput = GetCurrentInputState();
        AddInputToBuffer(currentInput);

        if (currentInput != _previousInputState)
        {
            if (PlayerId == 1)
            {
                // GD.Print(currentInput);
            }
            _previousInputState = currentInput;
        }
    }

    private InputFlag GetCurrentInputState()
    {
        InputFlag state = InputFlag.Neutral;

        if (Input.IsActionPressed($"p{PlayerId}_up"))
            state |= InputFlag.Up;
        if (Input.IsActionPressed($"p{PlayerId}_down"))
            state |= InputFlag.Down;
        if (Input.IsActionPressed($"p{PlayerId}_left"))
            state |= InputFlag.Left;
        if (Input.IsActionPressed($"p{PlayerId}_right"))
            state |= InputFlag.Right;

        if (Input.IsActionPressed($"p{PlayerId}_punch"))
            state |= InputFlag.Punch;
        if (Input.IsActionPressed($"p{PlayerId}_kick"))
            state |= InputFlag.Kick;

        return state;
    }

    private void AddInputToBuffer(InputFlag state)
    {
        _headIndex = (_headIndex + 1) % BufferSize;

        _buffer[_headIndex] = new InputFrame { State = state, FrameNumber = _currentFrame };
    }

    public bool IsInputActive(InputFlag flag)
    {
        return (_buffer[_headIndex].State & flag) == flag;
    }

    public bool IsNeutral()
    {
        if (GetCurrentInputState() == InputFlag.Neutral)
        {
            return true;
        }
        return false;
    }

    public bool IsInputPressed(InputFlag flag)
    {
        bool isPressedNow = (_buffer[_headIndex].State & flag) == flag;

        int prevIndex = _headIndex - 1;
        if (prevIndex < 0)
        {
            prevIndex = BufferSize - 1;
        }

        bool wasPressedBefore = (_buffer[prevIndex].State & flag) == flag;

        return isPressedNow && !wasPressedBefore;
    }

    public bool WasInputPressedWithin(InputFlag flag, int frameWindow)
    {
        frameWindow = Math.Min(frameWindow, BufferSize - 1);

        for (int i = 0; i <= frameWindow; i++)
        {
            int currentIndex = (_headIndex - i + BufferSize) % BufferSize;

            int prevIndex = (currentIndex - 1 + BufferSize) % BufferSize;

            bool wasPressedAtCurrent = (_buffer[currentIndex].State & flag) == flag;
            bool wasPressedAtPrev = (_buffer[prevIndex].State & flag) == flag;

            if (wasPressedAtCurrent && !wasPressedAtPrev)
            {
                return true;
            }
        }
        return false;
    }

    public bool WasInputReleasedWithin(InputFlag flag, int frameWindow)
    {
        frameWindow = Math.Min(frameWindow, BufferSize - 1);

        for (int i = 0; i <= frameWindow; i++)
        {
            int currentIndex = (_headIndex - i + BufferSize) % BufferSize;
            int prevIndex = (currentIndex - 1 + BufferSize) % BufferSize;

            bool isPressedNow = (_buffer[currentIndex].State & flag) == flag;
            bool wasPressedPrev = (_buffer[prevIndex].State & flag) == flag;

            if (!isPressedNow && wasPressedPrev)
                return true;
        }
        return false;
    }

    public int GetNumpadDirection(InputFlag state, int facingDirection)
    {
        bool down = (state & InputFlag.Down) == InputFlag.Down;
        bool up = (state & InputFlag.Up) == InputFlag.Up;
        bool left = (state & InputFlag.Left) == InputFlag.Left;
        bool right = (state & InputFlag.Right) == InputFlag.Right;

        bool forward = (facingDirection == 1) ? right : left;
        bool back = (facingDirection == 1) ? left : right;

        if (up && back)
            return 7;
        if (up && forward)
            return 9;
        if (up)
            return 8;
        if (down && back)
            return 1;
        if (down && forward)
            return 3;
        if (down)
            return 2;
        if (back)
            return 4;
        if (forward)
            return 6;

        return 5;
    }

    public bool CheckMotion(
        int[] expectedMotion,
        InputFlag requiredButton,
        int facingDirection,
        int frameWindow = 8
    )
    {
        frameWindow = Math.Min(frameWindow, BufferSize - 1);

        if (!WasInputPressedWithin(requiredButton, 3))
            return false;

        int motionIndex = expectedMotion.Length - 1;

        for (int i = 0; i <= frameWindow; i++)
        {
            int index = (_headIndex - i + BufferSize) % BufferSize;
            InputFrame frame = _buffer[index];

            int frameDir = GetNumpadDirection(frame.State, facingDirection);

            if (frameDir == expectedMotion[motionIndex])
            {
                motionIndex--;
                if (motionIndex < 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public string GetDebugHistory()
    {
        string history = "--- INPUT HISTORY ---\n";

        int framesToDisplay = 20;

        for (int i = 0; i < framesToDisplay; i++)
        {
            int index = (_headIndex - i + BufferSize) % BufferSize;
            InputFlag state = _buffer[index].State;

            string stateText = (state == InputFlag.Neutral) ? "Neutral" : state.ToString();

            if (i == 0)
            {
                history += $"[NOW] : {stateText}\n";
            }
            else
            {
                history += $"[{i.ToString().PadLeft(2)}] : {stateText}\n";
            }
        }

        return history;
    }
}
