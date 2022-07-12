using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTesting : MonoBehaviour
{
    public bool test = false;
    public Vector3 Top;
    public Vector3 Bottom;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnValidate()
    {
        Top = new Vector3(this.transform.up.x, this.transform.up.y, this.transform.up.z);
        Bottom = new Vector3(-this.transform.up.x, -this.transform.up.y, -this.transform.up.z);
    }
}
