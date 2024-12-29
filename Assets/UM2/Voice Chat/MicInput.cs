using UnityEngine;
using System;
using TMPro;

public class MicInput : MonoBehaviour
{
    //settings
    public float secondsPerPacket = 2;
    public int micBufferSeconds; //needs to be comfortably bigger than the size of sent clips

    [Range(4000, 50000)]
    public int targetSampleRate;

    //references
    AudioSource audioSource;
    AudioClip micInputLoop;

    //other
    int pastMicPos = 0;
    int sampleRate;
    float[] audioData;

    //debug
    public TextMeshProUGUI bytesPerSecondText;
    public TextMeshProUGUI samplesPerSecondText;
    int bytesPerSec = 0;
    int samplesPerSec = 0;
    float avgBytesPerSec = 0;
    float avgSamplesPerSec = 0;

    void Start()
    {
        // Check if a microphone is available
        if (Microphone.devices.Length > 0)
        {
            //create audio source for playback
            audioSource = gameObject.AddComponent<AudioSource>();

            //get the max frequency (and use it)
            Microphone.GetDeviceCaps(null, out int minFreq, out int maxFreq);
            sampleRate = targetSampleRate; //Math.Clamp(targetSampleRate, minFreq, maxFreq);
            Debug.Log("Sample Rate: " + sampleRate);

            // Start recording from the microphone
            // null is for the default microphone
            micInputLoop = Microphone.Start(null, true, micBufferSeconds, sampleRate);

            //wait until microphone is recording
            while (!(Microphone.GetPosition(null) > 0)) { }

            //send the clip after it is recorded, and every record time after that
            InvokeRepeating("SendClip", secondsPerPacket, secondsPerPacket);
            InvokeRepeating("UpdateText", 1, 1);
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }
    }

    void UpdateText(){
        //average
        avgBytesPerSec = (bytesPerSec + avgBytesPerSec) / 2;
        avgSamplesPerSec = (samplesPerSec + avgSamplesPerSec) / 2;

        //show
        bytesPerSecondText.text = "Bytes Per Sec: " + (int)avgBytesPerSec;
        samplesPerSecondText.text = "Samples Per Sec: " + (int)avgSamplesPerSec;

        //clear
        bytesPerSec = 0;
        samplesPerSec = 0;
    }

    void SendClip(){
        //get where the mic is at
        int currentMicPos = Microphone.GetPosition(null);

        //get the samples
        audioData = GetWrappedSamples(micInputLoop, pastMicPos, currentMicPos);
        samplesPerSec += audioData.Length;

        //record where it stopped
        pastMicPos = currentMicPos;

        byte[] audioBytes = EncodeFloatArray(audioData);
        bytesPerSec += audioBytes.Length;

        //"send" the clip
        RecieveClip(audioBytes);
    }

    void RecieveClip(byte[] clipBytes){
        float[] clipData = DecodeByteArray(clipBytes);

        //create new temp clip the right size
        AudioClip newClip = AudioClip.Create("RecordedClip", clipData.Length, 1, sampleRate, false);
        
        //set the clip samples
        newClip.SetData(clipData, 0);
        
        //set clip
        audioSource.clip = newClip;
        
        //start from beginning of clip
        audioSource.Play();
    }

    private void OnDestroy() {
        //clean up
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }
    }

    //gets the samples of an audioclip with a start and end position. Can wrap
    public static float[] GetWrappedSamples(AudioClip audioClip, int start, int end)
    {
        int totalSamples = audioClip.samples * audioClip.channels;
        int startSample = start * audioClip.channels;
        int endSample = end * audioClip.channels;

        int sampleLength = endSample - startSample;
        if (sampleLength < 0) // Handle wrap-around
        {
            sampleLength += totalSamples;
        }

        float[] clipData = new float[totalSamples];
        float[] resultData = new float[sampleLength];

        // Load all samples from the clip
        audioClip.GetData(clipData, 0);

        for (int i = 0; i < sampleLength; i++)
        {
            int sampleIndex = (startSample + i) % totalSamples;
            resultData[i] = clipData[sampleIndex];
        }

        return resultData;
    }

    public static byte[] EncodeFloatArray(float[] floatArray)
    {
        byte[] byteArray = new byte[floatArray.Length * sizeof(float)];
        Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    public static float[] DecodeByteArray(byte[] byteArray)
    {
        float[] floatArray = new float[byteArray.Length / sizeof(float)];
        Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
        return floatArray;
    }

}
