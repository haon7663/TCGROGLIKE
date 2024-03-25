using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CinemachineManager : MonoBehaviour
{
    public static CinemachineManager Inst;
    void Awake() => Inst = this;

    [SerializeField] CinemachineVirtualCamera cinevirtual;
    [SerializeField] Transform viewPoint;
    [SerializeField] CameraMovement cameraMovement;

    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            LightManager.Inst.ChangeLight(false);
            SetOrthoSize(false);

            UnitManager.Inst.DeSelectUnit(UnitManager.sUnit);
            GridManager.Inst.RevertTiles();
        }
    }

    public void SetOrthoSize(bool isJoom, bool useDotween = true, float dotweenTime = 0.5f)
    { 
        transform.DOKill();
        cameraMovement.SetOrthoSize(isJoom, useDotween, dotweenTime);
    }

    public void SetViewPoint(Vector3 point, bool useDotween = true, float dotweenTime = 0.5f)
    {
        point = new Vector3(point.x, point.y - 1.5f, -20);
        cameraMovement.SetViewPoint(point, useDotween, dotweenTime);
    }
}
