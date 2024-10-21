using System;
namespace SynicSugar.Samples.Tank
{
    public enum Direction
    {
        Up, Left, Down, Right
    }

    internal static class DirectionAngles
    {
        internal const int Up = 0, Right = 90, Down = 180, Left = 270;
        internal static float GetValue(Direction direction) => direction switch
        {
            Direction.Up => Up,
            Direction.Right => Right,
            Direction.Down => Down,
            Direction.Left => Left,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), $"Not expected direction value: {direction}"),
        };
    }
}