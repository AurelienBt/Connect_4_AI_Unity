using UnityEngine;

public class Mouvement : MonoBehaviour
{
    // Vitesse
    [SerializeField] // permet d'accéder aux variables dans l'Inspector de Unity
    float speed = 10f;

    // Position cible
    public Vector3 targetPostion;

    // Fonction appelée à chaque frame
    void Update()
    {
        // Fait bouger le jeton
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPostion, step);
    }
}
