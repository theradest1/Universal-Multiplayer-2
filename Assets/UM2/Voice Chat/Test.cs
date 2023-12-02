using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Test : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip sourceClip;

    [Range(0.5f, 0.01f)]
    public float peiceSeconds = .05f;
    int peiceLength;
    int currentIndex = 0;
    public int sampleRate = 8000;

    [Header("Debug:")]
    public TextMeshProUGUI bitPerSecText;
    int bits = 0;
    public TextMeshProUGUI simulatedPingText;
    public int simulatedPing = 60;
    public TextMeshProUGUI serverLatencyText;
    public TextMeshProUGUI clientLatencyText;
    public TextMeshProUGUI sampleRateText;


    private void Start()
    {
        //make the original less data
        sourceClip = SetSampleRateSimple(sourceClip, sampleRate);

        //start loop
        StartCoroutine(playbackAudio(null));
        StartCoroutine(debugUpdate());
    }

    private IEnumerator debugUpdate()
    {
        sampleRateText.text = "Sample rate: " + sampleRate + "hz";

        bitPerSecText.text = "Bandwidth: " + bits + " bits/sec";
        bits = 0;

        serverLatencyText.text = "Latency (server): " + (simulatedPing + peiceSeconds * 1000) + "ms";
        clientLatencyText.text = "Latency (client): " + (simulatedPing * 2 + peiceSeconds * 1000) + "ms";
        simulatedPingText.text = "Simulated ping: " + simulatedPing;

        yield return new WaitForSeconds(1);
        StartCoroutine(debugUpdate());
    }

    private IEnumerator playbackAudio(AudioClip pastChunk)
    {
        //lil math
        peiceLength = (int)(sampleRate * peiceSeconds);

        //create new clip
        AudioClip peice = AudioClip.Create("PlaybackClip", peiceLength, 1, sampleRate, false);

        //copy over sample peice to new clip
        float[] samples = new float[peiceLength];
        sourceClip.GetData(samples, currentIndex);
        peice.SetData(samples, 0);
        bits += peiceLength * 8; //a rough calculation for bits/sec

        //play created peice
        audioSource.clip = peice;
        audioSource.Play();

        //destroy the past peice
        if (pastChunk != null)
        {
            AudioClip.Destroy(pastChunk);
        }

        //step samples
        currentIndex += peiceLength;
        if (currentIndex >= sourceClip.samples)
        {
            currentIndex = 0;
        }

        yield return new WaitForSeconds(peice.length);
        StartCoroutine(playbackAudio(peice));
    }

    public static AudioClip SetSampleRateSimple(AudioClip clip, int frequency)
    {
        if (clip.frequency == frequency) return clip;

        var samples = new float[clip.samples * clip.channels];

        clip.GetData(samples, 0);

        var samplesLength = (int)(frequency * clip.length) * clip.channels;
        var samplesNew = new float[samplesLength];
        var clipNew = AudioClip.Create(clip.name + "_" + frequency, samplesLength, clip.channels, frequency, false);

        for (var i = 0; i < samplesLength; i++)
        {
            var index = (int)((float)i * samples.Length / samplesLength);

            samplesNew[i] = samples[index];
        }

        clipNew.SetData(samplesNew, 0);

        return clipNew;
    }
}
