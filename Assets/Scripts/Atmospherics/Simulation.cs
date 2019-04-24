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
    private const float BaseFlux = 10;

    [FormerlySerializedAs("gasses")] [SerializeField] private GasDefinition[] gasDefinitions = new GasDefinition[0];

    private AtmosTile[] tiles;

    private const float NormalTemp = 293.15f;
    private const float NormalPres = 101325;

    private EntityManager manager;
    private NativeArray<Entity> gasses;

    private readonly float[] velocity = new float[2];

    private void OnEnable()
    {
        if (manager == null) manager = World.Active.GetOrCreateManager<EntityManager>();
        var gasArchetype = manager.CreateArchetype(typeof(GridPosition), typeof(Gas));

        var wid = 1;
        var hei = 2;

        gasses = new NativeArray<Entity>(wid*hei*gasDefinitions.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            
        manager.CreateEntity(gasArchetype, gasses);

        var maxMoles = Moles(2, NormalPres, NormalTemp);
        var nitrogen = maxMoles * 0.78f;
        var oxygen = maxMoles * 0.22f;
        
        for (var x = 0; x < wid; x++)
        for (var y = 0; y < hei; y++)
        for (byte i = 0; i < gasDefinitions.Length; i++)
        {
            var gas = gasses[i + x * gasDefinitions.Length + y * gasDefinitions.Length * wid];
            manager.SetComponentData(gas, new GridPosition (new int3(x, 0, y)));

            if (y == 0)
            {
                if(i == 0) manager.SetComponentData(gas, new Gas(i, nitrogen*2, NormalTemp));
                if(i == 1) manager.SetComponentData(gas, new Gas(i, 0, NormalTemp));
            }
            if (y == 1)
            {
                if(i == 0) manager.SetComponentData(gas, new Gas(i, 0, NormalTemp));
                if(i == 1) manager.SetComponentData(gas, new Gas(i, oxygen*2, NormalTemp/2f));
            }
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

        //StartCoroutine(Step());
    }

    private IEnumerator Step()
    {
        yield return new WaitForSeconds(5);
        while (true)
        {
            yield return new WaitForFixedUpdate();
            
            ProcessPressure(0);
            ProcessPressure(1);

            HeatTransfer(0);
            HeatTransfer(1);
        
            EqualizeTemperature(ref tiles[0]);
            EqualizeTemperature(ref tiles[1]);
        }
    }

    private void HeatTransfer(int gasIndex)
    {
        if (tiles[0].moles[gasIndex] == 0) return;
        if (tiles[1].moles[gasIndex] == 0) return;
        if (velocity[gasIndex] == 0) return;
        
        var ta = tiles[0].temperature[gasIndex];
        var tb = tiles[1].temperature[gasIndex];

        const float d = 8f / 6f;
        var h = (gasDefinitions[gasIndex].heatConductivity 
                 * 0.023f 
                 * (((velocity[gasIndex]+BaseFlux)*d)/gasDefinitions[gasIndex].viscosity) 
                 * math.pow((gasDefinitions[gasIndex].viscosity*gasDefinitions[gasIndex].heatCapacity)/gasDefinitions[gasIndex].heatConductivity,0.4f)) 
                    / d;
        
        var amount = h * 2 * (ta-tb) * Time.fixedDeltaTime;

        Debug.Log("Amount for " + gasIndex + ": " + amount);
        
        var fromEnergy = Energy(gasIndex, tiles[0].moles[gasIndex], tiles[0].temperature[gasIndex]);
        var toEnergy = Energy(gasIndex, tiles[1].moles[gasIndex], tiles[1].temperature[gasIndex]);
        
        tiles[0].temperature[gasIndex] = Temperature(gasIndex, tiles[0].moles[gasIndex], fromEnergy - amount);
        tiles[1].temperature[gasIndex] = Temperature(gasIndex, tiles[1].moles[gasIndex], toEnergy + amount);
    }

    
    private void ProcessPressure(int gas)
    {
        var mass0 = tiles[0].moles[gas] * gasDefinitions[gas].molarMass;
        var mass1 = tiles[1].moles[gas] * gasDefinitions[gas].molarMass;
        
        var pressure0 = GasConstant * tiles[0].temperature[gas] * tiles[0].moles[gas] / 2;
        var pressure1 = GasConstant * tiles[1].temperature[gas] * tiles[1].moles[gas] / 2;
        
        if (tiles[0].moles[gas] <= 0 && velocity[gas] > 0) velocity[gas] = 0;
        if (tiles[1].moles[gas] <= 0 && velocity[gas] < 0) velocity[gas] = 0;
        
        var force1 = (pressure0 - pressure1) / 10;
        if (force1 > 0)
        {
            var acceleration = mass0 > 0 ? force1 / mass0 : 0;
    
            velocity[gas] += 0.5f * acceleration * Time.fixedDeltaTime;
            
            var amount = 2 * (velocity[gas] * Time.fixedDeltaTime + 0.5f * acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime);
            Move(amount, gas, ref tiles[0], ref tiles[1]);
        }
        else
        {
            force1 = -force1;
            
            var acceleration = mass1 > 0 ? force1 / mass1 : 0;
    
            velocity[gas] -= acceleration * Time.fixedDeltaTime;
        
            var amount = 2 * (-velocity[gas] * Time.fixedDeltaTime - 0.5f * acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime );
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

    private void EqualizeTemperature(ref AtmosTile tile)
    {
        var totalEnergy = 0f;
        var totalCapacity = 0f;
        for (var i = 0; i < gasDefinitions.Length; i++)
        {
            totalCapacity += gasDefinitions[i].heatCapacity * tile.moles[i];
            totalEnergy += gasDefinitions[i].heatCapacity * tile.moles[i] * tile.temperature[i];
        }
        for (var i = 0; i < gasDefinitions.Length; i++)
            tile.temperature[i] = totalEnergy / totalCapacity;
    }

    [Pure]
    public static float Moles(float volume, float pressure, float temperature)
        => temperature > 0 ? (pressure * volume) / (GasConstant * temperature) : 0;

    [Pure]
    public float Energy(int gasIndex, float moles, float temperature)
        => gasDefinitions[gasIndex].heatCapacity * temperature * moles;

    [Pure]
    public float Temperature(int gasIndex, float moles, float energy)
        => moles > 0 ? energy / (gasDefinitions[gasIndex].heatCapacity * moles) : 0;
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = new Color(0, 0, 0, 0.3f);
        foreach (var entity in gasses)
        {
            var pos = manager.GetComponentData<GridPosition>(entity);
            Gizmos.DrawWireCube((float3)pos.value, new Vector3(1, 0, 1));
        }
    }
}