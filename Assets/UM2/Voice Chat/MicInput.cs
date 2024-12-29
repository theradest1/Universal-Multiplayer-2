using UnityEngine;

public class MicInput : MonoBehaviour
{
    //settings
    public int sampleRate = 44100;
    public float secondsPerPacket = 2;
    public int micBufferSeconds; //needs to be comfortably bigger than the size of sent clips 

    //references
    AudioSource audioSource;
    AudioClip micInputLoop;

    //other
    int pastMicPos = 0;
    float[] audioData;

    void Start()
    {
        // Check if a microphone is available
        if (Microphone.devices.Length > 0)
        {
            //create audio source for playback
            audioSource = gameObject.AddComponent<AudioSource>();

            // Start recording from the microphone
            // null is for the default microphone
            micInputLoop = Microphone.Start(null, true, micBufferSeconds, sampleRate);

            //wait until microphone is recording
            while (!(Microphone.GetPosition(null) > 0)) { }

            //send the clip after it is recorded, and every record time after that
            InvokeRepeating("SendClip", secondsPerPacket, secondsPerPacket);
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }
    }

    void SendClip(){
        //get where the mic is at
        int currentMicPos = Microphone.GetPosition(null);

        //get the samples
        audioData = GetWrappedSamples(micInputLoop, pastMicPos, currentMicPos);

        //record where it stopped
        pastMicPos = currentMicPos;

        //"send" the clip
        RecieveClip(audioData);
    }

    void RecieveClip(float[] clipData){
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
}
