using UnityEngine;

public class BackgroundColorChanger : MonoBehaviour
{
    public AudioSource audioSource;  
    public Color baseColor = Color.black;
    public Color beatColor;
    public float sensitivity=2f; 
    public float lerpSpeed=2f;
    public float startTime=11f; 
    public float bassThreshold=10f;
    public float beatDecayRate=8f; 

    private Camera cam;
    private float[] spectrumData=new float[64];  
    private float targetBeatIntensity=0f; 
    private float currentBeatIntensity=0f; 

    void Start()
    {
        cam=Camera.main;
    }

    void Update()
    {
        if (audioSource.time<startTime)
        {
            cam.backgroundColor=baseColor; 
            return;
        }
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        float bassValue=0f;
        int bassRange=10; 

        for (int i=0; i<bassRange; i++)
        {
            bassValue+=spectrumData[i];
        }

        bassValue*=sensitivity;
        if (bassValue>bassThreshold)
        {
            targetBeatIntensity=Mathf.Min(1f, bassValue);
        }
        targetBeatIntensity=Mathf.Max(0f, targetBeatIntensity - (Time.deltaTime * beatDecayRate));
        currentBeatIntensity=Mathf.Lerp(currentBeatIntensity, targetBeatIntensity, Time.deltaTime * lerpSpeed);
        cam.backgroundColor=Color.Lerp(baseColor, beatColor, currentBeatIntensity);
    }
}