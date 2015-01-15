using EETuring.Physics;

namespace EETuring
{
    public class PlayerNode
    {
        /// <summary>
        /// Input used to obtain node
        /// </summary>
        public Input Input { get; set; }

        /// <summary>
        /// Player physics state
        /// </summary>
        public PlayerState State { get; set; }

        /// <summary>
        /// X Block coordinate
        /// </summary>
        public int BX { get { return State.BlockX; } }

        /// <summary>
        /// Y Block coordinate
        /// </summary>
        public int BY { get { return State.BlockY; } }

        /// <summary>
        /// X coordinate
        /// </summary>
        public double X { get { return State.X; } }

        /// <summary>
        /// Y coordinate
        /// </summary>
        public double Y { get { return State.Y; } }

        /// <summary>
        /// Heuristic cost
        /// </summary>
        public double F { get; set; }

        public bool IsGoal(Point g)
        {
            return State.BlockX == g.x && State.BlockY == g.y;
        }

        public override string ToString()
        {
            return State.ToString();
        }

        /// <summary>
        /// Player node
        /// </summary>
        public PlayerNode(Input input, PlayerState state)
        {
            Input = input;
            State = state;
        }
    }
}
