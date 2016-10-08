using UnityEngine;
using System.Collections;

public class ButtonPositionOffset : MonoBehaviour {

    public Vector3 offsetPos;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnPress(bool isPressed)
    {
        Vector3 pos = transform.localPosition;
        pos = (isPressed) ? pos + offsetPos : pos - offsetPos;
        transform.localPosition = pos;
    }
}
