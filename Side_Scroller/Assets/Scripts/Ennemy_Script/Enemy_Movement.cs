using System;
using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{
    public float speed = 2f;
    public Vector3 pointA;
    public Vector3 pointB;

    // false -> va vers pointA, true -> va vers pointB
    public bool dir = false;

    // tolérance pour considérer qu'on a atteint un point
    public float arriveTolerance = 0.1f;

    private void Start()
    {
        // Oriente initialement vers la cible courante
        Vector3 initialTarget = dir ? pointB : pointA;
        FlipTowards(initialTarget);
    }

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

        if (Vector2.Distance(currentXY, aXY) < arriveTolerance && !dir)
        {
            // Arrivé à A alors on change pour aller vers B
            dir = true;
            FlipTowards(pointB);
        }
        else if (Vector2.Distance(currentXY, bXY) < arriveTolerance && dir)
        {
            // Arrivé à B alors on change pour aller vers A
            dir = false;
            FlipTowards(pointA);
        }
        else
        {
            // Tant qu'on se déplace, s'assurer que l'ennemi regarde la cible actuelle
            FlipTowards(target);
        }
    }

    // Oriente le sprite pour regarder vers "target" (sur l'axe X).
    // Ne modifie la scale que si la cible est significativement à gauche/droite.
    private void FlipTowards(Vector3 target)
    {
        float dx = target.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.01f) return; // pas de changement si la cible est presque verticalement alignée

        float sign = Mathf.Sign(dx);
        Vector3 ls = transform.localScale;
        ls.x = Mathf.Abs(ls.x) * sign; // applique -1 ou +1 selon la direction
        transform.localScale = ls;
    }
}


