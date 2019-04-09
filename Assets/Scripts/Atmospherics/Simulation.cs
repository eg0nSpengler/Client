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

public class Simulation : MonoBehaviour
{
    private const float GasConstant = 8.31445984848f;

    [FormerlySerializedAs("gasses")] [SerializeField] private GasDefinition[] gasDefinitions = new GasDefinition[0];

    private AtmosTile[] tiles;

    private const float NormalTemp = 293.15f;
    private const float NormalPres = 101325;

    private EntityManager manager;
    private NativeArray<Entity> nodes;

    private readonly float[] velocity = new float[2];
    
    [SerializeField] private AnimationCurve pressure0Curve = AnimationCurve.Constant(0,0,0);
    [SerializeField] private AnimationCurve pressure1Curve = AnimationCurve.Constant(0,0,0);

    private void OnEnable()
    {
        if (manager == null) manager = World.Active.GetOrCreateManager<EntityManager>();
        var nodeArchetype = manager.CreateArchetype(typeof(GridPosition), typeof(AtmosphericsNode));
        var gasArchetype = manager.CreateArchetype(typeof(GridPosition), typeof(Gas));

        nodes = new NativeArray<Entity>(2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            
        manager.CreateEntity(nodeArchetype, nodes);

        var maxMoles = Moles(2, NormalPres, NormalTemp);
        var nitrogen = maxMoles * 0.78f;
        var oxygen = maxMoles * 0.22f;
        
        for (var i = 0; i < nodes.Length; i++)
        {
            manager.SetComponentData(nodes[i], new GridPosition (new int3(i % 4, 0, i / 4)));
            manager.SetComponentData(nodes[i], new AtmosphericsNode (NormalTemp));

            for (byte j = 0; j < gasDefinitions.Length; j++)
            {
                var gas = manager.CreateEntity(gasArchetype);
                manager.SetComponentData(gas, new GridPosition (new int3(i % 4, 0, i / 4)));

                if (i == 0)
                {
                    if(j == 0) manager.SetComponentData(gas, new Gas(j, nitrogen*2));
                    if(j == 1) manager.SetComponentData(gas, new Gas(j, 0));
                }
                if (i == 1)
                {
                    if(j == 0) manager.SetComponentData(gas, new Gas(j, 0));
                    if(j == 1) manager.SetComponentData(gas, new Gas(j, oxygen*2));
                }
            }
        }
    }

    private void OnDisable()
    {
        if (!nodes.IsCreated) return;
        if (Application.isPlaying)
            manager.DestroyEntity(nodes);
        nodes.Dispose();
    }

    private void OnApplicationQuit()
    {
        if (nodes.IsCreated) nodes.Dispose();
    }

    private void Start()
    {
        var maxMoles = Moles(2, NormalPres, NormalTemp);
        var nitrogen = maxMoles * 0.78f;
        var oxygen = maxMoles * 0.22f;

        tiles = new[]
        {
            new AtmosTile {moles = new[] {nitrogen * 2, 0}, temperature = new []{NormalTemp,0}},
            new AtmosTile {moles = new[] {0, oxygen * 2}, temperature = new []{0,NormalTemp / 4}},
        };

        StartCoroutine(Step());
    }

    private IEnumerator Step()
    {
        yield return new WaitForSeconds(5);
        while (!Stable())
        {
            yield return null;
            
            PrintTiles();

            pressure0Curve.AddKey(Time.time, Pressure(2, tiles[0].TotalMoles, Temperature(tiles[0])));
            pressure1Curve.AddKey(Time.time, Pressure(2, tiles[1].TotalMoles, Temperature(tiles[1])));
            Debug.Log(velocity[0]+ " "+velocity[1]);
            
            Process(0);
            Process(1);

            HeatTransfer();
        }
    }

    private void HeatTransfer()
    {
        var k = 0;
        var amount = -k * Time.deltaTime / 2;
    }

    private bool Stable()
    {
        const float threshold = 1E-4f;
        
        var pressure00 = tiles[0].moles[0] / tiles[0].TotalMoles * Pressure(2, tiles[0].TotalMoles, Temperature(tiles[0]));
        var pressure10 = tiles[1].moles[0] / tiles[1].TotalMoles * Pressure(2, tiles[1].TotalMoles, Temperature(tiles[1]));
        var pressure01 = tiles[0].moles[1] / tiles[0].TotalMoles * Pressure(2, tiles[0].TotalMoles, Temperature(tiles[0]));
        var pressure11 = tiles[1].moles[1] / tiles[1].TotalMoles * Pressure(2, tiles[1].TotalMoles, Temperature(tiles[1]));
        Debug.Log(threshold + " " +Math.Abs(pressure00 - pressure10) +" "+ Math.Abs(pressure01 - pressure11) );
        return Math.Abs(pressure00 - pressure10) < threshold && Math.Abs(pressure01 - pressure11) < threshold;
    }

    private void Process(int gas)
    {
        var mass0 = tiles[0].moles[gas] * gasDefinitions[gas].mass;
        var mass1 = tiles[1].moles[gas] * gasDefinitions[gas].mass;
        
        var pressure0 = tiles[0].moles[gas] / tiles[0].TotalMoles * Pressure(2, tiles[0].TotalMoles, Temperature(tiles[0]));
        var pressure1 = tiles[1].moles[gas] / tiles[1].TotalMoles * Pressure(2, tiles[1].TotalMoles, Temperature(tiles[1]));
        
        if (tiles[0].moles[1] <= 0 && velocity[gas] > 0) velocity[gas] = 0;
        if (tiles[1].moles[1] <= 0 && velocity[gas] < 0) velocity[gas] = 0;
        
        var force1 = (pressure0 - pressure1) / 10;
        if (force1 > 0)
        {
            var acceleration = mass0 > 0 ? force1 / mass0 : 0;
    
            velocity[gas] += 0.5f * acceleration * Time.deltaTime;
            
            var amount = 2 * (velocity[gas] * Time.deltaTime + 0.5f * acceleration * Time.deltaTime * Time.deltaTime);
            Move(amount, gas, ref tiles[0], ref tiles[1]);
        }
        else
        {
            force1 = -force1;
            
            var acceleration = mass1 > 0 ? force1 / mass1 : 0;
    
            velocity[gas] -= acceleration * Time.deltaTime;
        
            var amount = 2 * (-velocity[gas] * Time.deltaTime - 0.5f * acceleration * Time.deltaTime * Time.deltaTime );
            Move(amount, gas, ref tiles[1], ref tiles[0]);
        }
        
        velocity[gas] *= 0.95f;
    }


    private void Move(float amount, int gas, ref AtmosTile from, ref AtmosTile to)
    {
        if (amount < 0)
        {
            Move(-amount, gas, ref to, ref from);
            return;
        }
        
        amount = Mathf.Min(amount, from.moles[gas]);
        
        var fromEnergy = Energy(gas, from.moles[gas], from.temperature[gas]);
        var toEnergy = Energy(gas, to.moles[gas], to.temperature[gas]);
        
        if (amount <= 0) return;

        var energy = Energy(gas, amount, from.temperature[gas]);
        
        from.moles[gas] -= amount;
        to.moles[gas] += amount;

        from.temperature[gas] = Temperature(gas, from.moles[gas], fromEnergy - energy);
        to.temperature[gas] = Temperature(gas, to.moles[gas], toEnergy + energy);
    }

    private void PrintTiles()
    {
        for (var index = 0; index < tiles.Length; index++)
        {
            var tile = tiles[index];
            var i = 0;
            Debug.LogFormat("[ Node: {4} Moles: {0:N1}, Pressure: {1:N1}, Temperature: [{2} ], Content: [{3} ] ]",
                tile.TotalMoles,
                Pressure(2, tile.TotalMoles, Temperature(tile)),
                tile.temperature.Aggregate("", (s, t) => $"{s} {t:N2}"),
                tile.moles.Aggregate("", (s, n) => $"{s} {gasDefinitions[i++].name}:{n / tile.TotalMoles:N2}"),
                index);
        }
    }

    [Pure]
    public static float Pressure(float volume, float moles, float temperature)
        => (moles * GasConstant * temperature) / volume;

    [Pure]
    public static float Volume(float pressure, float moles, float temperature)
        => (moles * GasConstant * temperature) / pressure;

    [Pure]
    public static float Moles(float volume, float pressure, float temperature)
        => temperature > 0 ? (pressure * volume) / (GasConstant * temperature) : 0;

    [Pure]
    public static float Temperature(float volume, float pressure, float moles)
        => moles > 0 ? (pressure * volume) / (GasConstant * moles) : 0;

    [Pure]
    public float Energy(int gasIndex, float moles, float temperature)
        => gasDefinitions[gasIndex].heatCapacity * temperature * moles;

    [Pure]
    public static float Energy(float moles, float temperature)
        => 1.5f * GasConstant * temperature * moles;

    [Pure]
    public float Temperature(int gasIndex, float moles, float energy)
        => moles > 0 ? energy / (gasDefinitions[gasIndex].heatCapacity * moles) : 0;
    
    [Pure]
    public static float Temperature(float moles, float energy)
        => moles > 0 ? energy / (1.5f * GasConstant * moles) : 0;

    [Pure]
    public float HeatEnergy(AtmosTile tile)
    {
        var total = 0f;
        for (var i = 0; i < tile.moles.Length; i++)
            total += gasDefinitions[i].heatCapacity * tile.moles[i] * tile.temperature[i];
        return total;
    }

    [Pure]
    public float Temperature(AtmosTile tile)
    {
        var totalMoles = 0f;
        var totalEnergy = 0f;
        for (var i = 0; i < tile.moles.Length; i++)
        {
            totalMoles += tile.moles[i];
            totalEnergy += gasDefinitions[i].heatCapacity * tile.moles[i] * tile.temperature[i];
        }
        return Temperature(totalMoles, totalEnergy);
    }
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = new Color(0, 0, 0, 0.3f);
        foreach (var entity in nodes)
        {
            var pos = manager.GetComponentData<GridPosition>(entity);
            Gizmos.DrawWireCube((float3)pos.value, new Vector3(1, 0, 1));
        }
    }
}