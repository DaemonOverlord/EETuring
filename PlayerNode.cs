using EETuring.Physics;

namespace EETuring
{
    public class PlayerNode
    {
        public Input Input { get; set; }

        public PlayerState State { get; set; }

        public int BX { get { return State.BlockX; } }
        public int BY { get { return State.BlockY; } }
        public double X { get { return State.X; } }
        public double Y { get { return State.Y; } }

        public double F { get; set; }

        public bool IsGoal(Point g)
        {
            return State.BlockX == g.x && State.BlockY == g.y;
        }

        public override string ToString()
        {
            return State.ToString();
        }

        public PlayerNode(Input input, PlayerState state)
        {
            Input = input;
            State = state;
        }
    }
}
