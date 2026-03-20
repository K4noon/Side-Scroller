using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generate : MonoBehaviour
{
    public GameObject Chunk1;
    public GameObject Chunk2;
    public GameObject Chunk3;
    public GameObject Chunk4;
    public GameObject Chunk5;
    public GameObject Chunk6;
    public GameObject Chunk7;
    public GameObject Chunk8;
    public GameObject Chunk9;
    public GameObject Chunk10;
    public GameObject Chunk11;
    public GameObject Chunk12;
    public GameObject Chunk13;
    public GameObject Chunk14;
    public GameObject Chunk15;

    // Intervalle entre chaque instanciation (en secondes)
    public float spawnInterval = 1f;

    // Intervalle entre chaque suppression automatique du plus ancien clone (en secondes)
    public float destroyInterval = 5f;

    // Délai avant la première suppression automatique (en secondes)
    public float destroyStartDelay = 25f;

    // Optionnel : parent pour organiser la hiérarchie des objets instanciés
    public Transform spawnRoot;

    // Position X courante ; chaque nouvel objet est placé +21 par rapport à la précédente
    private float currentSpawnX;

    // File pour suivre l'ordre des instances créées (utile pour supprimer le plus ancien)
    private Queue<GameObject> spawnedQueue = new Queue<GameObject>();

    // Index du dernier chunk instancié (1..15)
    private int lastChunkIndex = 0;

    void Start()
    {
        currentSpawnX = transform.position.x;
        // Spawn immédiat du premier chunk qui doit être obligatoirement Chunk1
        SpawnChunk(1, Chunk1);
        lastChunkIndex = 1;

        // Démarre la boucle de spawn pour la suite (aléatoire)
        StartCoroutine(SpawnRoutine());

        // Démarre la boucle de suppression indépendante (démarre après destroyStartDelay)
        StartCoroutine(DeleteRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Construire la liste des indices autorisés selon les règles
            List<int> allowed = new List<int>();
            for (int i = 1; i <= 15; i++)
            {
                if (!IsForbiddenSequence(lastChunkIndex, i))
                    allowed.Add(i);
            }

            int chunkIndex;
            if (allowed.Count == 0)
            {
                // Cas improbable : aucune option autorisée -> retombe sur sélection libre pour éviter blocage
                chunkIndex = Random.Range(1, 16); // 1..15 inclus
            }
            else
            {
                chunkIndex = allowed[Random.Range(0, allowed.Count)];
            }

            GameObject prefab = GetChunkByIndex(chunkIndex);
            if (prefab == null) continue;

            SpawnChunk(chunkIndex, prefab);
        }
    }

    // Instancie et met à jour lastChunkIndex
    private void SpawnChunk(int index, GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 spawnPos = new Vector3(currentSpawnX + 21f, transform.position.y, transform.position.z);
        GameObject instance = Instantiate(prefab, spawnPos, prefab.transform.rotation);
        instance.SetActive(true);

        if (spawnRoot != null)
            instance.transform.SetParent(spawnRoot, true);

        // Enqueue pour garder l'ordre des clones créés
        spawnedQueue.Enqueue(instance);

        currentSpawnX += 21f;
        lastChunkIndex = index;
    }

    // Coroutine indépendante : attend destroyStartDelay avant la première suppression,
    // puis supprime le plus ancien clone toutes les destroyInterval secondes.
    private IEnumerator DeleteRoutine()
    {
        // Attente initiale avant la première suppression
        yield return new WaitForSeconds(destroyStartDelay);

        while (true)
        {
            if (spawnedQueue.Count > 0)
            {
                GameObject oldest = spawnedQueue.Dequeue();
                if (oldest != null)
                {
                    Destroy(oldest);
                }
            }

            yield return new WaitForSeconds(destroyInterval);
        }
    }

    // Règles interdites :
    // - Les chunks 5, 9, 10 et 12 ne peuvent pas être générés deux fois d'affilée.
    // - Les paires consécutives interdites (dans les deux sens) : 10 <-> 5, 5 <-> 9, 10 <-> 9.
    // - Le chunk 12 ne peut pas être immédiatement suivi par 10, 13 ou 14.
    // - Les chunks 10, 14 et 15 ne peuvent pas se suivre entre eux (aucune paire consécutive parmi ces trois).
    private bool IsForbiddenSequence(int previous, int candidate)
    {
        if (previous == 0) return false; // pas de contrainte pour le premier spawn si non initialisé

        // Interdire doublons pour 5,9,10,12
        if ((previous == 5 || previous == 9 || previous == 10 || previous == 12) && previous == candidate)
            return true;

        // Interdire paires suivantes (les deux sens)
        // 10 <-> 5
        if ((previous == 10 && candidate == 5) || (previous == 5 && candidate == 10))
            return true;
        // 5 <-> 9
        if ((previous == 5 && candidate == 9) || (previous == 9 && candidate == 5))
            return true;
        // 10 <-> 9
        if ((previous == 10 && candidate == 9) || (previous == 9 && candidate == 10))
            return true;

        // Le chunk 12 ne peut pas être avant 10, 13, 14
        if (previous == 12 && (candidate == 10 || candidate == 13 || candidate == 14))
            return true;

        // Interdire que 10, 14 et 15 se suivent entre eux (toutes les paires dans le groupe)
        if ((previous == 10 || previous == 14 || previous == 15) &&
            (candidate == 10 || candidate == 14 || candidate == 15))
            return true;

        return false;
    }

    private GameObject GetChunkByIndex(int index)
    {
        switch (index)
        {
            case 1: return Chunk1;
            case 2: return Chunk2;
            case 3: return Chunk3;
            case 4: return Chunk4;
            case 5: return Chunk5;
            case 6: return Chunk6;
            case 7: return Chunk7;
            case 8: return Chunk8;
            case 9: return Chunk9;
            case 10: return Chunk10;
            case 11: return Chunk11;
            case 12: return Chunk12;
            case 13: return Chunk13;
            case 14: return Chunk14;
            case 15: return Chunk15;
            default: return null;
        }
    }
}
