using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoEnemySpawner : MonoBehaviour {
    private static List<DemoEnemy> m_SpawnedEnemies = new List<DemoEnemy>();
    [SerializeField] Transform[] m_SpawnPoints;
    [SerializeField] private GameObject m_DemoEnemyPrefab;
    [SerializeField] private Transform m_TargetTransform;
    [SerializeField] private float m_SpawnDuration = 1f;

    public static DemoEnemy[] SpawnedEnemies {
        get {
            return m_SpawnedEnemies.ToArray();
        }
    }

    IEnumerator Start() {
        int spawnIndex = 0;

        while(true) {
            Transform spawnPoint = m_SpawnPoints[spawnIndex];
            GameObject enemyObject = Instantiate(m_DemoEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
            DemoEnemy enemy = enemyObject.GetComponent<DemoEnemy>();
            enemy.Track(m_TargetTransform.position);
            enemy.OnDead.AddListener(targetEnemy => m_SpawnedEnemies.Remove(targetEnemy));

            m_SpawnedEnemies.Add(enemy);

            yield return new WaitForSeconds(m_SpawnDuration);

            spawnIndex++;

            if (spawnIndex >= m_SpawnPoints.Length) {
                spawnIndex = 0;
            }
        }
    }
}
