using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CharacterMovement : NetworkBehaviour
{
    public Renderer[] CharacterRenderers;
    public float MoveSpeed = 6.0f;
    public float LeftRightSpeed = 6.0f;
    [Range(0.0f, 1.0f)]
    public float CrouchSpeed = .5f;
    public float JumpForce = 8.0f;
    public float Gravity = 20.0f;
    public AudioClip WalkingAudio;

    private Animator _animator;
    private CharacterController _characterController;
    private Vector3 _moveDirection;
    private float _orgHeight;
    private float _orgY;
    private AudioSource _audioSource;
    private Vector3 _impact;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponents<AudioSource>()[1];

        _animator.applyRootMotion = false;
        _moveDirection = Vector3.zero;
        _orgHeight = _characterController.height;
        _orgY = _characterController.center.y;
        _impact = Vector3.zero;

        if (isLocalPlayer)
        {
            foreach (Renderer renderer in CharacterRenderers)
            {
                renderer.enabled = false;
            }
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        SetAnimator();

        MoveImpact();

        MoveCharacterController();

        PlayAudio();
    }

    public void AddImpact(Vector3 dir, float force)
    {
        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y;
        _impact += dir.normalized * force;
    }

    private void MoveImpact()
    {
        if (_impact.magnitude > 0.2) _characterController.Move(_impact * Time.deltaTime);
        _impact = Vector3.Lerp(_impact, Vector3.zero, 5 * Time.deltaTime);
    }

    private void SetAnimator()
    {
        float fowardAxis = Input.GetAxis(InputConstants.FOWARD_MOVMENT);
        float horizontalAxis = Input.GetAxis(InputConstants.LEFT_RIGHT_MOVEMENT);

        _animator.SetFloat(AnimatorContants.SPEED, Input.GetAxis(InputConstants.FOWARD_MOVMENT));
        _animator.SetFloat(AnimatorContants.SIDE_SPEED, Input.GetAxis(InputConstants.LEFT_RIGHT_MOVEMENT));      

        if(Input.GetButtonDown(InputConstants.CROUCH))
        {
            _animator.SetBool(AnimatorContants.IS_CROUCHED, !_animator.GetBool(AnimatorContants.IS_CROUCHED));                 
        }
        
        if (Input.GetButtonDown(InputConstants.JUMP) && _characterController.isGrounded)
        {
            _animator.SetTrigger(AnimatorContants.JUMP);
        }
    }

    private void MoveCharacterController()
    {
        if (_characterController.isGrounded)
        {
            float crouchMultiplier = 1.0f;

            _animator.SetTrigger(AnimatorContants.IS_GROUNDED);
            
            if(_animator.GetBool(AnimatorContants.IS_CROUCHED))
            {
                crouchMultiplier = CrouchSpeed;
                _characterController.height = _orgHeight / 2;
                _characterController.center = new Vector3(0, _orgY / 2, 0);
            }
            else
            {
                _characterController.height = _orgHeight;
                _characterController.center = new Vector3(0, _orgY, 0);
            }

            _moveDirection = Vector3.zero;
            _moveDirection += transform.TransformDirection(Vector3.forward) 
                * _animator.GetFloat(AnimatorContants.SPEED) * MoveSpeed * crouchMultiplier;
            _moveDirection += transform.TransformDirection(Vector3.left)
                * _animator.GetFloat(AnimatorContants.SIDE_SPEED) * LeftRightSpeed * crouchMultiplier;  

            if(Input.GetButtonDown(InputConstants.JUMP))
            {
                _moveDirection.y = JumpForce;
            }
            
        }
        _moveDirection.y -= Gravity * Time.deltaTime;
        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    private void PlayAudio()
    {
        if (Input.GetButtonDown(InputConstants.FOWARD_MOVMENT) || Input.GetButtonDown(InputConstants.LEFT_RIGHT_MOVEMENT))
        {
            _audioSource.clip = WalkingAudio;
            _audioSource.volume = .3f;
            _audioSource.loop = true;
            _audioSource.Play();
        }
        else if (!Input.GetButton(InputConstants.FOWARD_MOVMENT) && !Input.GetButton(InputConstants.LEFT_RIGHT_MOVEMENT))
        {
            _audioSource.Stop();
        }
    }
}
