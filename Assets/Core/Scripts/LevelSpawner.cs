using System.Collections.Generic;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    public static LevelSpawner Instance;

    [SerializeField] private GameObject _playerMovement;

    [SerializeField] private List<GameObject> _levels;
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
        EventManager.LevelCompleted += SpawnLevel;
    }

    private void OnDisable()
    {
        EventManager.LevelCompleted -= SpawnLevel;
    }

    public void SpawnLevel()
    {
        if (_levels.Count == 0)
        {
            Debug.LogWarning("No levels available in the pool!");
            return;
        }

        GameObject levelPrefab = _levels[Random.Range(0, _levels.Count)];

        _spawnPoint = new Vector3(_playerMovement.transform.position.x + 75, levelPrefab.transform.position.y, levelPrefab.transform.position.z);

        GameObject spawnedLevel = Instantiate(levelPrefab, _spawnPoint, Quaternion.identity);

        Debug.Log($"Spawned level: {spawnedLevel.name}");
    }
}
