using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

public class CameraMovement : MonoBehaviour
{
    public static CameraMovement inst;
    private void Awake()
    {
        inst = this;
        _camera = GetComponent<Camera>();
    }

    private Camera _camera;

    public float shakeAmount;
    private float _shakeTime;
    private float _originShakeTime;
    
    private Vector3 _initialPosition = new Vector3(0, 0, -10);
    private float _initialRotationZ = 0;
    private float _initialOrthographicSize = 5;
    
    private void FixedUpdate()
    {
        //transform.position = _initialPosition;
        //transform.rotation = Quaternion.Euler(0, 0, _initialRotationZ);
        //_camera.orthographicSize = _initialOrthographicSize;
        
        if (_shakeTime <= 0)
            return;
        
        transform.position += Random.insideUnitSphere * (shakeAmount * (_shakeTime / _originShakeTime));
        _shakeTime -= Time.deltaTime;
    }

    public void VibrationForTime(float time)
    {
        _shakeTime = time;
        _originShakeTime = time;
    }

    public void ProductionAtTime(Vector3 position, float rotationZ, float orthographicSize, bool useDotween = false, float dotweenTime = 0.1f)
    {
        transform.DOKill(true);
        if (useDotween)
        {
            DOTween.To(() => _initialPosition, x => _initialPosition = x, position, dotweenTime);
            DOTween.To(() => _initialRotationZ, x => _initialRotationZ = x, rotationZ, dotweenTime);
            DOTween.To(() => _initialOrthographicSize, x => _initialOrthographicSize = x, orthographicSize, dotweenTime);
        }
        else
        {
            _initialPosition = position;
            _initialRotationZ = rotationZ;
            _initialOrthographicSize = orthographicSize;
        }
    }
}
