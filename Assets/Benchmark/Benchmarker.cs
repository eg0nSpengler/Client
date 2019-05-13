using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class Benchmarker : MonoBehaviour
{
    [SerializeField] private Text text = null;
    [SerializeField] private bool done = false;

    private IEnumerator Start()
    {
        var current = new StringBuilder();
        var totalFrameTime = 0f;
        var totalFrames = 0;
        var minFrameTime = float.PositiveInfinity;
        var maxFrameTime = float.NegativeInfinity;
        
        
        yield return new WaitForSeconds(1);

        float averageFrameTime;
        while (!done)
        {
            yield return null;

            var frameTime = Time.deltaTime;
            totalFrameTime += frameTime;
            totalFrames++;

            if (frameTime < minFrameTime) minFrameTime = frameTime;
            if (frameTime > maxFrameTime) maxFrameTime = frameTime;

            averageFrameTime = totalFrameTime / totalFrames;

            current.Clear();
            current.AppendLine("Benchmarking...");
            current.AppendLine("The following data will be sent at the end of the test:");
            current.AppendLine();

            current.AppendLine(SystemInfo.operatingSystem);
            current.AppendLine(SystemInfo.processorType);
            current.Append("Processor frequency: ");
            current.AppendLine(SystemInfo.processorFrequency.ToString());
            current.Append("Processor count: ");
            current.AppendLine(SystemInfo.processorCount.ToString());
            current.Append("System memory: ");
            current.AppendLine(SystemInfo.systemMemorySize.ToString());
            current.Append("Graphics device type: ");
            current.AppendLine(SystemInfo.graphicsDeviceType.ToString());
            current.AppendLine(SystemInfo.graphicsDeviceName);
            current.Append("Graphics memory: ");
            current.AppendLine(SystemInfo.graphicsMemorySize.ToString());
            current.Append("Battery: ");
            current.AppendLine(SystemInfo.batteryStatus.ToString());
            current.AppendLine();
            
            current.Append("Current frames per second: ");
            current.AppendLine((1/Time.deltaTime).ToString(CultureInfo.InvariantCulture));
            current.Append("Average frames per second: ");
            current.AppendLine((1/averageFrameTime).ToString(CultureInfo.InvariantCulture));
            current.Append("Minimum frames per second: ");
            current.AppendLine((1/maxFrameTime).ToString(CultureInfo.InvariantCulture));
            current.Append("Maximum frames per second: ");
            current.AppendLine((1/minFrameTime).ToString(CultureInfo.InvariantCulture));

            text.text = current.ToString();
        }

        current.Clear();
        current.AppendLine("Sending benchmarking results...");
        current.AppendLine();

        current.AppendLine(SystemInfo.operatingSystem);
        current.AppendLine(SystemInfo.processorType);
        current.Append("Processor frequency: ");
        current.AppendLine(SystemInfo.processorFrequency.ToString());
        current.Append("Processor count: ");
        current.AppendLine(SystemInfo.processorCount.ToString());
        current.Append("System memory: ");
        current.AppendLine(SystemInfo.systemMemorySize.ToString());
        current.Append("Graphics device type: ");
        current.AppendLine(SystemInfo.graphicsDeviceType.ToString());
        current.AppendLine(SystemInfo.graphicsDeviceName);
        current.Append("Graphics memory: ");
        current.AppendLine(SystemInfo.graphicsMemorySize.ToString());
        current.Append("Battery: ");
        current.AppendLine(SystemInfo.batteryStatus.ToString());
        current.AppendLine();
        
        averageFrameTime = totalFrameTime / totalFrames;
        
        current.Append("Average frame time: ");
        current.AppendLine(averageFrameTime.ToString(CultureInfo.InvariantCulture));
        current.Append("Minimum frames time: ");
        current.AppendLine(minFrameTime.ToString(CultureInfo.InvariantCulture));
        current.Append("Maximum frames time: ");
        current.AppendLine(maxFrameTime.ToString(CultureInfo.InvariantCulture));

        Analytics.CustomEvent("Benchmark", new Dictionary<string, object>
        {
            {"Operating System", SystemInfo.operatingSystem},
            {"Processor Type", SystemInfo.processorType},
            {"Processor Frequency", SystemInfo.processorFrequency},
            {"Processor Count", SystemInfo.processorCount},
            {"System Memory", SystemInfo.systemMemorySize},
            {"Graphics Device Type", SystemInfo.graphicsDeviceType},
            {"Graphics Device Name", SystemInfo.graphicsDeviceName},
            {"Graphics Device Memory", SystemInfo.graphicsMemorySize},
            {"Battery Status", SystemInfo.batteryStatus},
            {"Average Frame Time", averageFrameTime},
            {"Minimum Frame Time", minFrameTime},
            {"Maximum Frame Time", maxFrameTime},
        });
        
        
        text.text = current.ToString();
        
        yield return new WaitForSeconds(1);
        
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void Done()
    {
        done = true;
    }
}
