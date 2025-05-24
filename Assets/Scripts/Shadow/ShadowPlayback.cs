using UnityEngine;
using System.Collections.Generic;

public class ShadowPlayback : MonoBehaviour
{
    private List<PositionData> recordingData;
    private int currentIndex = 0;
    private float playbackStartTime;
    private bool isPlaying = false;

    public void Initialize(List<PositionData> data)
    {
        recordingData = new List<PositionData>(data);
        currentIndex = 0;
        playbackStartTime = Time.time;
        isPlaying = true;

        Debug.Log($"Shadow initialized with {recordingData.Count} positions");

        // Start at first position
        if (recordingData.Count > 0)
        {
            transform.position = recordingData[0].position;
        }
    }

    void Update()
    {
        if (!isPlaying || recordingData == null || recordingData.Count == 0)
            return;

        float currentTime = Time.time - playbackStartTime;

        // Find the right position based on time
        while (currentIndex < recordingData.Count - 1)
        {
            if (currentTime >= recordingData[currentIndex].time)
            {
                currentIndex++;
                transform.position = recordingData[currentIndex].position;
            }
            else
            {
                break;
            }
        }

        // Stop when we've played all positions
        if (currentIndex >= recordingData.Count - 1)
        {
            isPlaying = false;
            Debug.Log("Shadow playback complete");
        }
    }
}