using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float panSpeed;
    [SerializeField] float panWidth;
    float panUnits;
    Camera cam;


    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        panUnits = cam.orthographicSize * panWidth;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 viewPos = cam.WorldToViewportPoint(player.position);
        if (viewPos.x > 1) StartCoroutine(PanRight(transform.position));
        else if (viewPos.x < 0) StartCoroutine(PanLeft(transform.position));
    }

    IEnumerator PanRight(Vector3 initialPosition)
    {
        while (transform.position.x >= initialPosition.x + panUnits)
        {
            transform.position = new Vector3(transform.position.x + panSpeed, initialPosition.y, initialPosition.z);
            yield return null;
        }
        transform.position = new Vector3(transform.position.x - panUnits, initialPosition.y, initialPosition.z);
    }

    IEnumerator PanLeft(Vector3 initialPosition)
    {
        while (transform.position.x >= initialPosition.x - panUnits)
        {
            transform.position = new Vector3(transform.position.x - panSpeed, initialPosition.y, initialPosition.z);
            yield return null;
        }
        transform.position = new Vector3(transform.position.x - panUnits, initialPosition.y, initialPosition.z);
    }
}
