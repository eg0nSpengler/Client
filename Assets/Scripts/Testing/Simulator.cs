using System.Threading;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Atmos;

public class Simulator : MonoBehaviour
{
    public int width = 16;
    public int height = 16;
    private Atmosphere atmos;

	public float rate = 0f;
	private bool threadDone = true;
	private float lastStep;
	private int activeTiles = 0;

	public bool debugDraw = false;
	public float debugRadius = 3.5f;
	public enum ViewType { Content, Pressure, Temperature, Combined };
	public ViewType debugView = ViewType.Content;
	
	void Start ()
    {
		lastStep = Time.fixedTime;
		
		atmos = new Atmosphere(width, height);
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

		if (Input.GetKeyDown("z"))
		{
			for (int i = 0; i < width; ++i)
			{
				for (int j = 0; j < height; ++j)
				{
					//atmos.GetTile(i, j).SetGasses(new float[] { (Mathf.Max(i, j) + 1f) * 15f, 0f, 0f, 0f });
					atmos.GetTile(i, j).MakeAir();
					atmos.GetTile(i, j).RemoveFlux();
				}
			}
		}
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

	private void OnDrawGizmos()
	{
		if (debugDraw)
		{
			Plane plane = new Plane(Vector3.up, Vector3.up * 2);
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			float distance;

			if (plane.Raycast(ray, out distance))
			{
				Vector3 hit = ray.GetPoint(distance);

				for (int i = 0; i < width; ++i) //width-1; i > 0; --i)
				{
					for (int j = 0; j < height; ++j) //height-1; j > 0; --j)
					{
						Vector3 draw = new Vector3(i, 0, j) / 1f;

						if (Vector3.Distance(draw, hit) < debugRadius || true)
						{
							Color state;
							switch (atmos.GetTile(i, j).GetState())
							{
								case TileStates.Active: state = new Color(0, 0, 0, 0); break;
								case TileStates.Semiactive: state = new Color(0, 0, 0, 0.8f); break;
								case TileStates.Inactive: state = new Color(0, 0, 0, 0.8f); break;
								default: state = new Color(0, 0, 0, 1); break;
							}

							switch (debugView)
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
											//Debug.DrawRay(new Vector3(i / 4f, offset, j / 4f), Vector3.up * grid[i, j].gases[k] / 30f, colors[k] - state);
											offset += moles;
										}
									}
									//Debug.DrawRay(new Vector3(i, 0, j), new Vector3(grid[i, j].velocity.x, 0, grid[i, j].velocity.y), Color.red);
									break;
								/*case ViewType.Pressure:
									Color color = Color.white - state;

									Debug.DrawRay(new Vector3(i / 4f, 0, j / 4f), Vector3.up * atmos[i, j].GetPressure() / 30f, color);
									break;
								case ViewType.Temperature:
									Color color2 = Color.red - state;

									Debug.DrawRay(new Vector3(i / 4f, 0, j / 4f), Vector3.up * atmos[i, j].temperature / 100f, color2);
									break;
								case ViewType.Combined:
									float pressure = atmos[i, j].GetPressure() / 30f;

									Gizmos.color = new Color(atmos[i, j].temperature / 500f, 0, 0, 1) - state;
									Gizmos.DrawCube(new Vector3(i, pressure / 2f, j), new Vector3(1, pressure, 1));

									//Debug.DrawRay(new Vector3(i / 4f, 0, j / 4f), Vector3.up * grid[i, j].GetPressure() / 30f, temperature);
									break;*/
							}
						}
					}
				}
			}
		}
	}
}
