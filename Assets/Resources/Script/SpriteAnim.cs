using UnityEngine;
using System.Collections;

public class SpriteAnim : MonoBehaviour
{
    public bool isStart;
    public float delay;
    public string[] namePrefix;
    public bool isRand;
    bool isRunning = false;
    UISpriteAnimation spriteAnim = null;

    void Start()
    {
        spriteAnim = transform.GetComponent<UISpriteAnimation>();
        if(spriteAnim == null)
        {
            spriteAnim = gameObject.AddComponent<UISpriteAnimation>();
        }

        spriteAnim.namePrefix = namePrefix[0];
        spriteAnim.loop = false;
    }

    void Update()
    {
        if(isStart)
        {

        }
    }
}