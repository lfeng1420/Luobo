using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour {

    static SceneManager _sceneManager = null;
    static GameObject _sceneHolder = null;
    static Transform _sceneParent = null;
    private bool canOpen = true;

    public static SceneManager getInstance()
    {
        if (_sceneHolder == null)
        {
            _sceneHolder = LuoboTool.getGameObject("SceneManager", true);
        }
        if(_sceneParent == null)
        {
            _sceneParent = _sceneHolder.transform.Find("ui/Camera/Anchor");
        }
        if(_sceneManager == null)
        {
            _sceneManager = _sceneHolder.GetComponent<SceneManager>();
            if(_sceneManager == null)
            {
                _sceneManager = _sceneHolder.AddComponent<SceneManager>();
            }
        }

        return _sceneManager;
    }

    /// <summary>
    /// 打开窗口;
    /// </summary>
    /// <param name="name"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public IEnumerator OpenWindow(string name, float delay, bool waitSceneVisble, 
        bool isLoading, bool needCloseOther)
    {
        //如果需要关闭其他窗口;
        if (needCloseOther)
        {
            closeWindow();
        }
        if (delay > 0)
        {
            setWaitScene(waitSceneVisble, isLoading);
            yield return new WaitForSeconds(delay);
        }
        GameObject go = GameObject.Instantiate(Resources.Load(name, typeof(GameObject)), 
            Vector3.zero, new Quaternion(0, 0, 0, 0)) as GameObject;
        name = name.Replace("/", "_");
        go.name = name;
        go.transform.parent = _sceneParent;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        setWaitScene(false);
        go.SetActive(true);
        canOpen = true;
        yield return null;
    }

    public void openWindow(string name, float delay, bool waitSceneVisble = true, 
        bool isLoading = true, bool needCloseOther = true)
    {
        if (canOpen)
        {
            canOpen = false;
            StartCoroutine(OpenWindow(name, delay, waitSceneVisble, isLoading, needCloseOther));
        }
    }

    public void setWaitScene(bool isVisible, bool isLoading = true)
    {
        LuoboTool.changeState(_sceneParent, "WaitScene", isVisible);
        if (!isVisible) return;
        LuoboTool.changeState(_sceneParent, "WaitScene/Loading", isLoading);
        LuoboTool.changeState(_sceneParent, "WaitScene/Logo", !isLoading);
    }


    /// <summary>
    /// 关闭其他窗口;
    /// </summary>
    public void closeWindow()
    {
        Transform trans = _sceneParent;
        if (trans == null) return;

        //如果只有一个子物体，说明只有WaitScene，无需关闭窗口;
        if (trans.childCount == 1) return;

        for(int i = 0; i < trans.childCount; i++)
        {
            Transform child = trans.GetChild(i);
            if (child.gameObject.name.Equals("WaitScene")) continue;
            LuoboTool.Log("[Destory]:" + child.gameObject.name);
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);//销毁窗口;
        }
    }

    public void closeWindow(string winName)
    {
        if(winName == "")
        {
            LuoboTool.LogError("Cannot close window, 'winName' is EMPTY!");
            return;
        }
        Transform trans = _sceneParent.Find(winName);
        if(trans == null)
        {
            LuoboTool.LogError("Cannot find window:" + winName);
            return;
        }

        trans.gameObject.SetActive(false);
        Destroy(trans.gameObject);//销毁窗口;
    }

    
}
