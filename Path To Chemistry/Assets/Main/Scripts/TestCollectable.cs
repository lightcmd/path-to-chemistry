using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestCollectable : MonoBehaviour, ICollectable
{
    public Dictionary<string, int> returnMaterials { get; set; }
    private bool onCollectedInitialized = false;
    private float alphaCT = 0f;
    private bool called = false;
    private ParticleSystemRenderer particleSystemRenderer;

    void Awake()
    {
        returnMaterials = new Dictionary<string, int>()
        {
            {"Crystal", 5},
            {"Gold", 5}
        };

        particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        particleSystemRenderer.mesh = GetComponent<MeshFilter>().mesh;
        particleSystemRenderer.material = new Material(GetComponent<Renderer>().material);
    }

    // void Initialize()
    // {
    //     onCollectedInitialized = true;
    // }

    public void Collect()
    {
        // if (!onCollectedInitialized) Initialize();
        // particleSystem
        called = true;
        while (alphaCT < 0.8f)
        {
            alphaCT += 0.01f;
            transform.GetComponent<MeshRenderer>().material.SetFloat("Alpha_Clip_Threshold", alphaCT);
            
            GetComponent<ParticleSystem>().Play();
            return;
        }
        
        Return();
        Destroy(gameObject);
    }

    public void Return()
    {
        
    }
    
    private void Update()
    {
        if (!called && GetComponent<ParticleSystem>().isPlaying)
        {
            GetComponent<ParticleSystem>().Stop();
        }
        else
        {
            called = false;
        }
    }
}