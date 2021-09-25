using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnOnDistance : MonoBehaviour
{
    public ObjectGenerationSettings objectGenerationSettings;
    private List<GameObject> generatedObjects = new List<GameObject>();
    private Transform player;

    void Awake()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[0].transform;
        foreach (Transform child in transform)
        {
            generatedObjects.Add(child.gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(DisableOnDistance());
    }

    IEnumerator DisableOnDistance()
    {
        while (true)
        {
            foreach (GameObject obj in generatedObjects)
            {
                if (Vector3.Distance(player.position, obj.transform.position) >= objectGenerationSettings.despawnDistance)
                {
                    obj.SetActive(false);
                }
                else
                {
                    obj.SetActive(true);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }
}