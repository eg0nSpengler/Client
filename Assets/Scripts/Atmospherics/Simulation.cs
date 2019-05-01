using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using Atmospherics;
using Atmospherics.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Entity = Unity.Entities.Entity;
using Random = UnityEngine.Random;

public class Simulation : MonoBehaviour
{
    [SerializeField] private GasData[] gasData = new GasData[0];

    private const float NormalTemp = 293.15f;
    private const float NormalPres = 101325;

    private EntityManager manager;
    private NativeArray<Entity> gasses;

    [SerializeField] private int wid = 1;
    [SerializeField] private int hei = 2;

    private void OnEnable()
    {
        if (manager == null) manager = World.Active.GetOrCreateManager<EntityManager>();
        var gasArchetype = manager.CreateArchetype(typeof(GridPosition), typeof(Gas));

        World.Active.GetOrCreateManager<AtmosphericsSystem>().RegisterGasses(gasData);

        gasses = new NativeArray<Entity>(wid*hei*gasData.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            
        manager.CreateEntity(gasArchetype, gasses);

        var maxMoles = Moles(2, NormalPres, NormalTemp);
        var nitrogen = maxMoles * 0.78f;
        var oxygen = maxMoles * 0.22f;
        
        for (var x = 0; x < wid; x++)
        for (var y = 0; y < hei; y++)
        for (byte i = 0; i < gasData.Length; i++)
        {
            var gas = gasses[i + x * gasData.Length + y * gasData.Length * wid];
            manager.SetComponentData(gas, new GridPosition (new int3(x, 0, y)));

            if(i == 0) manager.SetComponentData(gas, new Gas(i, nitrogen*Random.value*4, NormalTemp));
            if(i == 1) manager.SetComponentData(gas, new Gas(i, oxygen*Random.value*4, NormalTemp));
        }
    }

    private void OnDisable()
    {
        if (!gasses.IsCreated) return;
        if (Application.isPlaying)
            manager.DestroyEntity(gasses);
        gasses.Dispose();
    }

    private void OnApplicationQuit()
    {
        if (gasses.IsCreated) gasses.Dispose();
    }

    [Pure]
    public static float Moles(float volume, float pressure, float temperature)
        => temperature > 0 ? (pressure * volume) / (AtmosphericsSystem.GasConstant * temperature) : 0;

    /*private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        foreach (var entity in gasses)
        {
            Gizmos.color = new Color(0, 0, 0, 0.3f);
            var pos = manager.GetComponentData<GridPosition>(entity);
            var gas = manager.GetComponentData<Gas>(entity);
            Gizmos.DrawWireCube((float3)pos.value, new Vector3(1, 0, 1));


            var w = 1f/2;
            var h = gas.partialPressure * 0.003f;
            Gizmos.color = new Color(0, 1, 1, 1f);
            Gizmos.DrawCube(pos.value + new float3(-w/2 + w * gas.id, h/2, -1/3f), new Vector3(w, h, 1/3f));
            
            
            h = gas.energy / (gasData[gas.id].heatCapacity * gas.moles) * 0.4f;
            Gizmos.color = new Color(1, 0, 0, 1f);
            Gizmos.DrawCube(pos.value + new float3(-w/2 + w * gas.id, h/2, 0f), new Vector3(w, h, 1/3f));

            switch (gas.id)
            {
            case 0:
                Gizmos.color = new Color(1, 1, 0, 1f);
                break;
            case 1:
                Gizmos.color = new Color(0, 0, 1, 1f);
                break;
            }

            h = gas.moles * 0.02f;
            Gizmos.DrawCube(pos.value + new float3(-w/2 + w * gas.id, h/2, 1/3f), new Vector3(w, h, 1/3f));
        }
    }*/
}