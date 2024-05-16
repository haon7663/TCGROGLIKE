using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{
    public static CameraManager inst;
    private void Awake()
    {
        inst = this;
    }

    private Camera _camera;
    
    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    [SerializeField] private float zoomSize;
    [SerializeField] private float normalSize; 
    private readonly float _directionForceMin = 0.01f;

    private bool _isZoom;
    private bool _userMoveInput;
    private Vector3 _startPosition;
    private Vector3 _directionForce;

    private void LateUpdate()
    {
        if (CardManager.Inst.hoveredCard || ArrangeManager.inst.isArrange || UnitManager.inst.isDrag || _isZoom)
            return;

        ControlCameraPosition();

        ReduceDirectionForce();

        UpdateCameraPosition();
    }

    private void ControlCameraPosition()
    {
        var mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
            CameraPositionMoveStart(mouseWorldPosition);
        else if (Input.GetMouseButton(0))
            CameraPositionMoveProgress(mouseWorldPosition);
        else
            CameraPositionMoveEnd();
    }
    private void CameraPositionMoveStart(Vector3 startPosition)
    {
        _userMoveInput = true;
        _startPosition = startPosition;
        _directionForce = Vector2.zero;
    }
    private void CameraPositionMoveProgress(Vector3 targetPosition)
    {
        if (!_userMoveInput)
            return;

        _directionForce = _startPosition - targetPosition;
    }
    private void CameraPositionMoveEnd()
    {
        _userMoveInput = false;
    }
    private void ReduceDirectionForce()
    {
        if (_userMoveInput)
            return;

        _directionForce = Vector3.Lerp(_directionForce, Vector3.zero, Time.deltaTime * 15f);
        if (_directionForce.magnitude < _directionForceMin)
            _directionForce = Vector3.zero;
    }
    private void UpdateCameraPosition()
    {
        if (_directionForce == Vector3.zero)
            return;

        var currentPosition = transform.position;
        var targetPosition = currentPosition + _directionForce;
        transform.position = Vector3.Lerp(currentPosition, targetPosition, 0.5f);
    }

    public void SetOrthographicSize(bool isZoom, bool useDotween = true, float dotweenTime = 0.5f)
    {
        _isZoom = isZoom;
        if (useDotween)
            DOTween.To(() => _camera.orthographicSize, x => _camera.orthographicSize = x, isZoom ? zoomSize : normalSize, dotweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
        else
            _camera.orthographicSize = isZoom ? zoomSize : normalSize;
    }
    public void SetViewPoint(Vector3 point, bool useDotween = true, float dotweenTime = 0.5f)
    {
        point = new Vector3(point.x, point.y, -20);
        if (useDotween)
            transform.DOMove(point, dotweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
        else
            transform.position = point;
    }
}
