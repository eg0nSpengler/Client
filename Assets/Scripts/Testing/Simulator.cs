using System.Threading;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Atmos;

public class Simulator : MonoBehaviour
{
    public int width = 16;
    public int length = 16;
    private Atmosphere atmos;

	public float rate = 0f;
	private bool threadDone = true;
	private float lastStep;
	private int activeTiles = 0;

	public bool drawDebug = false;
	public bool drawAll = false;
	public float drawRadius = 3.5f;
	public enum ViewType { Content, Pressure, Temperature, Combined };
	public ViewType drawView = ViewType.Content;
	
	public Gasses gas = Gasses.Oxygen;
	
	void Start ()
    {
		lastStep = Time.fixedTime;
		
		atmos = new Atmosphere(width, length);

		for (int i = 0; i < width; ++i)
		{
			for (int j = 0; j < length; ++j)
			{
				atmos.GetTile(i, j).MakeAir();
				atmos.GetTile(i, j).RemoveFlux();
			}
		}
	}

	private void Thread ()
	{
		activeTiles = atmos.Step();

		threadDone = true;
	}
	
	void Update ()
    {
		if (Time.fixedTime >= lastStep && threadDone)
		{
			Debug.Log("Ran for " + (Time.fixedTime - lastStep) + " seconds, simulating " + activeTiles + " atmos tiles");
			
			threadDone = false;
			new Thread(Thread).Start();

			activeTiles = 0;
			lastStep = Time.fixedTime + rate;
		}

		Vector3 hit = GetMouse();

		hit.x = Mathf.Round(hit.x);
		hit.z = Mathf.Round(hit.z);

		if (hit.x >= 0 && hit.x < width && hit.z >= 0 && hit.z < length)
		{
			Tile tile = atmos.GetTile((int)hit.x, (int)hit.z);

			if (Input.GetMouseButton(0))
			{
				tile.AddGas(gas, 60f);
			}
			else if (Input.GetMouseButton(1))
			{
				tile.MakeEmpty();
			}
			else if (Input.GetMouseButton(2))
			{
				tile.Block();
			}
			else if (Input.GetMouseButton(3))
			{
				tile.Heat(2000f);
			}
			else if (Input.GetMouseButton(4))
			{
				tile.Cool(0f);
			}
		}

		//atmos.GetTile(i, j).SetGasses(new float[] { (Mathf.Max(i, j) + 1f) * 15f, 0f, 0f, 0f });
		/*if (Input.GetKey("z"))
        {
            atmos[width - 1, 0].SetGases(new float[] { 0f, 0f, 0f, 0f });
		}
        if (Input.GetKey("x"))
        {
			atmos[Mathf.RoundToInt(width / 2), Mathf.RoundToInt(height / 2)].Heat(2000f);
		}
		if (Input.GetKey("n"))
		{
			atmos[Mathf.RoundToInt(width / 2), Mathf.RoundToInt(height / 2)].AddGases(new float[] { 0f, 0f, 0f, 30f });
		}
        if (Input.GetKeyDown("c"))
        {
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    atmos[i, j].MakeAir();
                    atmos[i, j].flux = Vector4.zero;
				}
            }
        }
        if (Input.GetKeyDown("v"))
        {
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    atmos[i, j].SetGases(new float[] { (Mathf.Max(i, j) + 1f) * 15f, 0f, 0f, 0f });
                    atmos[i, j].flux = Vector4.zero;
                }
            }
        }
        if (Input.GetKeyDown("b"))
        {
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    atmos[i, j].SetGases(new float[] { Random.Range(0f, 40f), Random.Range(0f, 40f), Random.Range(0f, 40f), Random.Range(0f, 40f) });
                    atmos[i, j].flux = Vector4.zero;
				}
            }
        }
		if (Input.GetKeyDown("h"))
		{
			for (int i = 0; i < width; ++i)
			{
				for (int j = 0; j < height; ++j)
				{
					atmos[i, j].SetGases(new float[] { (i < width/2f ? 100f : 0f), (i < width / 2f ? 0f : 100f), 0f, 0f });
					atmos[i, j].flux = Vector4.zero;
				}
			}
		}*/
	}

	private Vector3 GetMouse()
	{
		Plane plane = new Plane(Vector3.up, Vector3.zero);
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float distance;

		if (plane.Raycast(ray, out distance))
		{
			return ray.GetPoint(distance);
		}

		return Vector3.down;
	}

	private void OnDrawGizmos()
	{
		if (drawDebug)
		{
			Vector3 hit = GetMouse();

			if (hit != Vector3.down)
			{
				for (int i = 0; i < width; ++i) //width-1; i > 0; --i)
				{
					for (int j = 0; j < length; ++j) //height-1; j > 0; --j)
					{
						Vector3 draw = new Vector3(i, 0, j) / 1f;

						if (Vector3.Distance(draw, hit) < drawRadius || drawAll)
						{
							Color state;
							switch (atmos.GetTile(i, j).GetState())
							{
								case TileStates.Active: state = new Color(0, 0, 0, 0); break;
								case TileStates.Semiactive: state = new Color(0, 0, 0, 0.8f); break;
								case TileStates.Inactive: state = new Color(0, 0, 0, 0.8f); break;
								default: state = new Color(0, 0, 0, 1); break;
							}

							float pressure;

							if (atmos.GetTile(i, j).GetState() == TileStates.Blocked)
							{
								Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 1f);
								Gizmos.DrawCube(new Vector3(i, 0.5f, j), new Vector3(1, 1, 1));
							}
							else
							{
								switch (drawView)
								{
									case ViewType.Content:
										float[] gases = new float[5];
										Color[] colors = new Color[] { Color.blue, Color.red, Color.gray, Color.magenta };

										float offset = 0f;

										for (int k = 0; k < 4; ++k)
										{
											float moles = atmos.GetTile(i, j).GetGasses()[k] / 30f;

											if (moles != 0f)
											{
												Gizmos.color = colors[k] - state;
												Gizmos.DrawCube(new Vector3(i, moles / 2f + offset, j), new Vector3(1, moles, 1));
												offset += moles;
											}
										}
										break;
									case ViewType.Pressure:
										pressure = atmos.GetTile(i, j).GetPressure() / 30f;

										Gizmos.color = Color.white - state;
										Gizmos.DrawCube(new Vector3(i, pressure / 2f, j), new Vector3(1, pressure, 1));
										break;
									case ViewType.Temperature:
										float temperatue = atmos.GetTile(i, j).GetTemperature() / 100f;

										Gizmos.color = Color.red - state;
										Gizmos.DrawCube(new Vector3(i, temperatue / 2f, j), new Vector3(1, temperatue, 1));
										break;
									case ViewType.Combined:
										pressure = atmos.GetTile(i, j).GetPressure() / 30f;

										Gizmos.color = new Color(atmos.GetTile(i, j).GetTemperature() / 500f, 0, 0, 1) - state;
										Gizmos.DrawCube(new Vector3(i, pressure / 2f, j), new Vector3(1, pressure, 1));
										break;
								}
							}
						}
					}
				}
			}
		}
	}
}
