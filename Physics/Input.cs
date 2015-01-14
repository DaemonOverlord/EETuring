namespace EETuring.Physics
{
    /// <summary>
    /// List of possible single inputs
    /// </summary>
    public enum Input : uint
    {
        Nothing = 0,
        HoldLeft = 1 << 0,
        HoldUp = 1 << 1,
        HoldRight = 1 << 2,
        HoldDown = 1 << 3,
        HoldSpace = 1 << 4
    }
}
