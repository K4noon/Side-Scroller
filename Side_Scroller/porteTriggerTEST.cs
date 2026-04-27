
using UnityEngine;

public class porteTrigger : MonoBehaviour
{
    // Attribuer dans l'inspector ou laisser vide pour recherche automatique au trigger
    public Generate generator;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ne réagir qu'au joueur (assure-toi que le Player a le tag "Player")
        if (!collision.CompareTag("Player"))
            return;

        if (generator == null)
        {
            // Recherche de l'instance Generate dans la scène si non assignée
            generator = FindObjectOfType<Generate>();
            if (generator == null)
                return; // aucun Generate trouvé
        }

        // Demande à Generate de (re)lancer la génération
        generator.StartGeneration();
    }
}