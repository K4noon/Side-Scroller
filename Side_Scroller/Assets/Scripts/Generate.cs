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

    void Start()
    {
        currentSpawnX = transform.position.x;
        // Spawn immédiat du premier chunk qui doit être obligatoirement Chunk1
        SpawnChunk(Chunk1);

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

            int chunkIndex = Random.Range(1, 11); // 1..10 inclus
            GameObject prefab = GetChunkByIndex(chunkIndex);
            if (prefab == null) continue;

            SpawnChunk(prefab);
        }
    }

    private void SpawnChunk(GameObject prefab)
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
            default: return null;
        }
    }
}
