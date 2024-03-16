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

    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            LightManager.Inst.ChangeLight(false);
            SetOrthoSize(9);
            SetViewPoint(new Vector3(0, 0));

            UnitManager.Inst.DeSelectUnit(UnitManager.sUnit);
            GridManager.Inst.RevertTiles();
        }
    }

    public void SetOrthoSize(float size, bool useDotween = true, float dotweenTime = 0.5f)
    { 
        transform.DOKill();

        if(useDotween)
            DOTween.To(() => cinevirtual.m_Lens.OrthographicSize, x => cinevirtual.m_Lens.OrthographicSize = x, size, dotweenTime).SetEase(Ease.OutCirc).SetUpdate(UpdateType.Late);
        else
            cinevirtual.m_Lens.OrthographicSize = size;
    }

    public void SetViewPoint(Vector3 point)
    {
        viewPoint.position = point;
    }
}
