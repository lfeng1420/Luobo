using UnityEngine;
using System.Collections;

public class LeafRotation : MonoBehaviour
{
    TweenRotation tp = null;
    TweenRotation tp1 = null;
    // Use this for initialization
    void Start()
    {
        Transform find = null;
        find = transform.Find("Leaf-2");

        if(find != null)
        {
            tp = find.GetComponent<TweenRotation>();
            if (tp != null)
            {
                EventDelegate.Add(tp.onFinished, RotationFinished);
                tp.enabled = true;
            }
        }

        find = transform.Find("Leaf-3");
        if (find != null)
        {
            tp1 = find.GetComponent<TweenRotation>();
            if (tp1 != null)
            {
                EventDelegate.Add(tp1.onFinished, Rotation1Finished);
                tp1.enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void RotationFinished()
    {
        tp.enabled = false;
        tp1.enabled = true;
        tp1.ResetToBeginning();
    }

    void Rotation1Finished()
    {
        tp1.enabled = false;
        tp.enabled = true;
        tp.ResetToBeginning();
    }
}
