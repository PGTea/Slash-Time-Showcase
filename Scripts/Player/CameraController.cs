using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    private Vector3 offset;
    private Transform cam;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = this.GetComponent<Transform>();
        offset = cam.position - player.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        cam.position = player.position + offset;
    }
}
