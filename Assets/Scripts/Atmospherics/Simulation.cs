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
            new AtmosTile {moles = new[] {nitrogen * 2, 0}, temperature = NormalTemp},
            new AtmosTile {moles = new[] {0, oxygen * 2}, temperature = NormalTemp / 4},
        };

        StartCoroutine(Step());

//        Debug.Log("BEFORE");
//        PrintTiles();
//
//        var difference = Mathf.Abs(tiles[0].TotalMoles - tiles[1].TotalMoles);
//        Move(difference / 2, ref tiles[0], ref tiles[1]);
//
//        Debug.Log("AFTER");
//        PrintTiles();
    }

    private IEnumerator Step()
    {
        yield return new WaitForSeconds(5);
        while (!Stable())
        {
            yield return null;
            
            PrintTiles();

            pressure0Curve.AddKey(Time.time, Pressure(2, tiles[0].TotalMoles, tiles[0].temperature));
            pressure1Curve.AddKey(Time.time, Pressure(2, tiles[1].TotalMoles, tiles[1].temperature));
            Debug.Log(velocity[0]+ " "+velocity[1]);
            
            Process(0);
            Process(1);
        }
    }

    private bool Stable()
    {
        const float threshold = 1E-4f;
        
        var pressure00 = tiles[0].moles[0] / tiles[0].TotalMoles * Pressure(2, tiles[0].TotalMoles, tiles[0].temperature);
        var pressure10 = tiles[1].moles[0] / tiles[1].TotalMoles * Pressure(2, tiles[1].TotalMoles, tiles[1].temperature);
        var pressure01 = tiles[0].moles[1] / tiles[0].TotalMoles * Pressure(2, tiles[0].TotalMoles, tiles[0].temperature);
        var pressure11 = tiles[1].moles[1] / tiles[1].TotalMoles * Pressure(2, tiles[1].TotalMoles, tiles[1].temperature);
        Debug.Log(threshold + " " +Math.Abs(pressure00 - pressure10) +" "+ Math.Abs(pressure01 - pressure11) );
        return Math.Abs(pressure00 - pressure10) < threshold && Math.Abs(pressure01 - pressure11) < threshold;
    }

    private void Process(int gas)
    {
        var mass0 = tiles[0].moles[gas] * gasDefinitions[gas].mass;
        var mass1 = tiles[1].moles[gas] * gasDefinitions[gas].mass;
        
        var pressure0 = tiles[0].moles[gas] / tiles[0].TotalMoles * Pressure(2, tiles[0].TotalMoles, tiles[0].temperature);
        var pressure1 = tiles[1].moles[gas] / tiles[1].TotalMoles * Pressure(2, tiles[1].TotalMoles, tiles[1].temperature);
        
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
        
        var fromMoles = from.TotalMoles;
        var toMoles = to.TotalMoles;
        var fromEnergy = Energy(fromMoles, from.temperature);
        var toEnergy = Energy(toMoles, to.temperature);

        if (amount <= 0) return;

        from.moles[gas] -= amount;
        to.moles[gas] += amount;

        var energy = Energy(amount, from.temperature);

        from.temperature = Temperature(from.TotalMoles, fromEnergy - energy);
        to.temperature = Temperature(to.TotalMoles, toEnergy + energy);
    }


    private void MoveAndMix(float amount, ref AtmosTile from, ref AtmosTile to)
    {
        var fromMoles = from.TotalMoles;
        var toMoles = to.TotalMoles;
        var totalEnergy = Energy(fromMoles, from.temperature) + Energy(toMoles, to.temperature);
        var totalMoles = fromMoles + toMoles;

        if (totalMoles <= 0) return;

        for (var i = 0; i < gasDefinitions.Length; i++)
        {
            var total = from.moles[i] + to.moles[i];
            from.moles[i] = total * (fromMoles - amount) / totalMoles;
            to.moles[i] = total * (toMoles + amount) / totalMoles;
        }

        from.temperature = Temperature(from.TotalMoles, totalEnergy * from.TotalMoles / totalMoles);
        to.temperature = Temperature(to.TotalMoles, totalEnergy * to.TotalMoles / totalMoles);
    }

    private void PrintTiles()
    {
        for (var index = 0; index < tiles.Length; index++)
        {
            var tile = tiles[index];
            var i = 0;
            Debug.LogFormat("[ Node: {4} Moles: {0:N1}, Pressure: {1:N1}, Temperature {2:N1}, Content: [{3} ] ]",
                tile.TotalMoles,
                Pressure(2, tile.TotalMoles, tile.temperature),
                tile.temperature,
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
    public static float Energy(float moles, float temperature)
        => 1.5f * GasConstant * temperature * moles;

    [Pure]
    public static float Temperature(float moles, float energy)
        => moles > 0 ? energy / (1.5f * GasConstant * moles) : 0;

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