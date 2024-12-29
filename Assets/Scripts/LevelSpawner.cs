using System.Collections.Generic;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    public static LevelSpawner Instance;

    [SerializeField] private List<GameObject> _levels;
    [SerializeField] private List<GameObject> _spawnedLevels;
    private Vector3 _spawnPoint;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        EventManager.LevelSpawned += SpawnLevel;
    }

    private void OnDisable()
    {
        EventManager.LevelSpawned -= SpawnLevel;
    }

    public void SpawnLevel()
    {
        if (_levels.Count == 0)
        {
            Debug.LogWarning("No levels available in the pool!");
            return;
        }

        GameObject levelPrefab = _levels[Random.Range(0, _levels.Count)];

        _spawnPoint = new Vector3(levelPrefab.transform.position.x + 150, levelPrefab.transform.position.y, levelPrefab.transform.position.z);

        GameObject spawnedLevel = Instantiate(levelPrefab, _spawnPoint, Quaternion.identity);
        _spawnedLevels.Add(spawnedLevel);

        Debug.Log($"Spawned level: {spawnedLevel.name}");
    }

    public void RemoveLastLevel()
    {
        if (_spawnedLevels.Count == 0)
        {
            Debug.LogWarning("No levels to remove!");
            return;
        }

        GameObject lastLevel = _spawnedLevels[0];
        _spawnedLevels.RemoveAt(0);
        Destroy(lastLevel);

        Debug.Log("Removed last level.");
    }
}
