using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameType { Menu, Game }

public class GameManager : MonoBehaviour
{

    public static GameType gameType;

    bool isRed, hasGameFinished; // isPlayer sert à indiquer quel joueur doit jouer (même entre deux joueurs ou deux IA)

    public ToggleGroup toggleGroupPlayer1;
    public ToggleGroup toggleGroupPlayer2;

    private MCTS mcts;


    // Jeton rouge
    [SerializeField] // permet d'accéder aux variables dans l'inspecteur de Unity
    GameObject red;

    // Jeton jaune
    [SerializeField]
    GameObject yellow;

    // Texte indiquant qui doit jouer
    [SerializeField]
    Text TXT_Tour;

    // Couleurs de texte
    Color RED_COLOR = new Color(255, 69, 31, 255) / 255;
    Color YELLOW_COLOR = new Color(238, 228, 41, 255) / 255;

    Plateau myBoard;

    private void Awake()
    {
        isRed = true;
        hasGameFinished = false;
        myBoard = new Plateau();
        mcts = new MCTS();
    }

    // Charge la scène de menu
    public void GameStart()
    {
        gameType = GameType.Menu;
        SceneManager.LoadScene("Menu");
        GameParam.player1 = 1;
        GameParam.player2 = 1;
    }

    public void GameScene()
    {
        gameType = GameType.Game;
        SceneManager.LoadScene("Game");

        Toggle togglePlayer1;

        switch (GameParam.player1)
        {
            case 1:
                togglePlayer1 = GetToggleByName(toggleGroupPlayer1, "Player");
                break;
            case 2:
                togglePlayer1 = GetToggleByName(toggleGroupPlayer1, "MinMax");
                break;
            case 3:
                togglePlayer1 = GetToggleByName(toggleGroupPlayer1, "AlphaBeta");
                break;
            case 4:
                togglePlayer1 = GetToggleByName(toggleGroupPlayer1, "MCTS");
                break;
            default:
                togglePlayer1 = GetToggleByName(toggleGroupPlayer1, "Player");
                break;
        }

        Toggle togglePlayer2;
        switch (GameParam.player2)
        {
            case 1:
                togglePlayer2 = GetToggleByName(toggleGroupPlayer2, "Player");
                break;
            case 2:
                togglePlayer2 = GetToggleByName(toggleGroupPlayer2, "MinMax");
                break;
            case 3:
                togglePlayer2 = GetToggleByName(toggleGroupPlayer2, "AlphaBeta");
                break;
            case 4:
                togglePlayer2 = GetToggleByName(toggleGroupPlayer2, "MCTS");
                break;
            default:
                togglePlayer2 = GetToggleByName(toggleGroupPlayer2, "Player");
                break;

        }

        if (togglePlayer1 != null)
        {
            togglePlayer1.isOn = true;

        }
        if (togglePlayer2 != null)
        {
            togglePlayer2.isOn = true;
        }
    }

    private Toggle GetToggleByName(ToggleGroup group, string name)
    {
        foreach (Toggle toggle in group.GetComponentsInChildren<Toggle>())
        {
            if (toggle.name == name)
            {
                return toggle;
            }
        }
        return null;
    }

    Toggle GetActiveToggle(ToggleGroup group)
    {
        foreach (Toggle toggle in group.ActiveToggles())
        {
            return toggle;
        }
        return null;
    }

    int GetToggleMode(Toggle activeToggle)
    {
        return activeToggle.name switch
        {
            "Player" => 1,
            "MinMax" => 2,
            "AlphaBeta" => 3,
            "MCTS" => 4,
            _ => 1,
        };
    }

    public void Rejouer()
    {
        Toggle activeTogglePlayer1 = GetActiveToggle(toggleGroupPlayer1);
        Toggle activeTogglePlayer2 = GetActiveToggle(toggleGroupPlayer2);

        if (activeTogglePlayer1 != null)
        {
            GameParam.player1 = GetToggleMode(activeTogglePlayer1);
        }
        if (activeTogglePlayer2 != null)
        {
            GameParam.player2 = GetToggleMode(activeTogglePlayer2);
        }
        GameScene();
    }

    public void Quitter()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

    // Fonction appelée à chaque frame pour update le jeu
    private void Update()
    {
        // Tour de l'IA
        if ((isRed && GameParam.player1 > 1) || (!isRed && GameParam.player2 > 1))
        {
            if (hasGameFinished) return;
            // Recherche du meilleur mouvement
            int numCol;
            if(isRed)
            {
                numCol = GameParam.player1 switch
                {
                    2 => myBoard.MinMax(myBoard, 5, isRed),
                    3 => myBoard.AlphaBeta(myBoard, 8, isRed),
                    4 => mcts.GetBestMove(myBoard, true, 100000),
                    _ => myBoard.MinMax(myBoard, 5, isRed),
                };
            }
            else
            {
                numCol = GameParam.player2 switch
                {
                    2 => myBoard.MinMax(myBoard, 5, isRed),
                    3 => myBoard.AlphaBeta(myBoard, 8, isRed),
                    4 => mcts.GetBestMove(myBoard, false, 100000),
                    _ => myBoard.MinMax(myBoard, 5, isRed),
                };
            }

            // Placement du jeton
            string nomCol = "Colonne" + numCol;
            GameObject col = GameObject.Find(nomCol);
            if (col != null) {
                Colonne c = col.GetComponent<Colonne>();
                Vector3 spawnPos = c.GetComponent<Colonne>().spawnLocation;
                Vector3 targetPos = c.GetComponent<Colonne>().targetLocation;
                GameObject circle = Instantiate(isRed ? red : yellow);
                circle.transform.position = spawnPos;
                circle.GetComponent<Mouvement>().targetPostion = targetPos;

                // Augmente la hauteur min de la colonne (à cause du nouveau jeton placé)
                c.GetComponent<Colonne>().targetLocation = new Vector3(targetPos.x, targetPos.y + 54f, targetPos.z);

                // Met à jour la grille en mémoire
                myBoard.UpdateBoard(c.GetComponent<Colonne>().col - 1, isRed);

                // Vérifie s'il y a un gagnant
                if (myBoard.Result(isRed, numCol))
                {
                    TXT_Tour.text = "Le " + (isRed ? "Rouge" : "Jaune") + " Gagne !";
                    hasGameFinished = true;
                    return;
                }
                // Vérifie si le plateau est rempli
                else if (myBoard.IsFull())
                {
                    TXT_Tour.text = "Égalité !";
                    TXT_Tour.color = Color.white;
                    hasGameFinished = true;
                    return;
                }

                // Message de tour
                TXT_Tour.text = !isRed ? "Au tour du Rouge" : "Au tour du Jaune";
                TXT_Tour.color = !isRed ? RED_COLOR : YELLOW_COLOR;

                // Changement de joueur
                isRed = !isRed;
            }
            else
            {
                UnityEngine.Debug.Log(nomCol);
            }
        }
        // Tour du joueur humain
        else
        {
            // Clic de souris
            if (Input.GetMouseButtonDown(0))
            {
                if (hasGameFinished) return;

                // Raycast2D des coordonnées de la souris pour voir si l'on croise une zone de collision
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new(mousePos.x, mousePos.y);
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (!hit.collider) return;

                // Vérifie que la zone de collision touchée soit bien une colonne
                if (hit.collider.CompareTag("appui"))
                {
                    // Vérifie que l'on ne dépasse pas de la colonne (colonne déjà remplie)
                    if (hit.collider.gameObject.GetComponent<Colonne>().targetLocation.y > 350f) return;

                    // Placement du jeton
                    Vector3 spawnPos = hit.collider.gameObject.GetComponent<Colonne>().spawnLocation;
                    Vector3 targetPos = hit.collider.gameObject.GetComponent<Colonne>().targetLocation;
                    GameObject circle = Instantiate(isRed ? red : yellow);
                    circle.transform.position = spawnPos;
                    circle.GetComponent<Mouvement>().targetPostion = targetPos;

                    // Augmente la hauteur min de la colonne (à cause du nouveau jeton placé)
                    hit.collider.gameObject.GetComponent<Colonne>().targetLocation = new Vector3(targetPos.x, targetPos.y + 54f, targetPos.z);

                    // Met à jour la grille en mémoire
                    myBoard.UpdateBoard(hit.collider.gameObject.GetComponent<Colonne>().col - 1, isRed);

                    // Vérifie s'il y a un gagnant
                    if (myBoard.Result(isRed, hit.collider.gameObject.GetComponent<Colonne>().col - 1))
                    {
                        TXT_Tour.text = "Le " + (isRed ? "Rouge" : "Jaune") + " Gagne !";
                        hasGameFinished = true;
                        return;
                    }
                    // Vérifie si le plateau est rempli
                    else if (myBoard.IsFull())
                    {
                        TXT_Tour.text = "Égalité !";
                        TXT_Tour.color = Color.white;
                        hasGameFinished = true;
                        return;
                    }

                    // Message de tour
                    TXT_Tour.text = !isRed ? "Au tour du Rouge" : "Au tour du Jaune";
                    TXT_Tour.color = !isRed ? RED_COLOR : YELLOW_COLOR;

                    // Changement de joueur
                    isRed = !isRed;
                }
            }
        }
    }
}