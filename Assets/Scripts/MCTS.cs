using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class MCTS
{
    private static System.Random random = new System.Random();

    public Node Select(Node node, double c)
    {
        while (node.Children.Count > 0)
        {
            node = BestUCT(node, c);
        }

        return node;
    }

    public Node BestUCT(Node node, double c)
    {
        Node bestNode = null;
        double bestValue = double.MinValue;

        foreach (var child in node.Children)
        {
            int wins = child.IsPlaying ? child.Score.Win : child.Score.Lose;
            double intensification = (double)child.Score.Win / (child.Visits);
            double diversification = c * System.Math.Sqrt(2* System.Math.Log((double) node.Visits) / (child.Visits));
            double uctValue = intensification + diversification;

            if (uctValue > bestValue)
            {
                bestValue = uctValue;
                bestNode = child;
            }
        }
        return bestNode;
    }

    public Node Expand(Node node)
    {
        if (node.State.Result(node.IsRed, node.Move)) return node; //grille gagnante

        List<int> legalMoves = node.GetLegalMoves(node.State);
        if (legalMoves.Count == 0) return node;

        foreach (var move in legalMoves)
        {
            Plateau newState = node.State.Clone();
            newState.UpdateBoard(move, node.IsRed);
            Node childNode = new Node(node, move, !node.IsRed, newState, !node.IsPlaying);
            node.Children.Add(childNode);
        }

        return node.Children[random.Next(node.Children.Count)];
    }

    public int Simulate(Node node) // DefaultPolicy
    {
        Plateau state = node.State.Clone();
        bool isRed = node.IsRed;
        int move = node.Move;

        while (!state.IsFull() && !state.Result(isRed, move))
        {
            List<int> legalMoves = node.GetLegalMoves(state);
            if (legalMoves.Count == 0) break;

            move = legalMoves[random.Next(legalMoves.Count)];
            state.UpdateBoard(move, isRed);
            isRed = !isRed; // changement de joueur
        }

        if (state.Result(isRed, move) || state.Result(!isRed, move))
        {
            if (node.IsPlaying) return 0; // win
            else return 1; // lose
        }
        else
        {
            return 2; // draw
        }
    }

    public void Backpropagate(Node node, int result)
    {
        while (node != null)
        {
            node.Visits++;
            switch(result)
            {
                case 0:
                    if (node.IsPlaying) node.Score.Lose++;
                    else node.Score.Win++;
                    break;
                case 1:
                    if (node.IsPlaying) node.Score.Win++;
                    else node.Score.Lose++;
                    break;
                case 2:
                    node.Score.Draw++;
                    break;
            }
            node = node.Parent;
        }
    }

    public int GetBestMove(Plateau state, bool isRed, int iterations, double c = 1.41)
    {
        Node root = new Node(null, -1, isRed, state, true);
        for (int i = 0; i < iterations; i++)
        {
            Node selectedNode = Select(root, c);
            Node expandedNode = Expand(selectedNode);
            int result = Simulate(expandedNode);
            Backpropagate(expandedNode, result);
        }

        // étude de l'arbre
        /*
        int numberOfLevels = GetNumberOfLevels(root);
        int[] nodesPerLevel = GetNumberOfNodesPerLevel(root, numberOfLevels);
        int totalNodes = GetTotalNodes(root);
        double balanceScore = GetBalanceScore(root);

        UnityEngine.Debug.Log($"Number of Levels: {numberOfLevels}");
        for(int i = 0; i<nodesPerLevel.Length; i++)
        {
            UnityEngine.Debug.Log($"Level {i}: {nodesPerLevel[i]} nodes");
        }
        UnityEngine.Debug.Log($"Number of nodes: {totalNodes}");

        UnityEngine.Debug.Log($"Balance Score: {balanceScore}");
        */
        return BestChild(root).Move;
    }

    public Node BestChild(Node node)
    {
        Node bestNode = null;
        double bestValue = double.MinValue;
        foreach (var child in node.Children)
        {
            double winRate = (double)child.Score.Win / child.Visits;
            if (winRate > bestValue)
            {
                bestValue = winRate;
                bestNode = child;
            }
        }
        return bestNode;
    }

    public int GetNumberOfLevels(Node root)
    {
        if (root == null)
            return 0;

        int maxDepth = 0;
        Queue<(Node, int)> queue = new Queue<(Node, int)>();
        queue.Enqueue((root, 1));

        while (queue.Count > 0)
        {
            var (node, depth) = queue.Dequeue();
            maxDepth = maxDepth<depth ? depth : maxDepth;

            foreach (Node child in node.Children)
            {
                queue.Enqueue((child, depth + 1));
            }
        }

        return maxDepth;
    }

    public int[] GetNumberOfNodesPerLevel(Node root, int numberOfLevels)
    {
        int[] nodesPerLevel = new int[numberOfLevels];

        if (root == null)
            return nodesPerLevel;

        Queue<(Node, int)> queue = new Queue<(Node, int)>();
        queue.Enqueue((root, 0));

        while (queue.Count > 0)
        {
            var (node, level) = queue.Dequeue();
            nodesPerLevel[level]++;
            foreach (var child in node.Children)
            {
                queue.Enqueue((child, level + 1));
            }
        }

        return nodesPerLevel;
    }

    public int GetTotalNodes(Node root)
    {
        if (root == null) return 0;

        int totalNodes = 0;
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            totalNodes++;

            foreach (Node child in node.Children)
            {
                queue.Enqueue(child);
            }
        }

        return totalNodes;
    }

    public double GetBalanceScore(Node root)
    {
        if (root == null) return 0;

        int numberOfLevels = GetNumberOfLevels(root);
        int totalNodes = GetTotalNodes(root);
        int[] nodesPerLevel = GetNumberOfNodesPerLevel(root, numberOfLevels);

        double averageNodesPerLevel = (double)totalNodes / numberOfLevels;
        double balanceScore = 0;

        for (int i = 0; i < numberOfLevels; i++)
        {
            balanceScore += Math.Abs(averageNodesPerLevel - nodesPerLevel[i]);
        }

        return balanceScore / numberOfLevels;
    }

}