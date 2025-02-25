using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Visualizer : MonoBehaviour
{
    public int cubeCount=100;
    public float radius=5f;
    public GameObject barCubes;
    public float height=10f;
    public GameObject randCircles; 
    public GameObject CornerCircles;
    public GameObject car; 
    private GameObject[] cubes;
    private GameObject[] cornerCircles; 
    private AudioSource audioSource;
    private float[] audioSamples=new float[512];
    private float[] spectrumBuffer=new float[512];
    private float[] beatBuffer=new float[512];
    private GameObject carInstance; 
    private bool circlesSpawned=false; 
    public float sampleValue;
    public GameObject TokyoCube; 
    private bool cubesSpawned=false;
    public GameObject outroSpherePrefab; 
    private GameObject outroSphere;
    private bool outroStarted=false;
    private float outroStartTime=102f; 

    void Start()
    {
        audioSource=GetComponent<AudioSource>();
        cubes=new GameObject[cubeCount];
        for (int i=0; i < cubeCount; i++)
        {
            float angle=i*Mathf.PI * 2f / cubeCount;
            Vector3 position=new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            cubes[i]=Instantiate(barCubes, position, Quaternion.identity, transform);
            cubes[i].transform.localScale=new Vector3(0.5f, 0.5f, 0.5f);
        }
        StartCoroutine (TimeStamps());
    }

    void Update()
    {
        audioSource.GetSpectrumData(audioSamples, 0, FFTWindow.Blackman);
        for (int i=0; i<cubeCount; i++)
        {
            float sampleValue=audioSamples[i % audioSamples.Length];
            float scaledHeight=Mathf.Clamp(sampleValue * height, 0.1f, 5f);

            Vector3 cubeScale=cubes[i].transform.localScale;
            cubeScale.y=scaledHeight;
            cubes[i].transform.localScale = cubeScale;

            Vector3 cubePosition=cubes[i].transform.position;
            cubePosition.y=scaledHeight / 2f;
            cubes[i].transform.position=cubePosition;
        }
        sampleValue = Mathf.Max(audioSamples);  
        float scaledSize=Mathf.Clamp(sampleValue * height, 0.5f, 2f); 
        if (cornerCircles != null)
        {
            foreach (GameObject circle in cornerCircles)
            {
                if (circle != null)
                {
                Vector3 newScale=new Vector3(3f, 3f, 3f) * scaledSize;
                circle.transform.localScale = newScale;
                }
            }
        }
        DetectBeats();
        // if (carInstance != null)
        // {
        //     carInstance.transform.Rotate(20 * Time.deltaTime, 0, 0 );
        // }
    }

    private void DetectBeats()
    {
        for (int i=0; i<audioSamples.Length; i++)
        {
            spectrumBuffer[i]=(spectrumBuffer[i] + audioSamples[i]) / 2f;
            beatBuffer[i]=audioSamples[i] - spectrumBuffer[i];
        }
    }

private IEnumerator TimeStamps()
{
    while (true)
    {
        float currTime = audioSource.time;

        if (currTime>=4f && currTime<=10f)
        {
            StartCoroutine(SpawnCircles());
        }
        if (currTime>=11f && !circlesSpawned)
        {
            SpawnCornerCircles();
            circlesSpawned=true; 
        }
    
        if (currTime>= 56f && currTime<=62f && !cubesSpawned)
        {
            StartCoroutine(SpawnCubes());
            cubesSpawned=true; 
        }
        
        if (currTime>62f && cubesSpawned)
        {
            cubesSpawned=false;
        }

        if (currTime>= outroStartTime && !outroStarted)
        {
            StartCoroutine(Outro());
            outroStarted=true;
        }

        yield return new WaitForSeconds(0.1f); 
    }
}

private IEnumerator SpawnCubes()
{
    for (int i=0; i<20; i++) 
    {
        Vector3 randomPosition=new Vector3(Random.Range(-10f,10f), Random.Range(5f,10f), Random.Range(-10f,10f));
        GameObject specialCube=Instantiate(TokyoCube, randomPosition, Quaternion.Euler(Random.Range(0,360), Random.Range(0,360), Random.Range(0,360)));
        StartCoroutine(MoveAndDestroyCube(specialCube));
        yield return new WaitForSeconds(0.3f); 
    }
}

private IEnumerator MoveAndDestroyCube(GameObject cube)
{
    Vector3 startPos=cube.transform.position;
    Vector3 endPos=startPos + new Vector3(Random.Range(-10f,10f), Random.Range(-10f,10f), Random.Range(-10f,10f));
    float duration=10f; 
    float elapsed=0f;
    Vector3 rotationSpeed = new Vector3(
        Random.Range(-180f,180f),
        Random.Range(-180f,180f),
        Random.Range(-180f,180f)
    );
    while (elapsed < duration)
    {
        cube.transform.position=Vector3.Lerp(startPos, endPos, elapsed / duration);
        cube.transform.Rotate(rotationSpeed * Time.deltaTime);
        float scale=1f+(Mathf.Max(audioSamples) * 5f);
        cube.transform.localScale=new Vector3(scale, scale, scale);
        elapsed+=Time.deltaTime;
        yield return null;
    }

    Destroy(cube);
}

    private IEnumerator SpawnCircles()
    {
        for (int i=0; i<10; i++)
        {
            Vector3 randomPosition=new Vector3(Random.Range(-10f, 10f), Random.Range(5f, 10f), Random.Range(-10f, 10f));
            GameObject circle=Instantiate(randCircles, randomPosition, Quaternion.identity);
            StartCoroutine(MoveAndDestroyCircle(circle));
            yield return new WaitForSeconds(0.2f); 
        }
    }

    private IEnumerator MoveAndDestroyCircle(GameObject circle)
    {
        Vector3 startPos=circle.transform.position;
        Vector3 endPos=startPos+new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        float duration=2f;
        float elapsed=0f;
        while (elapsed<duration)
        {
            circle.transform.position=Vector3.Lerp(startPos,endPos,elapsed / duration);
            elapsed+=Time.deltaTime;
            yield return null;
        }
        Destroy(circle);
    }


private void SpawnCar()
{
    Vector3 carPosition=transform.position;
    Quaternion carRotation=Quaternion.Euler(0, 0, 0); 
    carInstance=Instantiate(car, carPosition, carRotation);
    //carInstance.transform.localRotation = carRotation;
    //carInstance.transform.localPosition = new Vector3(0, 0.2f, -carLength / 2);
    //carInstance.transform.localScale = new Vector3(6f, 6f, 6f);
}
private void SpawnCornerCircles()
{
    cornerCircles=new GameObject[4];

    Vector3[] corners=new Vector3[]
    {
        new Vector3(13.53f, 5.62f, 2.25f),
        new Vector3(-12.53f, 5.62f, 2.25f),
        new Vector3(-13.53f, -4f, 2.25f),
        new Vector3(14.53f, -4.62f, 2.25f)
    };

    for (int i = 0; i < 4; i++)
    {
        cornerCircles[i]=Instantiate(CornerCircles, corners[i], Quaternion.identity);
        cornerCircles[i].transform.localScale=new Vector3(3f, 3f, 3f); 
    }
}

private IEnumerator Outro()
{
    outroSphere=Instantiate(outroSpherePrefab, new Vector3(0.21f, 0.69f, 3.69f), Quaternion.identity);
    outroSphere.transform.localScale = Vector3.zero; 
    float growDuration=3f;
    float elapsedTime=0f;
    
    while (elapsedTime<growDuration)
    {
        float t=elapsedTime / growDuration;
        outroSphere.transform.localScale = Vector3.one*t*5f; 
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    yield return new WaitForSeconds(1f);
    foreach (GameObject cube in cubes)
    {
        cube.SetActive(false);
    }
    if (cornerCircles!=null)
    {
        foreach (GameObject circle in cornerCircles)
        {
            if (circle!=null)
            {
                Destroy(circle);
            }
        }
    }
    StartCoroutine(MoveOutroSphere());
}

private IEnumerator MoveOutroSphere()
{
    while (audioSource.isPlaying)
    {
        audioSource.GetSpectrumData(audioSamples, 0, FFTWindow.Blackman);
        float maxSample=Mathf.Max(audioSamples);
        float _base=5f;
        float pulse=_base + (maxSample * 20f);
        outroSphere.transform.localScale=Vector3.one * pulse;
        outroSphere.transform.Rotate(0f, 90f * Time.deltaTime, 0f);
        yield return null;
    }
}
}