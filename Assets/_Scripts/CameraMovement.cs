using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraMovement : MonoBehaviour
{
    Camera _camera;

    [SerializeField] float joomSize;
    [SerializeField] float normalSize;
    float directionForceMin = 0.01f;

    bool isJoom;
    bool userMoveInput;
    Vector3 startPosition;
    Vector3 directionForce;

    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    void Update()
    {
        if (CardManager.Inst.hoveredCard || isJoom)
            return;

        ControlCameraPosition();

        ReduceDirectionForce();

        UpdateCameraPosition();
    }

    void ControlCameraPosition()
    {
        var mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
            CameraPositionMoveStart(mouseWorldPosition);
        else if (Input.GetMouseButton(0))
            CameraPositionMoveProgress(mouseWorldPosition);
        else
            CameraPositionMoveEnd();
    }
    void CameraPositionMoveStart(Vector3 startPosition)
    {
        userMoveInput = true;
        this.startPosition = startPosition;
        directionForce = Vector2.zero;
    }
    void CameraPositionMoveProgress(Vector3 targetPosition)
    {
        if (!userMoveInput)
        {
            CameraPositionMoveStart(targetPosition);
            return;
        }

        directionForce = startPosition - targetPosition;
    }
    void CameraPositionMoveEnd()
    {
        userMoveInput = false;
    }
    void ReduceDirectionForce()
    {
        if (userMoveInput)
            return;

        directionForce = Vector3.Lerp(directionForce, Vector3.zero, Time.deltaTime * 15f);
        if (directionForce.magnitude < directionForceMin)
            directionForce = Vector3.zero;
    }
    void UpdateCameraPosition()
    {
        if (directionForce == Vector3.zero)
            return;

        var currentPosition = transform.position;
        var targetPosition = currentPosition + directionForce;
        transform.position = Vector3.Lerp(currentPosition, targetPosition, 0.5f);
    }

    public void SetOrthoSize(bool isJoom, bool useDotween = true, float dotweenTime = 0.5f)
    {
        this.isJoom = isJoom;
        if (useDotween)
            DOTween.To(() => _camera.orthographicSize, x => _camera.orthographicSize = x, isJoom ? joomSize : normalSize, dotweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
        else
            _camera.orthographicSize = isJoom ? joomSize : normalSize;
    }
    public void SetViewPoint(Vector3 point, bool useDotween = true, float dotweenTime = 0.5f)
    {
        if (useDotween)
            transform.DOMove(point, dotweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
        else
            transform.position = point;
    }
}
