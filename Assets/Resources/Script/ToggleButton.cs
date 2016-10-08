using UnityEngine;
using System.Collections;

public class ToggleButton : MonoBehaviour 
{
    public UIButton Button = null;
    public string newNormalSpriteName = "";
    public string newHoverSpriteName = "";
    public string newPressedSpriteName = "";
    public string newDisabledSpriteName = "";

    string oldNormalSpriteName = "";
    string oldHoverSpriteName = "";
    string oldPressedSpriteName = "";
    string oldDisabledSpriteName = "";

    private bool isNew = true;

	// Use this for initialization
	void Start () 
    {
        if (Button == null)
        {
            Button = gameObject.GetComponent<UIButton>();
        }

        UIEventListener eventListener = gameObject.GetComponent<UIEventListener>();
        if(eventListener == null)
        {
            eventListener = gameObject.AddComponent<UIEventListener>();
        }
        eventListener.onClick += onButtonClick;

        oldNormalSpriteName = Button.normalSprite;
        oldPressedSpriteName = Button.pressedSprite;
        oldHoverSpriteName = Button.hoverSprite;
        oldDisabledSpriteName = Button.disabledSprite;

        if (newNormalSpriteName == "")
        {
            newNormalSpriteName = oldNormalSpriteName;
        }
        if (newHoverSpriteName == "")
        {
            newHoverSpriteName = oldHoverSpriteName;
        }
        if(newPressedSpriteName == "")
        {
            newPressedSpriteName = oldPressedSpriteName;
        }
        if(newDisabledSpriteName == "")
        {
            newDisabledSpriteName = oldDisabledSpriteName;
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
	    
	}

    void onButtonClick(GameObject go)
    {
        if(isNew)
        {
            Button.normalSprite = newNormalSpriteName;
            Button.hoverSprite = newHoverSpriteName;
            Button.pressedSprite = newPressedSpriteName;
            Button.disabledSprite = newDisabledSpriteName;
        }
        else
        {
            Button.normalSprite = oldNormalSpriteName;
            Button.hoverSprite = oldHoverSpriteName;
            Button.pressedSprite = oldPressedSpriteName;
            Button.disabledSprite = oldDisabledSpriteName;
        }
        isNew = !isNew;
    }
}
