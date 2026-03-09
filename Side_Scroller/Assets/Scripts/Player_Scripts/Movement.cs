using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    public float Speed = 10;
    private float Jump = 0;
    public float JumpForce = 1;
    public Rigidbody2D rb;


    // Check si touche le sol
    public bool bGrounded = false;

    // Double jump (global)
    public int MaxJumps = 2;
    private int jumpsRemaining;

    // Détection murale
    private bool touchingWall = false;

    // Limite par GameObject wall : permet 2 sauts par GameObject (persistant jusqu'au contact sol)
    public int MaxWallJumps = 2;
    private Dictionary<int, int> wallJumpRemaining = new Dictionary<int, int>(); // key = instanceID du mur
    private int currentWallId = 0;

    private void Start()
    {
        jumpsRemaining = MaxJumps;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject);
        if (collision.gameObject.CompareTag("Ground")) //si touche le sol
        {
            bGrounded = true;
            touchingWall = false;
            currentWallId = 0;
            jumpsRemaining = MaxJumps; // reset des sauts disponibles
            wallJumpRemaining.Clear(); // réinitialise les compteurs muraux au contact du sol
        }
        else if (collision.gameObject.CompareTag("WALL"))
        {
            // En entrant en contact avec un mur, mémorise l'ID du mur et initialise son compteur s'il n'existe pas
            touchingWall = true;
            bGrounded = false;
            currentWallId = collision.gameObject.GetInstanceID();
            if (!wallJumpRemaining.ContainsKey(currentWallId))
            {
                wallJumpRemaining[currentWallId] = MaxWallJumps;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            bGrounded = false;
        }
        else if (collision.gameObject.CompareTag("WALL"))
        {
            // Quitter le mur : on n'autorise plus l'usage du compteur de ce mur tant qu'on ne le retouche pas
            touchingWall = false;
            currentWallId = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float translation = Input.GetAxis("Horizontal") * Speed; //recupere les inputs
        translation *= Time.deltaTime;
        transform.Translate(translation, 0, 0); //deplace le joueur

        // Utiliser GetButtonDown pour éviter le saut continu si la touche est maintenue
        if (Input.GetButtonDown("Jump"))
        {
            bool allowed = false;

            // Si on est en contact avec un mur : autorisé seulement si ce mur a des sauts restants
            if (touchingWall && currentWallId != 0 && wallJumpRemaining.ContainsKey(currentWallId) && wallJumpRemaining[currentWallId] > 0)
            {
                allowed = true;
            }
            // Sinon, autoriser selon le compteur global de sauts (double jump standard)
            else if (jumpsRemaining > 0)
            {
                allowed = true;
            }

            if (allowed)
            {
                Jump = JumpForce;
                // stabiliser la vélocité verticale avant d'ajouter l'impulsion pour des sauts cohérents
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(new Vector2(0, Jump), ForceMode2D.Impulse); //ajoute une force vers le haut

                // Décrémente le compteur mural si applicable
                if (touchingWall && currentWallId != 0 && wallJumpRemaining.ContainsKey(currentWallId) && wallJumpRemaining[currentWallId] > 0)
                {
                    wallJumpRemaining[currentWallId]--;
                }

                // Décrémente le compteur global de sauts si disponible
                if (jumpsRemaining > 0)
                {
                    jumpsRemaining--;
                }

                bGrounded = false;
            }
        }
    }
}
