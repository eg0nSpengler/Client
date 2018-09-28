using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Atmos
{
	public enum TileStates
	{
		// Semiactive doesn't equalize pressure but it mixes gases
		Active, Semiactive, Inactive, Vacuum, Blocked
	}
	
	public enum Gasses
	{
		Oxygen = 0,
		Nitrogen = 1,
		CarbonDioxide = 2,
		Plasma = 3
	}
	
	public enum FluxDir
	{
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3
	}
	
	public class Atmosphere
	{
		private int width;
		private int length;
		private Tile[,] grid;

		public static int numOfGases = System.Enum.GetNames(typeof(Gasses)).Length;

		public Atmosphere(int width, int length)
		{
			width = Mathf.Clamp(width, 1, 255);
			length = Mathf.Clamp(length, 1, 255);

			this.width = width;
			this.length = length;
			
			grid = new Tile[width, length];

			for (int i = 0; i < width; ++i)
			{
				for (int j = 0; j < length; ++j)
				{
					grid[i, j] = new Tile(this, i, j);
				}
			}
		}
		
		public int Step()
		{
			int activeTiles = 0;
			
			for (int i = 0; i < width; ++i)
			{
				for (int j = 0; j < length; ++j)
				{
					grid[i, j].CalculateFlux();
				}
			}
			
			for (int i = 0; i < width; ++i)
			{
				for (int j = 0; j < length; ++j)
				{
					grid[i, j].SimulateFlux();
					if (grid[i, j].GetState() == TileStates.Active) { ++activeTiles; }
				}
			}

			return activeTiles;
		}

		public Tile GetTile(int x, int y)
		{
			if (x < 0 || y < 0 || x >= width || y >= length)
			{
				return null; // Out of bounds
			}

			return grid[x, y];
		}
	}

	public class Tile
	{
		private float[] gasses = new float[Atmosphere.numOfGases];
		private float temperature = 293f;
		private TileStates state = TileStates.Active;
		
		private int x;
		private int y;
		private Atmosphere owner;

		/*
		 * PV = nRT
		 * 
		 * P - Measured in pascals, 101.3 kPa
		 * V - Measured in cubic meters, 1 m^3
		 * n - Moles
		 * R - Gas constant, 8.314
		 * T - Measured in kelvin, 293 K
		 * 
		 * Human air consumption is 0.016 moles of oxygen per minute
		 * 
		 * Oxygen	Needed for breathing, less than 16kPa causes suffocation, oxidizer
		 * Nitrogen	Heat sink
		 * Carbon Dioxide	Waste gas, causes suffocation at 8kPa
		 * Plasma	FLAMEY GASES, ignites at high pressures in the presence of oxygen
		 */
		
		private Vector4 flux = Vector4.zero;
		private Vector2 velocity = Vector2.zero;
		private bool top = false;
		private bool bottom = false;
		private bool left = false;
		private bool right = false;
		private bool tempSetting = false;
		
		private const float dt = 0.1f;	// Delta time
		private const float gasConstant = 8.314f;	// Universal gas constant
		private const float volume = 2.5f;	// Volume of each tile in cubic meters
		private const float drag = 0.95f;	// Fluid drag, slows down flux so that gases don't infinitely slosh
		private const float thermalRate = 0.024f * volume;	// Rate of temperature equalization
		private const float mixRate = 0.02f;	// Rate of gas mixing
		private const float fluxEpsilon = 0.025f;	// Minimum pressure difference to simulate
		private const float thermalEpsilon = 0.01f;	// Minimum temperature difference to simulate

		public Tile(Atmosphere owner, int x, int y)
		{
			this.owner = owner;
			this.x = x;
			this.y = y;
			
			for (int i = 0; i < Atmosphere.numOfGases; ++i)
			{
				gasses[i] = 0;
			}
		}

		#region Tile Properties
		public float GetGas(Gasses index)
		{
			return gasses[(int)index];
		}

		public float[] GetGasses()
		{
			return gasses;
		}
		
		public TileStates GetState()
		{
			return state;
		}

		public void Block()
		{
			state = TileStates.Blocked;
		}
		
		public float GetTemperature()
		{
			return temperature;
		}
		
		public Vector4 GetFlux()
		{
			return flux;
		}

		public float GetFluxComponent(FluxDir dir)
		{
			return flux[(int)dir];
		}
		
		public void RemoveFlux()
		{
			flux = Vector4.zero;
		}
		#endregion


		#region Gas Manipulation
		public void AddGas(Gasses index, float amount)
		{
			gasses[(int)index] = Mathf.Max(gasses[(int)index] + amount, 0);

			state = TileStates.Active;
		}

		public void AddGasses(float[] amounts)
		{
			for (int i = 0; i < Mathf.Min(amounts.GetLength(0), Atmosphere.numOfGases); ++i)
			{
				gasses[i] = Mathf.Max(gasses[i] + amounts[i], 0);
			}

			state = TileStates.Active;
		}

		public void SetGasses(float[] amounts)
		{
			for (int i = 0; i < Mathf.Min(amounts.GetLength(0), Atmosphere.numOfGases); ++i)
			{
				gasses[i] = Mathf.Max(amounts[i], 0);
			}

			state = TileStates.Active;
		}

		public void MakeAir()
		{
			gasses[(int)Gasses.Oxygen] = 20.79f;	// Oxygen
			gasses[(int)Gasses.Nitrogen] = 83.17f;	// Nitrogen
			gasses[(int)Gasses.CarbonDioxide] = 0f;	// Carbon Dioxide
			gasses[(int)Gasses.Plasma] = 0f;	// Plasma
			temperature = 293f;
		}
		#endregion


		#region Temperature Manipulation
		public void Heat(float temp)
		{
			temperature += Mathf.Max(temp - temperature, 0f) / GetSpecificHeat() * (100 / GetMoles()) * dt;
			state = TileStates.Active;
		}

		public void Heat(float temp, float rate)
		{
			temperature += Mathf.Max(temp - temperature, 0f) / GetSpecificHeat() * (100 / GetMoles()) * rate * dt;
			state = TileStates.Active;
		}

		public void Cool(float temp)
		{
			temperature -= Mathf.Max(temperature - temp, 0f) / GetSpecificHeat() * (100 / GetMoles()) * dt;
			if (temperature < 0f)
			{
				temperature = 0f;
			}
			state = TileStates.Active;
		}

		public void Cool(float temp, float rate)
		{
			temperature -= Mathf.Max(temperature - temp, 0f) / GetSpecificHeat() * (100 / GetMoles()) * rate * dt;
			if (temperature < 0f)
			{
				temperature = 0f;
			}
			state = TileStates.Active;
		}
		#endregion


		#region Gas Properties
		public float GetMoles()
		{
			float moles = 0f;
			for (int i = 0; i < Atmosphere.numOfGases; ++i)
			{
				moles += gasses[i];
			}
			return moles;
		}

		public float GetPressure()
		{
			float pressure = 0f;
			for (int i = 0; i < Atmosphere.numOfGases; ++i)
			{
				pressure += (gasses[i] * gasConstant * temperature) / volume;
			}
			return pressure / 1000f; // KiloPascals
		}

		public float GetPartialPressure(int index)
		{
			return (gasses[index] * gasConstant * temperature) / volume / 1000f; // KiloPascals
		}

		public float GetPartialPressure(Gasses index)
		{
			return (gasses[(int)index] * gasConstant * temperature) / volume / 1000f; // KiloPascals
		}

		public bool IsBreathable()
		{
			return (GetPartialPressure(Gasses.Oxygen) >= 16f && GetPartialPressure(Gasses.CarbonDioxide) < 8f);
		}

		public float GetSpecificHeat()
		{
			float temp = 0f;
			temp += gasses[(int)Gasses.Oxygen] * 2f;	// Oxygen, 20
			temp += gasses[(int)Gasses.Nitrogen] * 20f;	// Nitrogen, 200
			temp += gasses[(int)Gasses.CarbonDioxide] * 3f;	// Carbon Dioxide, 30
			temp += gasses[(int)Gasses.Plasma] * 1f;	// Plasma, 10
			return temp / GetMoles();
		}

		public float GetMass()
		{
			float mass = 0f;
			mass += gasses[(int)Gasses.Oxygen] * 32f;	// Oxygen
			mass += gasses[(int)Gasses.Nitrogen] * 28f;	// Nitrogen
			mass += gasses[(int)Gasses.CarbonDioxide] * 44f;	// Carbon Dioxide
			mass += gasses[(int)Gasses.Plasma] * 78f;	// Plasma
			return mass;	// Grams
		}
		#endregion


		#region Gas Simulation
		public void CalculateFlux()
		{
			if (state == TileStates.Active)
			{
				float fluxTop = 0f;
				float fluxBottom = 0f;
				float fluxLeft = 0f;
				float fluxRight = 0f;

				top = false;
				bottom = false;
				left = false;
				right = false;

				//if (y < grid.GetLength(1) - 1 && grid[x, y + 1].state != TileStates.Blocked)
				Tile topTile = owner.GetTile(x, y + 1);
				if (topTile != null && topTile.state != TileStates.Blocked)
				{
					fluxTop = Mathf.Min(flux.x * drag + (GetPressure() - topTile.GetPressure()) * dt, 1000f);
					top = true;

					if (fluxTop < 0f)
					{
						topTile.state = TileStates.Active;
						fluxTop = 0f;
					}
				}
				Tile bottomTile = owner.GetTile(x, y - 1);
				if (bottomTile != null && bottomTile.state != TileStates.Blocked)
				{
					fluxBottom = Mathf.Min(flux.y * drag + (GetPressure() - bottomTile.GetPressure()) * dt, 1000f);
					bottom = true;

					if (fluxBottom < 0f)
					{
						bottomTile.state = TileStates.Active;
						fluxBottom = 0f;
					}
				}
				Tile leftTile = owner.GetTile(x - 1, y);
				if (leftTile != null && leftTile.state != TileStates.Blocked)
				{
					fluxLeft = Mathf.Min(flux.z * drag + (GetPressure() - leftTile.GetPressure()) * dt, 1000f);
					left = true;

					if (fluxLeft < 0f)
					{
						leftTile.state = TileStates.Active;
						fluxLeft = 0f;
					}
				}
				Tile rightTile = owner.GetTile(x + 1, y);
				if (rightTile != null && rightTile.state != TileStates.Blocked)
				{
					fluxRight = Mathf.Min(flux.w * drag + (GetPressure() - rightTile.GetPressure()) * dt, 1000f);
					right = true;

					if (fluxRight < 0f)
					{
						rightTile.state = TileStates.Active;
						fluxRight = 0f;
					}
				}

				if (fluxTop > fluxEpsilon || fluxBottom > fluxEpsilon || fluxLeft > fluxEpsilon || fluxRight > fluxEpsilon)
				{
					if (fluxTop + fluxBottom + fluxLeft + fluxRight > GetPressure())
					{
						float scalingFactor = Mathf.Min(1, GetPressure() / (fluxTop + fluxBottom + fluxLeft + fluxRight) / dt);

						fluxTop *= scalingFactor;
						fluxBottom *= scalingFactor;
						fluxLeft *= scalingFactor;
						fluxRight *= scalingFactor;
					}

					flux = new Vector4(fluxTop, fluxBottom, fluxLeft, fluxRight);
				}
				else
				{
					flux = Vector4.zero;
					if (!tempSetting) { state = TileStates.Semiactive; } else { tempSetting = false; }
				}
			}
			if (state == TileStates.Active || state == TileStates.Semiactive)
			{
				SimulateMixing();
			}
		}

		public void SimulateFlux() //Tile[,] grid, int x, int y)
		{
			if (state == TileStates.Active)
			{
				float pressure = GetPressure();

				Tile topTile = owner.GetTile(x, y + 1);
				Tile bottomTile = owner.GetTile(x, y - 1);
				Tile leftTile = owner.GetTile(x + 1, y);
				Tile rightTile = owner.GetTile(x - 1, y);

				for (int i = 0; i < Atmosphere.numOfGases; ++i)
				{
					if (gasses[i] < 1f)
					{
						gasses[i] = 0f;
					}

					if (gasses[i] > 0f)
					{
						if (flux.x > 0f)
						{
							float factor = gasses[i] * (flux.x / pressure);
							if (topTile.state != TileStates.Vacuum)
							{
								topTile.gasses[i] += factor;
								topTile.state = TileStates.Active;
							}
							else
							{
								top = false;
							}
							gasses[i] -= factor;
						}
						if (flux.y > 0f)
						{
							float factor = gasses[i] * (flux.y / pressure);
							if (bottomTile.state != TileStates.Vacuum)
							{
								bottomTile.gasses[i] += factor;
								bottomTile.state = TileStates.Active;
							}
							else
							{
								bottom = false;
							}
							gasses[i] -= factor;
						}
						if (flux.w > 0f)
						{
							float factor = gasses[i] * (flux.w / pressure);
							if (leftTile.state != TileStates.Vacuum)
							{
								leftTile.gasses[i] += factor;
								leftTile.state = TileStates.Active;
							}
							else
							{
								left = false;
							}
							gasses[i] -= factor;
						}
						if (flux.z > 0f)
						{
							float factor = gasses[i] * (flux.z / pressure);
							if (rightTile.state != TileStates.Vacuum)
							{
								rightTile.gasses[i] += factor;
								rightTile.state = TileStates.Active;
							}
							else
							{
								right = false;
							}
							gasses[i] -= factor;
						}
					}
				}

				float difference;

				if (top)
				{
					difference = (temperature - topTile.temperature) * thermalRate; // / (GetSpecificHeat() * 5f);

					if (difference > thermalEpsilon)
					{
						topTile.temperature += difference;
						temperature -= difference;
						tempSetting = true;
					}
				}
				if (bottom)
				{
					difference = (temperature - bottomTile.temperature) * thermalRate;

					if (difference > thermalEpsilon)
					{
						bottomTile.temperature += difference;
						temperature -= difference;
						tempSetting = true;
					}
				}
				if (left && leftTile != null)
				{
					difference = (temperature - leftTile.temperature) * thermalRate;

					if (difference > thermalEpsilon)
					{
						leftTile.temperature += difference;
						temperature -= difference;
						tempSetting = true;
					}
				}
				if (right && rightTile != null)
				{
					difference = (temperature - rightTile.temperature) * thermalRate;

					if (difference > thermalEpsilon)
					{
						rightTile.temperature += difference;
						temperature -= difference;
						tempSetting = true;
					}
				}

				//float velHorizontal = fluxFromLeft + flux.w - flux.z - fluxFromRight;
				//float velVertical = fluxFromBottom + flux.x - flux.y - fluxFromTop;

				//velocity = new Vector2(velHorizontal, velVertical);
			}
			/*else if (state == States.semiactive)
			{
				SimulateMixing(grid, x, y);
			}*/
		}

		public void SimulateMixing() //Tile[,] grid, int x, int y)
		{
			return;
			
			bool mixed = false;
			float[] difference = new float[Atmosphere.numOfGases];

			/*for (int i = 0; i < _numOfGases; ++i)
			{
				if (gases[i] > 0f)
				{
					if (y < grid.GetLength(1) - 1 && grid[x, y + 1].state != States.blocked)
					{
						difference = (gases[i] - grid[x, y + 1].gases[i]) * 0.2f;

						if (difference > 0.1f)
						{
							grid[x, y + 1].gases[i] += difference;
							grid[x, y + 1].state = States.semiactive;
							gases[i] -= difference;
							mixed = true;
						}
					}
					/*if (y > 0 && grid[x, y - 1].state != States.blocked)
					{
						difference = (gases[i] - grid[x, y - 1].gases[i]) * 0.2f;

						if (difference > 0.1f)
						{
							grid[x, y - 1].gases[i] += difference;
							grid[x, y - 1].state = States.semiactive;
							gases[i] -= difference;
							mixed = true;
						}
					}
					if (x > 0 && grid[x - 1, y].state != States.blocked)
					{
						difference = (gases[i] - grid[x - 1, y].gases[i]) * 0.2f;

						if (difference > 0.1f)
						{
							grid[x - 1, y].gases[i] += difference;
							grid[x - 1, y].state = States.semiactive;
							gases[i] -= difference;
							mixed = true;
						}
					}
					if (x < grid.GetLength(0) - 1 && grid[x + 1, y].state != States.blocked)
					{
						difference = (gases[i] - grid[x + 1, y].gases[i]) * 0.2f;

						if (difference > 0.1f)
						{
							grid[x + 1, y].gases[i] += difference;
							grid[x, y + 1].state = States.semiactive;
							gases[i] -= difference;
							mixed = true;
						}
					}*//*
				}
			}*/

			/*if (y < grid.GetLength(1) - 1 && grid[x, y + 1].state != TileStates.Blocked)
			{
				difference = ArrayDiff(gasses, grid[x, y + 1].gasses);
				float pressure = GetMoles();
				float pressureOther = grid[x, y + 1].GetMoles();

				if (!ArrayZero(difference))
				{
					grid[x, y + 1].gasses = ArrayNorm(ArraySum(grid[x, y + 1].gasses, difference), pressureOther);
					if (grid[x, y + 1].state == TileStates.Inactive) { grid[x, y + 1].state = TileStates.Semiactive; }
					gasses = ArrayNorm(ArrayDiff(gasses, difference), pressure);
					mixed = true;
				}
			}
			if (y > 0 && grid[x, y - 1].state != TileStates.Blocked)
			{
				difference = ArrayDiff(gasses, grid[x, y - 1].gasses);
				float pressure = GetMoles();
				float pressureOther = grid[x, y - 1].GetMoles();

				if (!ArrayZero(difference))
				{
					grid[x, y - 1].gasses = ArrayNorm(ArraySum(grid[x, y - 1].gasses, difference), pressureOther);
					if (grid[x, y - 1].state == TileStates.Inactive) { grid[x, y - 1].state = TileStates.Semiactive; }
					gasses = ArrayNorm(ArrayDiff(gasses, difference), pressure);
					mixed = true;
				}
			}
			if (x > 0 && grid[x - 1, y].state != TileStates.Blocked)
			{
				difference = ArrayDiff(gasses, grid[x - 1, y].gasses);
				float pressure = GetMoles();
				float pressureOther = grid[x - 1, y].GetMoles();

				if (!ArrayZero(difference))
				{
					grid[x - 1, y].gasses = ArrayNorm(ArraySum(grid[x - 1, y].gasses, difference), pressureOther);
					if (grid[x - 1, y].state == TileStates.Inactive) { grid[x - 1, y].state = TileStates.Semiactive; }
					gasses = ArrayNorm(ArrayDiff(gasses, difference), pressure);
					mixed = true;
				}
			}
			if (x < grid.GetLength(0) - 1 && grid[x + 1, y].state != TileStates.Blocked)
			{
				difference = ArrayDiff(gasses, grid[x + 1, y].gasses);
				float pressure = GetMoles();
				float pressureOther = grid[x + 1, y].GetMoles();

				if (!ArrayZero(difference))
				{
					grid[x + 1, y].gasses = ArrayNorm(ArraySum(grid[x + 1, y].gasses, difference), pressureOther);
					if (grid[x + 1, y].state == TileStates.Inactive) { grid[x + 1, y].state = TileStates.Semiactive; }
					gasses = ArrayNorm(ArrayDiff(gasses, difference), pressure);
					mixed = true;
				}
			}

			if (!mixed && state == TileStates.Semiactive)
			{
				state = TileStates.Inactive;
			}*/
		}
		#endregion


		#region Array Functions
		private float[] ArrayDiff(float[] arr1, float[] arr2)
		{
			float[] difference = new float[Atmosphere.numOfGases];

			for (int i = 0; i < Atmosphere.numOfGases; ++i)
			{
				difference[i] = (arr1[i] - arr2[i]) * mixRate;
			}

			return difference;
		}

		private float[] ArraySum(float[] arr1, float[] arr2)
		{
			float[] sum = new float[Atmosphere.numOfGases];

			for (int i = 0; i < Atmosphere.numOfGases; ++i)
			{
				sum[i] = arr1[i] + arr2[i];
			}

			return sum;
		}

		private float[] ArrayNorm(float[] arr, float normal)
		{
			float div = ArraySize(arr);

			for (int i = 0; i < Atmosphere.numOfGases; ++i)
			{
				arr[i] = arr[i] / div * normal;
			}

			return arr;
		}

		private float ArraySize(float[] arr)
		{
			float size = 0f;

			for (int i = 0; i < Atmosphere.numOfGases; ++i)
			{
				size += arr[i];
			}

			return size;
		}

		private bool ArrayZero(float[] arr)
		{
			for (int i = 0; i < Atmosphere.numOfGases; ++i)
			{
				if (arr[i] / mixRate > 0.1f) { return false; }
			}

			return true;
		}
		#endregion
	}
}