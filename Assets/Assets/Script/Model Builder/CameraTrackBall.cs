using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrackBall : MonoBehaviour {

    private Transform _transform;
    private Vector3 _mousePos;
    private Ray _ray;

    private float _xDeg = 0.0f;
    private float _yDeg = 0.0f;

    [SerializeField]
    private float _movingOffset;
    [SerializeField]
    private float _rotateSpeed;

	void Start () {
        _transform = this.transform;
        StartCoroutine(HandleInputEvent());
	}
	
    private IEnumerator HandleInputEvent()
    {
        while(true)
        {
            var pos = _transform.localPosition;

            if (Input.GetKey(KeyCode.W))
            {
                _transform.Translate(0.0f, 0.0f, _movingOffset);
            }
            if (Input.GetKey(KeyCode.S))
            {
                _transform.Translate(0.0f, 0.0f, -_movingOffset);
            }
            if (Input.GetKey(KeyCode.A))
            {
                _transform.Translate(-_movingOffset, 0.0f, 0.0f);
            }
            if (Input.GetKey(KeyCode.D))
            {
                _transform.Translate(_movingOffset, 0.0f, 0.0f);
            }
            if (Input.GetMouseButton(1))
            {
                _xDeg += Input.GetAxis("Mouse X");
                _yDeg -= Input.GetAxis("Mouse Y");
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(_yDeg, _xDeg, 0), Time.deltaTime* _rotateSpeed);
            }

            _mousePos = Input.mousePosition;
            _ray = Camera.main.ScreenPointToRay(_mousePos);

            yield return null;
        }
    }
}
