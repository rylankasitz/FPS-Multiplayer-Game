using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CharacterWeapon : NetworkBehaviour
{

    #region Public Variables

    #region Visble Variables

    public RawImage HitRegRenderer;
    public RawImage UnaimedSight;

    [SyncVar]
    public GameObject Bullet;

    [SerializeField]
    public Renderer[] WeaponObjects;

    public GameObject[] Weapons;
    public GameObject[] WeaponUIS;
    public GameObject[] Sights;

    public GameObject EquipedWeaponUISlot;
    public GameObject SecondWeaponUISlot;

    public float BulletSpeed = 100;
    public float HitRegUITime = 1f;

    public AudioClip[] FireClips;
    public AudioClip ReloadClip;

    #endregion

    #region Hidden Variables

    [HideInInspector]
    [SyncVar]
    public int CurrentWeapon;

    [HideInInspector]
    [SyncVar]
    public int SecondWeapon;

    [HideInInspector]
    [SyncVar]
    public bool WeaponEquiped = false;

    [HideInInspector]
    [SyncVar]
    public bool HitPlayer = false;

    [HideInInspector]
    [SyncVar]
    public bool FatalHit = false;

    [HideInInspector]
    [SyncVar]
    public float HitDamage = 0.0f;

    [HideInInspector]
    public GameObject WeaponEquipedUI;

    [HideInInspector]
    public GameObject SeconWeaponUI;

    [HideInInspector]
    public int SightNumberCurrentWeapon;

    [HideInInspector]
    public int SightNumberSecondWeapon;

    [HideInInspector]
    public int EquipedWeapon;

    [HideInInspector]
    public int WeaponCurrentMagSize;

    [HideInInspector]
    public int WeaponCurrentBullets;

    [HideInInspector]
    public int WeaponSecondMagSize;

    [HideInInspector]
    public int WeaponSecondBullets;

    #endregion

    #endregion

    #region Private Variables

    private Animator _animator;
    public Animator _weaponAnimator;

    private GameObject _weapon;
    private GameObject _sight;

    private Renderer[] _fpsRenders;

    private ParticleSystem _particleSystem;

    private Transform _cameraLocation;
    private Transform _shootLocation;

    private Vector3 _sightOffset;
    private Vector3 _weaponMoveVec;
    private Quaternion _weaponRotVec;

    private AudioSource _audioSources;

    private WeaponScreenUI _weaponScreenUI;

    private float _fireRate;
    private float _reloadTime;
    private float _previousTime;
    private float _preHitTime;

    private int _newWeapon;

    private bool _automatic;
    private bool _fireing;
    public bool _reloading;
    private bool _spawnBullet;

    private bool _weaponADSSet;

    #endregion

    #region Unity Methods

    void Start()
    {
        if (!isLocalPlayer)
        {
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
        }

        _animator = GetComponent<Animator>();
        _cameraLocation = GetComponentInChildren<Camera>().transform;
        _audioSources = GetComponent<AudioSource>();
        _weaponScreenUI = GetComponent<WeaponScreenUI>();

        _shootLocation = _cameraLocation;

        EquipedWeapon = 0;

        _fireing = false;
        _spawnBullet = false;
        _weaponADSSet = false;

        _previousTime = Time.time;
        _preHitTime = Time.time;

        _weaponRotVec = Quaternion.identity;
        _weaponMoveVec = Vector3.zero;

        foreach (Animator animator in GetComponentsInChildren<Animator>())
        {
            if (animator.tag == "Weapon")
                Destroy(animator.gameObject);
        }

        SetWeapon();
        SetMagSizes();

        if (CurrentWeapon != 0)
            CreateSight();

        Init();
    }

    void Update()
    {
        Debug.Log(SecondWeapon);
        if (isLocalPlayer)
        {
            if (CurrentWeapon != 0) Shoot();
            WeaponControls();
            UpdateBulletText();
            HitRegUI();
        }
        else
        {
            SetServerSideWeapon();
        }

        if (_spawnBullet)
        {
            CmdCreateBullet();
        }
    }

    #endregion

    #region Public Methods

    public void Init()
    {
        CurrentWeapon = 0;
        SecondWeapon = 0;
        SightNumberCurrentWeapon = 0;
        SightNumberSecondWeapon = 0;
    }

    [Command]
    public void CmdUpdateHitUI()
    {
        RpcUpdateHitUI(false);
    }

    [Command]
    public void CmdUpdateFatalHitUI()
    {
        RpcUpdateHitUI(true);
    }

    #endregion

    #region Private Methods

    #region Weapon Methods

    private void SetServerSideWeapon()
    {
        if (CurrentWeapon != 0)
        {
            if (WeaponEquiped)
            {
                WeaponObjects[CurrentWeapon - 1].enabled = (true);
            }
            else
            {
                WeaponObjects[CurrentWeapon - 1].enabled = (false);
            }
        }

        if (SecondWeapon != 0)
            WeaponObjects[SecondWeapon - 1].enabled = (false);
    }

    private void WeaponControls()
    {
        if (Input.GetAxis(InputConstants.SWITCH_WEAPON) != 0)
        {
            if (EquipedWeapon == 0)
            {
                CmdSetWeaponEquipedTrue();
                SwitchWeapon(SecondWeapon);
                if (_sight == null && _weapon != null && SightNumberCurrentWeapon != 0 && CurrentWeapon != 0)
                {
                    CreateSight();
                }
                EquipedWeapon = 1;
            }
            else
            {
                CmdSetWeaponEquipedTrue();
                SwitchWeapon(SecondWeapon);
                if (_sight == null && _weapon != null && SightNumberCurrentWeapon != 0 && CurrentWeapon != 0)
                {
                    CreateSight();
                }
                EquipedWeapon = 0;
            }
        }

        if (Input.GetButtonDown(InputConstants.WEAPON1))
        {
            if (EquipedWeapon == 0)
            {
                CmdSetWeaponEquipedToggle();
            }
            else
            {
                CmdSetWeaponEquipedTrue();
                SwitchWeapon(SecondWeapon);
                if (_sight == null && _weapon != null && SightNumberCurrentWeapon != 0 && CurrentWeapon != 0)
                {
                    CreateSight();
                }
                EquipedWeapon = 0;
            }
        }
        else if (Input.GetButtonDown(InputConstants.WEAPON2))
        {
            if (EquipedWeapon == 1)
            {
                CmdSetWeaponEquipedToggle();
            }
            else
            {
                CmdSetWeaponEquipedTrue();
                SwitchWeapon(SecondWeapon);
                if (_sight == null && _weapon != null && SightNumberCurrentWeapon != 0 && CurrentWeapon != 0)
                {
                    CreateSight();
                }
                EquipedWeapon = 1;
            }
        }

        if (_sight == null && _weapon != null && SightNumberCurrentWeapon != 0 && CurrentWeapon != 0)
        {
            CreateSight();
        }

        if (CurrentWeapon != 0)
        {
            if (WeaponEquiped)
            {
                _weapon.SetActive(true);
                _animator.SetBool(AnimatorContants.IS_WEAPON_EQUIPED, true);
                SetFirReloadeRate(CurrentWeapon);
                if (_sight != null && _weapon != null && SightNumberCurrentWeapon != 0 && CurrentWeapon != 0)
                {               
                    AimDownSights(Input.GetButton(InputConstants.AIM));                
                }
                    
            }
            else
            {
                _weapon.SetActive(false);
                _animator.SetBool(AnimatorContants.IS_WEAPON_EQUIPED, false);
            }
        }
    }

    public void SetWeapon()
    {
        if (_weapon != null) Destroy(_weapon);
        if (WeaponEquipedUI != null) Destroy(WeaponEquipedUI);
        if (SeconWeaponUI != null) Destroy(SeconWeaponUI);

        if (CurrentWeapon != 0)
        {
            Transform cameraPosition = _cameraLocation;
            _weapon = Instantiate(Weapons[CurrentWeapon - 1], cameraPosition);
            _weapon.transform.parent = cameraPosition;
            _weaponAnimator = _weapon.GetComponent<Animator>();
            _particleSystem = _weapon.GetComponentInChildren<ParticleSystem>();
            _automatic = GunStats.AUTOMATIC[CurrentWeapon];
            _animator.SetInteger(AnimatorContants.WEAPON_TYPE, CurrentWeapon - 1);
        }

        WeaponEquipedUI = Instantiate(WeaponUIS[CurrentWeapon], EquipedWeaponUISlot.transform.position,
            EquipedWeaponUISlot.transform.rotation);
        WeaponEquipedUI.transform.SetParent(EquipedWeaponUISlot.transform);

        if (CurrentWeapon != 0)
            WeaponEquipedUI.GetComponentInChildren<Image>().color = Color.white;
        else
            WeaponEquipedUI.GetComponentInChildren<Text>().color = Color.white;

        SeconWeaponUI = Instantiate(WeaponUIS[SecondWeapon], SecondWeaponUISlot.transform.position,
            SecondWeaponUISlot.transform.rotation);
        SeconWeaponUI.transform.SetParent(EquipedWeaponUISlot.transform);

        if (SecondWeapon != 0)
            SeconWeaponUI.GetComponentInChildren<Image>().color = Color.gray;
        else
            SeconWeaponUI.GetComponentInChildren<Text>().color = Color.gray;

        DisableRenders();
    }

    public void SwitchWeapon(int newWeapon)
    {
        SecondWeapon = CurrentWeapon;
        CurrentWeapon = newWeapon;

        CmdSyncWeaponStates(CurrentWeapon, SecondWeapon);

        SwitchValues<int>(WeaponCurrentBullets, WeaponSecondBullets,
            out WeaponCurrentBullets, out WeaponSecondBullets);

        SwitchValues<int>(WeaponCurrentMagSize, WeaponSecondMagSize,
            out WeaponCurrentMagSize, out WeaponSecondMagSize);

        SwitchValues<int>(SightNumberCurrentWeapon, SightNumberSecondWeapon,
            out SightNumberCurrentWeapon, out SightNumberSecondWeapon);

        SetWeapon();
    }

    private void AimDownSights(bool state)
    {
        GameObject SightPos = GameObject.Find("SightPos");

        if (!_reloading)
        {
            if (state)
            {
                _sight.transform.rotation = _cameraLocation.rotation;
                _sight.transform.position = _cameraLocation.position + _sightOffset;
                if (CurrentWeapon != 4 && SightNumberCurrentWeapon >= 4)
                    GameObject.Find("WeaponsModels").GetComponentsInChildren<Renderer>()[CurrentWeapon - 1].enabled = state;
            }
            else
            {
                _sight.transform.position = SightPos.transform.position;
                _sight.transform.rotation = SightPos.transform.rotation;

                if (CurrentWeapon != 4 && SightNumberCurrentWeapon >= 4)
                {
                    foreach (Renderer renderer in GameObject.Find("WeaponsModels").GetComponentsInChildren<Renderer>())
                    {
                        renderer.enabled = false;
                    }
                }
            }

            foreach (Image image in _weapon.GetComponentsInChildren<Image>())
            {
                image.enabled = state;
            }

            if (_weapon != null)
            {
    
                _cameraLocation.GetComponent<Camera>().enabled = !state;
                if (_weapon.GetComponentInChildren<Camera>() != null)
                    _weapon.GetComponentInChildren<Camera>().enabled = state;
                _weapon.GetComponentsInChildren<Renderer>()[0].enabled = !state;
                _weapon.GetComponentsInChildren<Renderer>()[1].enabled = !state;

                if (state)
                {
                    _shootLocation = _weapon.GetComponentInChildren<Camera>().transform;
                    GetComponent<CameraController>().SensitivityX = GetComponent<CameraController>().SenXConst
                        *_weapon.GetComponentInChildren<Camera>().fieldOfView / 60;
                    GetComponent<CameraController>().SensitivityY = GetComponent<CameraController>().SenYConst
                        * _weapon.GetComponentInChildren<Camera>().fieldOfView / 60;
                }
                else
                {
                    _shootLocation = _cameraLocation;
                    GetComponent<CameraController>().SensitivityX = GetComponent<CameraController>().SenXConst;
                    GetComponent<CameraController>().SensitivityY = GetComponent<CameraController>().SenYConst;
                }
            }

            UnaimedSight.enabled = !state;
        }
        else
        {
            _sight.transform.position = SightPos.transform.position;
            _sight.transform.rotation = SightPos.transform.rotation;

            if (CurrentWeapon != 4 && SightNumberCurrentWeapon >= 4)
            {
                foreach (Renderer renderer in GameObject.Find("WeaponsModels").GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = false;
                }
            }

            foreach (Image image in _weapon.GetComponentsInChildren<Image>())
            {
                image.enabled = false;
            }

            if (_weapon.GetComponentInChildren<Camera>() != null)
                _weapon.GetComponentInChildren<Camera>().enabled = false;
             _cameraLocation.GetComponent<Camera>().enabled = true;
            _weapon.GetComponentsInChildren<Renderer>()[0].enabled = true;
            _weapon.GetComponentsInChildren<Renderer>()[1].enabled = true;

            GetComponent<CameraController>().SensitivityX = GetComponent<CameraController>().SenXConst;
            GetComponent<CameraController>().SensitivityY = GetComponent<CameraController>().SenYConst;

            UnaimedSight.enabled = true;
            _shootLocation = _cameraLocation;
        }
    }

    private void CreateSight()
    {
        GameObject SightPos = GameObject.Find("SightPos");

        _sight = Instantiate(Sights[SightNumberCurrentWeapon], SightPos.transform.position, SightPos.transform.rotation);
        _sight.transform.parent = SightPos.transform;

        _sightOffset = _sight.GetComponentInChildren<Camera>().transform.localPosition;

        if (CurrentWeapon == (int)GunVal.Sniper)
        {
            foreach (Renderer r in _sight.GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }
        }
    }

    private void DisableRenders()
    {
        if (!isLocalPlayer)
        {
            Renderer[] renders = _cameraLocation.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renders)
            {
                renderer.enabled = false;
            }
        }
    } 

    [Command]
    private void CmdSetWeaponEquipedToggle()
    {
        WeaponEquiped = !WeaponEquiped;
    }

    [Command]
    private void CmdSetWeaponEquipedTrue()
    {
        WeaponEquiped = true;
    }

    [Command]
    public void CmdSyncWeaponStates(int cw, int ss)
    {
        CurrentWeapon = cw;
        SecondWeapon = ss;
    }

    #endregion

    #region Fireing Methods

    private void Shoot()
    {
        if (_automatic)
        {
            _fireing = Input.GetButton(InputConstants.FIRE);
        }
        else
        {
            _fireing = Input.GetButtonDown(InputConstants.FIRE);
        }

        if (_fireing && !_reloading && (Time.time - _previousTime) >= _fireRate && WeaponEquiped)
        {
            SetShootAnimator(true);
            CmdCreateBullet();
            CmdSendFireAudio();
            _previousTime = Time.time;
            WeaponCurrentBullets--;
            _particleSystem.Play();

            if (WeaponCurrentBullets == 0)
            {
                Reload();
            }
        }
        else if (_automatic && Input.GetButtonUp(InputConstants.FIRE))
        {
            SetShootAnimator(false);
        }

        if (_reloading && (Time.time - _previousTime) >= _reloadTime)
        {
            _reloading = false;
            _previousTime = 0;
        }

        if (Input.GetButtonDown(InputConstants.RELOAD))
        {
            Reload();
        }
    }

    private void Reload()
    {
        WeaponCurrentBullets = WeaponCurrentMagSize;
        _weaponAnimator.SetTrigger(AnimatorContants.RELOAD);
        _reloading = true;
        _previousTime = Time.time;
        _animator.SetTrigger(AnimatorContants.RELOAD);
        SetShootAnimator(false);
    }
    
    [Command]
    private void CmdCreateBullet()
    {
        Vector3 rot = _shootLocation.position;
        if (SightNumberCurrentWeapon == 0 || !Input.GetButton(InputConstants.AIM))
        {
            _shootLocation.localPosition = new Vector3(
                _shootLocation.localPosition.x + Random.Range(-100, 100) * .002f,
                _shootLocation.localPosition.y + Random.Range(-100, 100) * .002f,
                _shootLocation.localPosition.z);
        }

        GameObject bullet = Instantiate(Bullet, _shootLocation.position, _shootLocation.rotation);
        bullet.GetComponent<Rigidbody>().velocity = BulletSpeed * _shootLocation.forward;
        _shootLocation.position = rot;
        bullet.GetComponent<Bullet>().SetID(transform.GetComponent<Health>().PlayerID);
        bullet.GetComponent<Bullet>().SentPlayer = GetComponent<CharacterWeapon>();
        bullet.GetComponent<Bullet>().Damage = GunStats.BULLET_DAMAGE[CurrentWeapon];

        Destroy(bullet, bullet.GetComponent<Bullet>().DespawnTime);

        if(isServer)
        {
            NetworkServer.Spawn(bullet);
        }
        else if(isClient)
        {
            NetworkServer.SpawnWithClientAuthority(bullet, connectionToClient);
        }

        _spawnBullet = false;
    }

    private void SetShootAnimator(bool state)
    {
        if (_automatic)
        {
            _weaponAnimator.SetBool(AnimatorContants.SHOOT, state);
        }
        else
        {
            _weaponAnimator.SetTrigger(AnimatorContants.SHOOT);
        }
    }

    private void SetFirReloadeRate(int weapon)
    {
        _fireRate = GunStats.FIRE_RATE[weapon];
        _reloadTime = GunStats.RELOAD_TIME[weapon];
    }

    private void SetMagSizes()
    {
        WeaponCurrentMagSize = GunStats.MAG_SIZE[CurrentWeapon];
        WeaponCurrentBullets = GunStats.MAG_SIZE[CurrentWeapon];
        WeaponSecondMagSize = GunStats.MAG_SIZE[SecondWeapon];
        WeaponSecondBullets = GunStats.MAG_SIZE[SecondWeapon];
    }

    #endregion

    #region UI Methods

    private void UpdateBulletText()
    {
        if (CurrentWeapon != 0)
        {
            WeaponEquipedUI.GetComponentsInChildren<Text>()[1].text = WeaponCurrentBullets.ToString();
            WeaponEquipedUI.GetComponentsInChildren<Text>()[0].text = WeaponCurrentMagSize.ToString();
        }

        if (SecondWeapon != 0)
        {
            SeconWeaponUI.GetComponentsInChildren<Text>()[1].text = WeaponSecondBullets.ToString();
            SeconWeaponUI.GetComponentsInChildren<Text>()[0].text = WeaponSecondMagSize.ToString();
        }

        WeaponEquipedUI.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        SeconWeaponUI.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    }

    [ClientRpc]
    private void RpcUpdateHitUI(bool fatal)
    {
        HitPlayer = true;
        FatalHit = fatal;
    }

    private void HitRegUI()
    {
        if (HitPlayer)
        {
            if(FatalHit)
            {
                HitRegRenderer.enabled = true;
                FatalHit = false;
            }
            
            HitRegRenderer.GetComponentInChildren<Text>().text = "-" + HitDamage;
            HitRegRenderer.GetComponentInChildren<Text>().GetComponent<Animation>().Rewind();
            HitRegRenderer.GetComponentInChildren<Text>().GetComponent<Animation>().Play();

            _preHitTime = Time.time;
            HitPlayer = false;
        }
        else if (HitRegUITime <= (Time.time - _preHitTime))
        {
            HitRegRenderer.enabled = false;
            HitRegRenderer.GetComponentInChildren<Text>().text = "";
        }
    }

    #endregion

    #region AudioRegions

    [Command]
    private void CmdSendFireAudio()
    {
        if (CurrentWeapon != 0)
            PlayAudioClip(FireClips[CurrentWeapon - 1]);
    }

    private void PlayAudioClip(AudioClip clip)
    {
        _audioSources.clip = clip;
        _audioSources.Play();
    }

    #endregion

    #region Misc

    private void SwitchValues<T>(T a, T b, out T c, out T d)
    {
        d = a;
        c = b;
    }

    #endregion

    #endregion
}
