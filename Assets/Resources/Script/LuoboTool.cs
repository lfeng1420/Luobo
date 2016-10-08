#define _DEBUG_

using UnityEngine;
using System.Collections;

public class LuoboTool
{
    ///<summary>
    ///更改GameObject状态;
    ///</summary>
    public static void changeState(Transform transform, bool b)
    {
        if (transform == null) return;
        transform.gameObject.SetActive(b);
    }
    
    ///<summary>
    ///更改GameObject状态;
    ///</summary>
    public static void changeState(Transform root, string path, bool b)
    {
        if (root == null) return;
        Transform t = getTransform(root, path);
        if (t == null) return;
        t.gameObject.SetActive(b);
    }
    
    ///<summary>
    //更改GameObject状态根据当前GameObject状态取相反状态;
    ///</summary>
    public static void changeState(Transform root, string path)
    {
        if (root == null || path == "") return;
        Transform t = LuoboTool.getTransform(root, path);
        changeState(t);
    }
    
    ///<summary>
    //更改GameObject状态根据当前GameObject状态取相反状态;
    ///</summary>
    public static void changeState(Transform t)
    {
        if (t == null) return;
        bool state = t.gameObject.activeSelf;
        changeState(t, !state);
    }

    
    public static GameObject cloneObj(GameObject parent, GameObject target, Vector3 position, string name)
    {
        if (target == null || parent == null) return null;
        Vector3 scale = target.transform.localScale;
        GameObject clone = GameObject.Instantiate(target) as GameObject;
        clone.layer = target.layer;
        clone.transform.parent = parent.transform;
        clone.transform.localScale = scale;
        clone.name = name;
        clone.transform.localPosition = position;
        clone.SetActive(true);

        return clone;
    }


    public static GameObject cloneObj(Transform parent, Transform dest, Vector3 position, string name)
    {
        if (dest == null || parent == null) return null;
        GameObject target = dest.gameObject;
        Vector3 scale = target.transform.localScale;
        GameObject clone = GameObject.Instantiate(target) as GameObject;
        clone.layer = target.layer;
        initObj(clone.transform, parent, name, position, scale);

        return clone;
    }

    public static void initObj(Transform t, Transform parent, string name, Vector3 pos, 
        bool b = true)
    {
        if (t == null || parent == null) return;
        t.parent = parent;
        t.localPosition = pos;
        t.localScale = Vector3.one;
        t.gameObject.name = name;
        t.gameObject.SetActive(b);
    }

    public static void initObj(Transform t, Transform parent, string name, Vector3 pos, 
        Vector3 scale, bool b = true)
    {
        if (t == null || parent == null) return;
        t.parent = parent;
        t.localPosition = pos;
        t.localScale = scale;
        t.gameObject.name = name;
        t.gameObject.SetActive(b);
    }
    
    ///<summary>
    //获取GameObject，根据传入的needCreate决定是否需要创建GameObject;
    ///</summary>
    public static GameObject getGameObject(string name, bool needCreate)
    {
        GameObject go = GameObject.Find(name);
        if (go == null && needCreate)
        {
            go = new GameObject(name);
        }

        return go;
    }


    public static Vector2 getSpriteSize(Transform parent, string path)
    {
        UISprite sprite = getSprite(parent, path);
        if (sprite == null) return Vector2.zero;
        else return sprite.localSize;
    }


    public static Vector2 getSpriteSize(Transform t)
    {
        UISprite sprite = getSprite(t);
        if (sprite == null) return Vector2.zero;
        else return sprite.localSize;
    }


    public static UISprite getSprite(Transform parent, string path)
    {
        if (parent == null) return null;
        Transform t = getTransform(parent, path);
        return getSprite(t);
    }

    public static UISprite getSprite(Transform t)
    {
        if (t == null) return null;
        UISprite sprite = t.GetComponent<UISprite>();
        return sprite;
    }

    public static void setSprite(UISprite sprite, string spriteName, UIAtlas atlas = null, bool needPixelPerfect = true)
    {
        if (sprite == null) return;
        if(atlas != null)
        {
            sprite.atlas = atlas;
        }
        sprite.spriteName = spriteName;
        if (needPixelPerfect)
        {
            sprite.MakePixelPerfect();
        }
    }

    public static void setSprite(Transform t, string spriteName, UIAtlas atlas = null, bool needPixelPerfect = true)
    {
        if (t == null) return;
        UISprite sprite = getSprite(t);
        setSprite(sprite, spriteName, atlas, needPixelPerfect);
    }

    public static void setSprite(Transform parent, string path, string spriteName, UIAtlas atlas = null, bool needPixelPerfect = true)
    {
        if (parent == null) return;
        Transform t = getTransform(parent, path);
        setSprite(t, spriteName, atlas, needPixelPerfect);
    }

    public static Transform getTransform(Transform root, string name)
    {
        if (root == null) return null;
        return root.Find(name);
    }


    public static UIEventListener getEventListener(Transform t, bool needAdd = true)
    {
        if (t == null) return null;
        UIEventListener eventListener = t.gameObject.GetComponent<UIEventListener>();
        if(eventListener == null)
        {
            eventListener = t.gameObject.AddComponent<UIEventListener>();
        }

        return eventListener;
    }


    public static UIEventListener getEventListener(GameObject go, bool needAdd = true)
    {
        if (go == null) return null;
        UIEventListener eventListener = go.GetComponent<UIEventListener>();
        if (eventListener == null && needAdd)
        {
            eventListener = go.AddComponent<UIEventListener>();
        }

        return eventListener;
    }


    public static string getSubstirng(string src, string find, bool fromFirstChar = true)
    {
        int index = src.IndexOf(find);
        if(fromFirstChar)
        {
            return src.Substring(0, index);
        }
        else
        {
            return src.Substring(index + 1);
        }
    }

    //LOG
    public static void Log(string info)
    {
#if (_DEBUG_)
        Debug.Log(info);
#endif
    }

    public static void LogError(string info)
    {
#if (_DEBUG_)
        Debug.LogError(info);
#endif
    }

    public static void LogWarning(string info)
    {
#if (_DEBUG_)
        Debug.LogWarning(info);
#endif
    }
}
