using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CameraController : NetworkBehaviour
{
    public GameObject CrouchPosition;
    public GameObject HeadPosition;
    public GameObject FPSObject;

    public float SensitivityX = 15F;
    public float SensitivityY = 15F;
    public float MinimumY = -60F;
    public float MaximumY = 60F;

    public float CrouchMoveSpeed = 1.0f;

    [HideInInspector]
    public float SenXConst;
    [HideInInspector]
    public float SenYConst;

    private Camera _camera;
    private bool _crouched = false;
    private Transform _sightPos;
    private CharacterWeapon _characterWeapon;

    private float _rotationY = 0.0f;
    private float _rotationX = 0.0f;

    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        _characterWeapon = GetComponent<CharacterWeapon>();

        SenXConst = SensitivityX;
        SenYConst = SensitivityY;

        if(!isLocalPlayer)
        {
            _camera.enabled = false;
            _camera.GetComponentInChildren<Canvas>().enabled = false;
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (GameObject.Find("SightPos") != null)
            _sightPos = GameObject.Find("SightPos").transform;

        RotateCamera();
        SetCrouchCameraPosition();
    }

    private void RotateCamera()
    {
        _rotationX = transform.localEulerAngles.y + Input.GetAxis(InputConstants.MOUSE_X) * SensitivityX;

        _rotationY += Input.GetAxis(InputConstants.MOUSE_Y) * SensitivityY;
        _rotationY = Mathf.Clamp(_rotationY, MinimumY, MaximumY);   

        /*if (_sightPos != null && !_characterWeapon._reloading &&
            !_characterWeapon._weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("TakeGun"))
        {
            //_rotationY = _sightPos.localEulerAngles.x;
            //_rotationX = _sightPos.localEulerAngles.y;
        }*/

        _camera.transform.localEulerAngles = new Vector3(-_rotationY, 0, 0);
        transform.localEulerAngles = new Vector3(0, _rotationX, 0);
    }

    private void SetCrouchCameraPosition()
    {
        if (Input.GetButtonDown(InputConstants.CROUCH))
        {
            _crouched = !_crouched;
        }

        if(_crouched)
        {
            FPSObject.transform.localPosition = Vector3.MoveTowards(FPSObject.transform.localPosition, 
                                                               CrouchPosition.transform.localPosition, 
                                                               CrouchMoveSpeed * Time.deltaTime);
        }
        else
        {
            FPSObject.transform.localPosition = Vector3.MoveTowards(FPSObject.transform.localPosition, 
                                                               HeadPosition.transform.localPosition, 
                                                               CrouchMoveSpeed * Time.deltaTime);
        }
    }
}
