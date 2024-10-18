using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

// Type de joueur
public enum PlayerType { NONE, RED, YELLOW }

// Position dans la grille
public struct GridPos
{
    public int row;
    public int col;
}

public class Plateau
{
    // Plateau de jeu
    PlayerType[][] playerBoard;

    // Position actuelle dans la grille
    GridPos currentPos;

    public Plateau()
    {
        // Initialisation du plateau vide
        playerBoard = new PlayerType[6][];
        for (int i = 0; i < playerBoard.Length; i++)
        {
            playerBoard[i] = new PlayerType[7];
            for (int j = 0; j < playerBoard[i].Length; j++)
            {
                playerBoard[i][j] = PlayerType.NONE;
            }
        }
    }

    // Met à jour le plateau avec le dernier coup joué
    public void UpdateBoard(int col, bool isRed)
    {
        int updatePos = 6;
        for (int i = 5; i >= 0; i--)
        {
            if (playerBoard[i][col] == PlayerType.NONE)
            {
                updatePos--;
            }
            else
            {
                break;
            }
        }

        playerBoard[updatePos][col] = isRed ? PlayerType.RED : PlayerType.YELLOW;
        currentPos = new GridPos { row = updatePos, col = col };
    }

    public bool Result(bool isRed, int col)
    {
        PlayerType current = isRed ? PlayerType.RED : PlayerType.YELLOW;

        // recherche de la ligne du pion joué
        if (col < 0) return false;
        int row = -1;
        for (int r = 5; r > -1; r--)
        {
            if (playerBoard[r][col] == current)
            {
                row = r;
                break;
            }
        }
        // ligne non trouvée
        if (row == -1) return false;

        // vérifie les alignements
        return CheckDirection(row, col, 1, 0, current) || // Horizontal
               CheckDirection(row, col, 0, 1, current) || // Vertical
               CheckDirection(row, col, 1, 1, current) || // Diagonale
               CheckDirection(row, col, 1, -1, current);  // Diagonale inverse
    }

    private bool CheckDirection(int row, int col, int dRow, int dCol, PlayerType player)
    {
        int count = 1;

        // vérifie dans la direction positive
        count += CountPions(row, col, dRow, dCol, player);
        // vérifie dans la direction négative
        count += CountPions(row, col, -dRow, -dCol, player);

        return count >= 4;
    }

    private int CountPions(int row, int col, int dRow, int dCol, PlayerType player)
    {
        int count = 0;

        // Parcourir la grille dans une direction donnée
        while (true)
        {
            row += dRow;
            col += dCol;

            if (row < 0 || row >= 6 || col < 0 || col >= 7)
            {
                break;
            }

            if (playerBoard[row][col] != player)
            {
                break;
            }

            count++;
        }

        return count;
    }

    // Vérifie si la grille est pleine
    public bool IsFull()
    {
        for (int i = 0; i < 7; i++)
        {
            if (IsColumnNotFull(i))
            {
                return false;
            }
        }
        return true;
    }

    // Indique si la colonne col est pleine
    public bool IsColumnNotFull(int col)
    {
        return playerBoard[5][col] == PlayerType.NONE;
    }

    // Renvoie un nouveau plateau qui est une copie du plateau actuel
    public Plateau Clone()
    {
        Plateau newP = new Plateau();
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                newP.playerBoard[i][j] = playerBoard[i][j];
            }
        }
        return newP;
    }



    // -----------------------------------
    // Fonctions pour MinMax et Alpha-Beta
    // -----------------------------------

    public int MinMax(Plateau p, int depth, bool isRed)
    {
        Tuple<int, int> eval_action = JoueurMax(p, depth, -1, isRed);
        
        return eval_action.Item2;
    }

    public Tuple<int, int> JoueurMax(Plateau p, int depth, int lastMove, bool isRed)
    {
        if (depth == 0 || p.Result(!isRed, lastMove))
        {
            return new Tuple<int, int>(p.Evaluate(isRed), -1);
        }

        int maxEval = int.MinValue;
        int bestAction = -1;

        // Initialise bestAction à la première colonne non pleine
        for(int i = 0; i< 7; i++)
        {
            if(p.IsColumnNotFull(i))
            {
                bestAction = i;
            }
        }
        for (int i = 0; i < 7; i++)
        {
            if (p.IsColumnNotFull(i))
            {
                Plateau newP = p.Clone();
                newP.UpdateBoard(i, isRed);
                int eval = JoueurMin(newP, depth - 1, i, isRed).Item1;
                if (eval > maxEval)
                {
                    maxEval = eval;
                    bestAction = i;
                }
            }
        }

        return new Tuple<int, int>(maxEval, bestAction);
    }

    public Tuple<int, int> JoueurMin(Plateau p, int depth, int lastMove, bool isRed)
    {
        if (depth == 0 || p.Result(!isRed, lastMove))
        {
            return new Tuple<int, int>(p.Evaluate(isRed), -1);
        }

        int minEval = int.MaxValue;
        int bestAction = -1;
        for (int i = 0; i < 7; i++)
        {
            if (p.IsColumnNotFull(i))
            {
                bestAction = i;
            }
        }
        for (int i = 0; i < 7; i++)
        {
            if (p.IsColumnNotFull(i))
            {
                Plateau newP = p.Clone();
                newP.UpdateBoard(i, !isRed);
                int eval = JoueurMax(newP, depth - 1, i, isRed).Item1;
                if (eval < minEval)
                {
                    minEval = eval;
                    bestAction = i;
                }
            }
        }

        return new Tuple<int, int>(minEval, bestAction);
    }



    public int AlphaBeta(Plateau p, int depth, bool isRed)
    {
        int alpha = int.MinValue;
        int beta = int.MaxValue;
        
        Tuple<int, int> eval_action = JoueurMax(p, depth, -1, isRed, alpha, beta);
        
        return eval_action.Item2;
    }


    public Tuple<int, int> JoueurMax(Plateau p, int depth, int lastMove, bool isRed, int alpha, int beta)
    {
        if (depth == 0 || p.Result(!isRed, lastMove))
        {
            return new Tuple<int, int>(p.Evaluate(isRed), -1);
        }

        int maxEval = int.MinValue;
        int bestAction = -1;
        for (int i = 0; i < 7; i++)
        {
            if (p.IsColumnNotFull(i))
            {
                bestAction = i;
            }
        }

        for (int i = 0; i < 7; i++)
        {
            if (p.IsColumnNotFull(i))
            {
                Plateau newP = p.Clone();
                newP.UpdateBoard(i, isRed);
                int eval = JoueurMin(newP, depth - 1, i, isRed, alpha, beta).Item1;
                if (eval > maxEval)
                {
                    maxEval = eval;
                    bestAction = i;
                }
                alpha = Math.Max(alpha, maxEval); 
                if (alpha >= beta)
                {
                    return new Tuple<int, int>(maxEval, bestAction);
                }
            }
        }
        return new Tuple<int, int>(maxEval, bestAction);
    }

    public Tuple<int, int> JoueurMin(Plateau p, int depth, int lastMove, bool isRed, int alpha, int beta)
    {
        if (depth == 0 || p.Result(!isRed, lastMove))
        {
            return new Tuple<int, int>(p.Evaluate(isRed), -1);
        }

        int minEval = int.MaxValue;
        int bestAction = -1;
        for (int i = 0; i < 7; i++)
        {
            if (p.IsColumnNotFull(i))
            {
                bestAction = i;
            }
        }
        for (int i = 0; i < 7; i++)
        {
            if (p.IsColumnNotFull(i))
            {
                Plateau newP = p.Clone();
                newP.UpdateBoard(i, !isRed);
                int eval = JoueurMax(newP, depth - 1, i, isRed, alpha, beta).Item1;
                if (eval < minEval)
                {
                    minEval = eval;
                    bestAction = i;
                }
                beta = Math.Min(beta, minEval);
                if (beta <= alpha)
                {
                    return new Tuple<int, int>(minEval, bestAction);
                }
            }
        }
        return new Tuple<int, int>(minEval, bestAction);
    }

    // Evalue une grille à l'aide de la fonction EvaluateLine
    public int Evaluate(bool isRed)
    {
        int value = 0;
        for (int i = 0; i < 6; i++)
        { //on parcourt toutes les lignes (deux fois, une fois pour chaque joueur)
            //row puis col
            value += EvaluateLine(i, 0, 0, 1, PlayerType.RED, isRed); // horizontal pour 1er joueur
            value += EvaluateLine(i, 0, 0, 1, PlayerType.YELLOW, isRed); // horizontal pour 2eme joueur
        }
        for (int j = 0; j < 7; j++)
        { //on parcourt toutes les colonnes (deux fois, une fois pour chaque joueur)
            value += EvaluateLine(0, j, 1, 0, PlayerType.RED, isRed); // vertical pour 1er joueur
            value += EvaluateLine(0, j, 1, 0, PlayerType.YELLOW, isRed); // vertical pour 2eme joueur
        }
        for (int i = 0; i < 7; i++) //on parcourt toutes les lignes puis on parcourt les diagonales inversées
        {
            //Parcours de la première moitié des diagonales
            value += EvaluateLine(0, i, 1, -1, PlayerType.RED, isRed); // diagonale
            value += EvaluateLine(0, i, 1, -1, PlayerType.YELLOW, isRed); // diagonale
        }
        for (int i = 1; i < 6; i++)
        {
            //parcours de la deuxième moitié des diagonales
            value += EvaluateLine(i, 6, 1, -1, PlayerType.RED, isRed); // diagonale
            value += EvaluateLine(i, 6, 1, -1, PlayerType.YELLOW, isRed); // diagonale
        }
        //parcours de la première moitié des diagonales inversées
        for (int i = 5; i > 0; i--)
        {
            value += EvaluateLine(i, 0, 1, 1, PlayerType.RED, isRed); // diagonale inversée
            value += EvaluateLine(i, 0, 1, 1, PlayerType.YELLOW, isRed); // diagonale inversée
        }
        //parcours de la deuxième moitié des diagonales inversées
        for (int i = 0; i < 7; i++)
        {
            value += EvaluateLine(0, i, 1, 1, PlayerType.RED, isRed); // diagonale inversée
            value += EvaluateLine(0, i, 1, 1, PlayerType.YELLOW, isRed); // diagonale inversée
        }
        return value;
    }

    // Evalue un certain type de ligne en fonction des paramètres
    private int EvaluateLine(int row, int col, int rowDiff, int colDiff, PlayerType player, bool isRed)
    {
        int counter = 0;
        int ponderationGagnante = 1;
        int ponderationPerdante = -1;
        int scorePour1 = 1; //1 pion et 3 espaces vides alignés
        int scorePour2 = 10; //2 pions et 2 espaces vides alignés
        int scorePour3 = 100; //3 pions et 1 espace vide alignés
        int scorePour4 = 1000; //4 pions alignés
        int emptyPlace = 0;
        int maxCounter = 0;
        int maxEmptyPlace = 0;
        bool specialCase = false;

        for (int i = 0; i < 7; i++)
        {
            int r = row + i * rowDiff;
            int c = col + i * colDiff;
            if (r >= 0 && r < 6 && c >= 0 && c < 7)
            {
                if (playerBoard[r][c] == player)
                {
                    counter++;
                }
                else
                {
                    if (playerBoard[r][c] == PlayerType.NONE)
                    {
                        emptyPlace++;
                    }
                    else
                    {
                        if (counter >= maxCounter)
                        {
                            specialCase = true;
                            maxCounter = counter;
                            maxEmptyPlace = emptyPlace;
                        }
                        counter = 0;
                        emptyPlace = 0;

                    }
                }
            }
        }

        if (!specialCase || counter > maxCounter || counter == maxCounter && emptyPlace > maxEmptyPlace)
        {
            maxCounter = counter;
            maxEmptyPlace = emptyPlace;
        }

        int ponderation;
        if ((isRed && (player == PlayerType.RED)) || (!isRed && (player == PlayerType.YELLOW)))
        {
            ponderation = ponderationGagnante;
        }
        else
        {
            ponderation = ponderationPerdante;
        }

        int value = 0;
        switch (maxCounter)
        {
            case 1:
                if (maxEmptyPlace >= 3)
                {
                    value = scorePour1 * ponderation;
                }
                break;
            case 2:
                if (maxEmptyPlace >= 2)
                {
                    value = scorePour2 * ponderation;
                }
                break;
            case 3:
                if (maxEmptyPlace >= 1)
                {
                    value = scorePour3 * ponderation;
                }
                break;
            case 4:
                value = scorePour4 * ponderation;
                break;
        }
        return value;
    }
}
