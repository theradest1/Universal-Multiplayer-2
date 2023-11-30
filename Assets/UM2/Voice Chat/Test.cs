using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip sourceClip;
    public float maxDelay = .05f;
    int peiceLength;
    int currentIndex = 0;
    public int sampleRate = 8000;

    private void Start(){
        //make the original less data
        sourceClip = SetSampleRateSimple(sourceClip, sampleRate);

        //math
        peiceLength = (int)(sampleRate * maxDelay);
        print(sampleRate);
        print(maxDelay);
        print(peiceLength);

        //start loop
        StartCoroutine(playbackAudio(null));
    }

    private IEnumerator playbackAudio(AudioClip pastChunk){
        //create new clip
        AudioClip peice = AudioClip.Create("PlaybackClip", peiceLength, 1, sampleRate, false);

        //copy over sample peice to new clip
        float[] samples = new float[peiceLength];
        sourceClip.GetData(samples, currentIndex);
        peice.SetData(samples, 0);
        
        //play created peice
        audioSource.clip = peice;
        audioSource.Play();

        //destroy the past peice
        if(pastChunk != null){
            AudioClip.Destroy(pastChunk);
        }

        //step samples
        currentIndex += peiceLength;
        if(currentIndex >= sourceClip.samples){
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
            var index = (int) ((float) i * samples.Length / samplesLength);

            samplesNew[i] = samples[index];
        }

        clipNew.SetData(samplesNew, 0);

        return clipNew;
    }
}
