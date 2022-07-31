using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected bool isDontDestroyObj;

    protected static T instance = null;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    instance = new GameObject().AddComponent<T>();
                }
            }
            return instance;
        }
    }

}
