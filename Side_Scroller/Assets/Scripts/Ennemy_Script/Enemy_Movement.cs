using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{
    public float speed = 2f;
    public Vector3 pointA;
    public Vector3 pointB;



    public bool dir = false;
    private bool prevDir;

        void Update()
        {
            // Déplacer vers le point courant (dans le plan XY)
            Vector3 target = dir ? pointB : pointA;
            target.z = 0f;

            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            // Forcer Z = 0 pour éviter toute dérive sur l'axe Z
            Vector3 pos = transform.position;
            pos.z = 0f;
            transform.position = pos;

            // Vérification de la distance dans le plan XY pour changer de direction
            Vector2 currentXY = new Vector2(transform.position.x, transform.position.y);
            Vector2 aXY = new Vector2(pointA.x, pointA.y);
            Vector2 bXY = new Vector2(pointB.x, pointB.y);

            if (Vector2.Distance(currentXY, aXY) < 0.1f && !dir)
            {
                dir = true;
            }
            else if (Vector2.Distance(currentXY, bXY) < 0.1f && dir)
            {
                dir = false;
            }

        }
    }


