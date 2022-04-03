using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Stars : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] Light light;
    [SerializeField] float intensityIncrease;

    [Header("Stars")]
    [SerializeField] GameObject star;
    [SerializeField] int numStars;
    [SerializeField] Vector2 topLeft;
    [SerializeField] Vector2 bottomRight;
    [SerializeField] float maxRadius;
    [SerializeField] float maxBrightness;

    [Header("Expand")]
    [SerializeField] float expandTime;
    [SerializeField] float[] expandScale;
    [SerializeField] float[] expandRate;
    [SerializeField] float[] expandDrop;
    [SerializeField] float[] expandRotationRate;

    [Header("Spin")]
    [SerializeField] float spinTime;

    [Header("Fade")]
    [SerializeField] float fadeRate;
    [SerializeField] float fadeTime;
    [SerializeField] float[] fadeScale;
    [SerializeField] float[] fadeRotation;

    [Header("Player")]
    [SerializeField] Player player;

    GameObject[] stars;
    Star[] starData;
    GameObject vortex;
    GameObject[] outerStars;
    GameObject square2;
    GameObject[] StarObjects;
    SpriteRenderer squareRenderer;
    SpriteRenderer squareRenderer2;
    float widthFactor;
    
    public class Star
    {
        float expandRate;
        float expandDrop;
        public Vector3 position;
        public Vector3 scale;

        public Star(float rate, float drop, Vector3 initialPosition, Vector3 initialScale)
        {
            expandRate = rate + (UnityEngine.Random.Range(-1, 1) * rate * 0.1f);
            expandDrop = drop + (UnityEngine.Random.Range(-1, 1) * drop * 0.1f);

            position = initialPosition;
            scale = initialScale;
        }

        public void Expand()
        {
            position.x += expandDrop;
            position.y += expandDrop;
            scale.x += expandRate;
            scale.y += expandRate;
        }

        public void Shrink()
        {
            scale.x -= expandRate * 10;
            scale.y -= expandRate * 10;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        widthFactor = Screen.currentResolution.width / 1920f;
        stars = new GameObject[numStars];
        starData = new Star[numStars];
        square2 = GameObject.Find("Square2");
        StarObjects = new GameObject[2];
        StarObjects[0] = GameObject.Find("Stars");
        StarObjects[1] = GameObject.Find("Stars2");
        vortex = GameObject.Find("Vortex");
        light.intensity = 0;
        float amplifier = 1 + Mathf.Log(widthFactor, 1024);
        for (int i = 0; i < expandRate.Length; i++)
        {
            expandScale[i] *= amplifier;
            expandRate[i] *= amplifier;
            expandRotationRate[i] *= amplifier;
            expandDrop[i] *= amplifier;
            intensityIncrease *= amplifier; // 1 + Mathf.Log(widthFactor, 1024); // (widthFactor * 0.75f);
        }


        for (int i = 0; i < numStars; i++)
        {
            float radius = UnityEngine.Random.Range(0, maxRadius);
            float radians = UnityEngine.Random.Range(0, 2 * Mathf.PI);
            float x = Mathf.Cos(radians) * radius * 1.2f;
            float y = Mathf.Sin(radians) * radius * 1.2f;
            //float x = UnityEngine.Random.Range(topLeft.x, bottomRight.x);
            //float y = UnityEngine.Random.Range(topLeft.y, bottomRight.y);

            stars[i] = Instantiate(star, StarObjects[i % 2].transform, false);
            stars[i].transform.localPosition = new Vector3(x, y, 0f);
            stars[i].transform.localScale = new Vector3(0, 0, 1);
            starData[i] = new Star(expandRate[i % 2], expandDrop[i % 2], stars[i].transform.localPosition, stars[i].transform.localScale);
        }

        // BeginSequence();
    }

    public void BeginSequence()
    {
        StartCoroutine(Wait(Time.time, 3f, Expand));
    }

    IEnumerator Wait(float startTime, float duration, Action callable)
    {
        while (Time.time < startTime + duration)
        {
            yield return null;
        }
        callable();
    }

    public void Expand()
    {
        float startTime = Time.time;
        StartCoroutine(ExpandStars(startTime, startTime + expandTime));
    }

    public void Spin()
    {
        float startTime = Time.time;
        StartCoroutine(SpinStars(startTime, spinTime));
    }
    
    public void Fade()
    {
        StartCoroutine(FadeStars(Time.time));
    }

    IEnumerator ExpandStars(float startTime, float endTime)
    {
        while (Time.time < startTime + expandTime)
        {
            for (int i = 0; i < numStars; i++)
            {
                starData[i].Expand();
                stars[i].transform.localScale = starData[i].scale;
                stars[i].transform.localPosition = starData[i].position;
            }

            for (int i = 0; i < 2; i++)
            {
                Vector3 pos = StarObjects[i].transform.localPosition;
                pos.y -= expandDrop[i];

                Vector3 rot = StarObjects[i].transform.rotation.eulerAngles;
                rot.z += expandRotationRate[i];

                Vector3 scale = StarObjects[i].transform.localScale;
                scale.x += expandScale[i];
                scale.y += expandScale[i]; 

                StarObjects[i].transform.localPosition = pos;
                StarObjects[i].transform.rotation = Quaternion.Euler(rot);
                StarObjects[i].transform.localScale = scale;
            }

            light.intensity += intensityIncrease;
            //float v = maxBrightness * (Time.time - startTime) / (endTime - startTime);
            //quareRenderer.color = Color.HSVToRGB(0f, 0f, v);
            yield return null;
        }
        Spin();
    }
    IEnumerator SpinStars(float startTime, float endTime)
    {
        while (Time.time < startTime + endTime)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector3 rot = StarObjects[i].transform.rotation.eulerAngles;
                rot.z += expandRotationRate[i];
                StarObjects[i].transform.rotation = Quaternion.Euler(rot);

            }
            yield return null;
        }
        Fade();
    }
    
    IEnumerator FadeStars(float startTime)
    {
        while (Time.time < startTime + fadeTime)
        {
            foreach (GameObject star in stars)
            {
                Vector3 pos = star.transform.position;
                pos = Vector3.MoveTowards(pos, vortex.transform.position, fadeRate);

                //Vector3 scale = star.transform.localScale;
                //scale.x += 2f;

                // star.transform.localScale = scale;
                star.transform.position = pos;
            }
            
            for (int i = 0; i < 2; i++)
            {
                Vector3 rot = StarObjects[i].transform.rotation.eulerAngles;
                Vector3 scale = StarObjects[i].transform.localScale;
                scale.x *= fadeScale[i];
                scale.y *= fadeScale[i];
                rot.z += fadeRotation[i];
                StarObjects[i].transform.localScale = scale;
                StarObjects[i].transform.rotation = Quaternion.Euler(rot);
            }
            light.intensity -= intensityIncrease * (expandTime / fadeTime);
            yield return null;
        }
        //player.ActivateLight();
    }




}
