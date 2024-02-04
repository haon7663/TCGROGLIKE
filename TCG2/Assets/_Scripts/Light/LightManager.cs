using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager Inst;
    void Awake() => Inst = this;

    [SerializeField] Animator SelectedLight;
    [SerializeField] Animator CardLight;

    void Start()
    {
        ChangeLight(false);
    }

    public void ChangeLight(bool onCard)
    {
        SelectedLight.SetBool("isOn", !onCard);
        CardLight.SetBool("isOn", onCard);
    }
}
