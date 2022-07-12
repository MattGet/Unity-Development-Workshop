using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireworksMania.Core.Behaviors.Fireworks.Parts;

public class Playanim : MonoBehaviour
{
    public Fuse fuse;
    private Animator laser;
    [SerializeField] private string clip = "test 0";
    private bool once = false;
    // Start is called before the first frame update
    void Start()
    {
        laser = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fuse.IsUsed == true && once == false)
        {
            laser.Play(clip, 0, 0.0f);
            once = true;
        }
    }
}
