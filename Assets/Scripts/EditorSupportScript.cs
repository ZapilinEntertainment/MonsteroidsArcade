using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EditorSupportScript : MonoBehaviour
{
    public static EditorSupportScript Instance { get {
            if (Current == null) Current = new GameObject("EditorSupportScript").AddComponent<EditorSupportScript>();
            return Current;
        }        }
    public static EditorSupportScript Current { get; private set; }
    private RectTransform _target, _disk;
    private float _radius;
    private bool _draw = false;

    private void Awake()
    {
        if (Application.isPlaying) Destroy(gameObject);
        else
        {
            //_disk = FindObjectOfType<Canvas>().
        }
    }

}
