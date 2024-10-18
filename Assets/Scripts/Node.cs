using System.Collections.Generic;

public struct Score
{
    public int Win;
    public int Lose;
    public int Draw;
}

public class Node
{
    public Node Parent;
    public List<Node> Children;
    public Score Score;
    public int Visits;
    public int Move;
    public bool IsRed;
    public Plateau State;
    public bool IsPlaying;

    public Node(Node parent, int move, bool isRed, Plateau state, bool isPlaying)
    {
        Parent = parent;
        Children = new List<Node>();
        Score = new Score { Win = 0, Lose = 0, Draw = 0 };
        Visits = 1;
        Move = move;
        IsRed = isRed;
        State = state;
        IsPlaying = isPlaying;
    }

    public bool IsFullyExpanded()
    {
        return Children.Count == GetLegalMoves(State).Count;
    }

    public List<int> GetLegalMoves(Plateau state)
    {
        List<int> legalMoves = new List<int>();
        for (int col = 0; col < 7; col++)
        {
            if (state.IsColumnNotFull(col))
            {
                legalMoves.Add(col);
            }
        }
        return legalMoves;
    }
}