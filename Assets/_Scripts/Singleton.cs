using Unity.VisualScripting;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _inst;

    public static T Inst
    {
        get
        {
            if (!_inst)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                _inst = (T)FindObjectOfType(typeof(T));

                if (!_inst)
                {
                    var obj = new GameObject(typeof(T).Name, typeof(T));
                    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                    _inst = obj.GetComponent<T>();
                }
            }
            return _inst;
        }
    }

    private void Awake()
    {
        if (transform.parent != null && transform.root != null)
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}