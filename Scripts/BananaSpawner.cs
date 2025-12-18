using UnityEngine;

public class BananaSpawner : MonoBehaviour
{
    [Header("Prefab de la Banana")]
    public GameObject bananaPrefab;

    [Header("Cantidad por grupo")]
    public int minBananasPerGroup = 3;
    public int maxBananasPerGroup = 7;

    [Header("Tiempo entre grupos")]
    public float minTimeBetweenGroups = 4f;
    public float maxTimeBetweenGroups = 30f;

    [Header("Formación del grupo")]
    public float separationBetweenBananas = 1.2f;   
    public Vector2 spawnDirection = Vector2.right; 
    public float timeBetweenEachBanana = 0.15f;    

    private float nextGroupTime;

    void Start()
    {
        if (bananaPrefab == null)
        {
            Debug.LogError("BananaSpawner: Falta el bananaPrefab.");
            enabled = false;
            return;
        }

        ScheduleNextGroup();
    }

    void Update()
    {
        if (Time.time >= nextGroupTime)
        {
            StartCoroutine(SpawnBananaGroupCoroutine());
            ScheduleNextGroup();
        }
    }

    private System.Collections.IEnumerator SpawnBananaGroupCoroutine()
    {
        int bananaCount = Random.Range(minBananasPerGroup, maxBananasPerGroup + 1);

        Vector3 currentPosition = transform.position;

        for (int i = 0; i < bananaCount; i++)
        {
            Instantiate(bananaPrefab, currentPosition, transform.rotation);

            currentPosition += (Vector3)(spawnDirection.normalized * separationBetweenBananas);

            if (i < bananaCount - 1)  
            {
                yield return new WaitForSeconds(timeBetweenEachBanana);
            }
        }
    }

    private void ScheduleNextGroup()
    {
        float delay = Random.Range(minTimeBetweenGroups, maxTimeBetweenGroups);
        nextGroupTime = Time.time + delay;
    }
}