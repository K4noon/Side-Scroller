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
    public GameObject ChunkPAUSE;

    // Intervalle entre chaque instanciation (en secondes)
    public float spawnInterval = 1f;

    // Intervalle entre chaque suppression automatique du plus ancien clone (en secondes)
    public float destroyInterval = 5f;

    // Délai avant la première suppression automatique (en secondes)
    public float destroyStartDelay = 25f;

    // Durée du chrono après l'instanciation de ChunkPAUSE (modifiable dans l'Inspector)
    public float pauseDuration = 30f;

    // Délai entre la reprise de la génération et la suppression du ChunkPAUSE (modifiable dans l'Inspector)
    public float pauseRemovalDelay = 5f;

    // Optionnel : parent pour organiser la hiérarchie des objets instanciés
    public Transform spawnRoot;

    // Nombre maximum de chunks (parmi 1..15) à instancier avant d'instancier ChunkPAUSE
    public int maxChunksToSpawn = 5;

    // Position X courante ; chaque nouvel objet est placé +21 par rapport à la précédente
    private float currentSpawnX;

    // File pour suivre l'ordre des instances créées (utile pour supprimer le plus ancien)
    private Queue<GameObject> spawnedQueue = new Queue<GameObject>();

    // Index du dernier chunk instancié (1..15)
    private int lastChunkIndex = 0;

    // Compteur des chunks générés (ne compte que les chunks 1..15, pas ChunkPAUSE)
    private int generatedChunksCount = 0;

    // Indique si le ChunkPAUSE a déjà été instancié pour la pause en cours
    private bool pauseSpawned = false;

    // Référence à l'instance actuelle de ChunkPAUSE pour pouvoir la détruire plus tard
    private GameObject currentPauseInstance;

    // Chunks interdits tant que ChunkPAUSE n'a pas été instancié
    private static readonly HashSet<int> restrictedBeforePause = new HashSet<int> { 5, 9, 10, 12, 14, 15 };

    // Coroutines référencées pour éviter doublons
    private Coroutine spawnCoroutine;
    private Coroutine deleteCoroutine;
    private Coroutine pauseCoroutine;
    private Coroutine removePauseCoroutine;

    // Indique si la génération (spawn loop) est active
    public bool Generating { get; private set; } = false;

    void Start()
    {
        currentSpawnX = transform.position.x;
        // Spawn immédiat du premier chunk qui doit être obligatoirement Chunk1
        SpawnChunk(1, Chunk1); // ceci incrémente generatedChunksCount

        // Si on a déjà atteint le maximum, instancier ChunkPAUSE et ne pas démarrer la coroutine de spawn
        if (generatedChunksCount >= Mathf.Max(1, maxChunksToSpawn))
        {
            SpawnPause();
            // Démarre quand même la routine de suppression (si nécessaire)
            if (deleteCoroutine == null)
                deleteCoroutine = StartCoroutine(DeleteRoutine());
            Generating = false;
            return;
        }

        // Démarre la boucle de spawn pour la suite (aléatoire)
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnRoutine());
            Generating = true;
        }

        // Démarre la boucle de suppression indépendante (démarre après destroyStartDelay)
        if (deleteCoroutine == null)
            deleteCoroutine = StartCoroutine(DeleteRoutine());
    }

    // Méthode publique pour (re)lancer la génération depuis un autre script (ex : porteTrigger)
    public void StartGeneration()
    {
        if (Generating)
            return;

        // Si on a déjà atteint le maximum, instancier ChunkPAUSE et ne pas démarrer la coroutine de spawn
        if (generatedChunksCount >= Mathf.Max(1, maxChunksToSpawn))
        {
            if (!pauseSpawned)
                SpawnPause();

            if (deleteCoroutine == null)
                deleteCoroutine = StartCoroutine(DeleteRoutine());

            Generating = false;
            return;
        }

        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnRoutine());
            Generating = true;
        }

        if (deleteCoroutine == null)
            deleteCoroutine = StartCoroutine(DeleteRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Si on a atteint le maximum de chunks autorisés, instancier ChunkPAUSE puis arrêter le spawn
            if (generatedChunksCount >= Mathf.Max(1, maxChunksToSpawn))
            {
                if (!pauseSpawned)
                {
                    SpawnPause();
                }
                // Marquer fin de génération
                Generating = false;
                spawnCoroutine = null;
                yield break;
            }

            // Construire la liste des indices autorisés selon les règles
            List<int> allowed = new List<int>();
            for (int i = 1; i <= 15; i++)
            {
                // Exclure les chunks restreints tant que le ChunkPAUSE n'est pas spawn
                if (!pauseSpawned && restrictedBeforePause.Contains(i))
                    continue;

                if (!IsForbiddenSequence(lastChunkIndex, i))
                    allowed.Add(i);
            }

            int chunkIndex;
            if (allowed.Count == 0)
            {
                // Fallback : essayer une sélection qui respecte au moins la restriction avant pause
                List<int> fallback = new List<int>();
                for (int i = 1; i <= 15; i++)
                {
                    if (!pauseSpawned && restrictedBeforePause.Contains(i))
                        continue;
                    fallback.Add(i);
                }

                if (fallback.Count > 0)
                {
                    chunkIndex = fallback[Random.Range(0, fallback.Count)];
                }
                else
                {
                    // Cas extrême : autoriser toute sélection pour éviter blocage (devrait être rare)
                    chunkIndex = Random.Range(1, 16); // 1..15 inclus
                }
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

        // Compter uniquement les chunks 1..15
        if (index >= 1 && index <= 15)
            generatedChunksCount++;
    }

    // Instancie ChunkPAUSE (une seule fois par pause) et NE L'ENQUEUE PAS pour l'exclure de la suppression automatique
    private void SpawnPause()
    {
        if (pauseSpawned || ChunkPAUSE == null) return;

        Vector3 spawnPos = new Vector3(currentSpawnX + 21f, transform.position.y, transform.position.z);
        GameObject instance = Instantiate(ChunkPAUSE, spawnPos, ChunkPAUSE.transform.rotation);
        instance.SetActive(true);

        if (spawnRoot != null)
            instance.transform.SetParent(spawnRoot, true);

        // IMPORTANT : n'ajoute pas ChunkPAUSE à spawnedQueue => il ne sera pas détruit par DeleteRoutine
        currentSpawnX += 21f;
        pauseSpawned = true;

        // Stocke l'instance pour pouvoir la supprimer plus tard
        currentPauseInstance = instance;

        // Démarrer le chrono configurable pour relancer la génération après pauseDuration secondes
        if (pauseCoroutine != null)
            StopCoroutine(pauseCoroutine);
        pauseCoroutine = StartCoroutine(PauseTimer());
    }

    // Chrono après l'apparition du ChunkPAUSE : attend pauseDuration puis relance la génération.
    private IEnumerator PauseTimer()
    {
        yield return new WaitForSeconds(pauseDuration);
        pauseCoroutine = null;

        // Réinitialise le compteur pour permettre une nouvelle série de chunks
        generatedChunksCount = 0;

        // Relancer la génération si elle n'est pas déjà active
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnRoutine());
            Generating = true;
        }

        // --- Reset du timer de suppression pour préserver le tempo ---
        // On redémarre DeleteRoutine pour que la prochaine suppression attende destroyStartDelay à nouveau.
        if (deleteCoroutine != null)
        {
            StopCoroutine(deleteCoroutine);
            deleteCoroutine = null;
        }
        deleteCoroutine = StartCoroutine(DeleteRoutine());
        // --- fin reset ---

        // Après la reprise, attendre pauseRemovalDelay puis supprimer ChunkPAUSE pour laisser le joueur avancer
        if (removePauseCoroutine != null)
            StopCoroutine(removePauseCoroutine);
        removePauseCoroutine = StartCoroutine(RemovePauseAfterDelay());
    }

    // Supprime l'instance de ChunkPAUSE après pauseRemovalDelay et autorise un prochain cycle de pause
    private IEnumerator RemovePauseAfterDelay()
    {
        yield return new WaitForSeconds(pauseRemovalDelay);
        removePauseCoroutine = null;

        if (currentPauseInstance != null)
        {
            Destroy(currentPauseInstance);
            currentPauseInstance = null;
        }

        // Permettre un futur SpawnPause (cycle répétable)
        pauseSpawned = false;
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

        // Interdire doublons pour 1,5,9,10,12
        if ((previous == 1 || previous == 5 || previous == 9 || previous == 10 || previous == 12) && previous == candidate)
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

        // Le chunk 12 ne peut pas être avant 10, 13, 14 et 15
        if (previous == 12 && (candidate == 9 || candidate == 10 || candidate == 13 || candidate == 14 || candidate == 15))
            return true;

        // Le chunk 3 ne peut pas être suivi de 15, 4, 14, 9, 10, 12 et 13
        if ((previous == 3 && (candidate == 15 || candidate == 4 || candidate == 12 || candidate == 13 || candidate == 14 || candidate == 9 || candidate == 10)))
            return true;

        // Le chunk 4 ne peut pas être suivi de 14
        if ((previous == 4 && (candidate == 14)))
            return true;

        // Le chunk 2 ne peut pas être suivi de 9 et 5
        if ((previous == 2 && (candidate == 9 || candidate == 5)))
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