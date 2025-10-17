using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SPAWNER : MonoBehaviour
{
    [SerializeField]
    private GameObject swarmerprefab;

    [SerializeField]
    private float swarmerInterval = 3.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(spawnEnemy(swarmerInterval, swarmerprefab));
    }

    // Update is called once per frame
   private IEnumerator spawnEnemy(float interval, GameObject enemy)
    {
        yield return new WaitForSeconds(interval);
        GameObject newEnemy = Instantiate(enemy, new Vector3(Random.Range(-5f, 5), Random.Range(-6f, 6), 0), Quaternion.identity);
        StartCoroutine(spawnEnemy(interval, enemy));
    }    
}
