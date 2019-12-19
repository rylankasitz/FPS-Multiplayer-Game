using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WeaponScreenUI : NetworkBehaviour
{
    [HideInInspector]
    public int Money = 0;

    public float EconTime = .5f;

    public Canvas WeaponScreen;
    public Canvas HUD;

    public RectTransform OwnedWeaponsSlot;
    public RectTransform StoreWeaponsSlot;
    public RectTransform OwnedWeaponPrefab;
    public RectTransform StoreWeaponPrefab;

    public RectTransform Weapon1;
    public RectTransform Weapon2;
    public RectTransform ScopeSlot1;
    public RectTransform ScopeSlot2;
    public RectTransform AmmoSlot1;
    public RectTransform AmmonSlot2;

    public Text GunNameSlot1;
    public Text GunNameSlot2;

    public RectTransform BuyMenuPrefab;
    public RectTransform StoreItemPrefab;

    public List<WeaponStats> OwnedGuns = new List<WeaponStats>();
    public List<WeaponStats> StoreGuns = new List<WeaponStats>();

    public WeaponStats _Weapon1;
    public WeaponStats _Weapon2;

    private RectTransform _buyMenu;

    private bool _open;

    private CameraController _cameraController;
    private CharacterMovement _characterMovement;
    private CharacterWeapon _characterWeapon;

    private Image[] _Weapon1Images;
    private Image[] _Weapon2Images;

    private float _preTime;

    void Start()
    {
        if (!isLocalPlayer) return;

        _open = false;

        _cameraController = GetComponent<CameraController>();
        _characterMovement = GetComponent<CharacterMovement>();
        _characterWeapon = GetComponent<CharacterWeapon>();

        _Weapon1Images = Weapon1.GetComponentsInChildren<Image>();
        _Weapon2Images = Weapon2.GetComponentsInChildren<Image>();

        _preTime = Time.time;

        Init();
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetButtonDown(InputConstants.INVENTORY_SCREEN))
        {
            OpenCloseScreen();
            SetEquipedWeapons();
            SetOwnedGuns();
            SetStoreGuns();
        }

        UpdateEcon();
    }

    public void Init()
    {
        Debug.Log("Initializing Weapon Store...");

        Money = 100000;

        _Weapon1 = GunStats.None;
        _Weapon2 = GunStats.None;
        StoreGuns.Add(GunStats.Pistol);
        StoreGuns.Add(GunStats.Sniper);
        StoreGuns.Add(GunStats.AR);
        StoreGuns.Add(GunStats.MachineGun);
    }

    private void UpdateEcon()
    {
        if ((Time.time - _preTime) > EconTime)
        {
            Money += 1;
            Text money = GameObject.Find("Money").GetComponentInChildren<Text>();
            money.text = "$" + Money;
            _preTime = Time.time;
        }
    }

    public void SpawnBuyMenu(int weaponNum)
    {
        if (!isLocalPlayer) return;

        if (_buyMenu != null) Destroy(_buyMenu.gameObject);

        RectTransform scopeMenu = Instantiate(BuyMenuPrefab, WeaponScreen.transform);

        int count = 0;
        if (weaponNum == 0)
        {
            scopeMenu.anchoredPosition = new Vector3(-487, 133, 0);
            count = _Weapon1.Scopes.Count;
        }
        else
        {
            scopeMenu.anchoredPosition = new Vector3(-487, -362, 0);
            count = _Weapon2.Scopes.Count;
        }
           

        for (int i = 0; i < count; i++)
        {
            int scopeIndex = 0;
            RectTransform scopeItem = Instantiate(StoreItemPrefab, scopeMenu.transform);
            scopeItem.anchoredPosition = new Vector3(-39, 177.2f - ( i* 40), 0);

            if (weaponNum == 0)
            {
                scopeIndex = (int)_Weapon1.Scopes[i].Scope;
                if (_Weapon1.Scopes[i].bought)
                {
                    scopeItem.GetComponentsInChildren<Text>()[1].text = "EQUIP";
                }
                else
                {
                    scopeItem.GetComponentsInChildren<Text>()[1].text = "$" + _Weapon1.Price;
                }
            }
            else
            {
                scopeIndex = (int)_Weapon2.Scopes[i].Scope;
                if (_Weapon2.Scopes[i].bought)
                {
                    scopeItem.GetComponentsInChildren<Text>()[1].text = "EQUIP";
                }
                else
                {
                    scopeItem.GetComponentsInChildren<Text>()[1].text = "$" + _Weapon2.Price;
                }
            }

            scopeItem.GetComponentsInChildren<Text>()[0].text = GunStats.SCOPE_TEXTS[scopeIndex];

            int snum = scopeIndex;
            int wnum = weaponNum;
            int index = i;
            scopeItem.GetComponentInChildren<Button>().onClick.AddListener(() => OnBuyItem(wnum, snum, index));
        }

        _buyMenu = scopeMenu;
    }

    private void SetCharacterWeapons()
    {
        if (_characterWeapon.EquipedWeapon == 0)
        {
            _characterWeapon.CurrentWeapon = (int)_Weapon1.GunVal;
            _characterWeapon.SecondWeapon = (int)_Weapon2.GunVal;
            _characterWeapon.CmdSyncWeaponStates((int)_Weapon1.GunVal, (int)_Weapon2.GunVal);
            _characterWeapon.SightNumberCurrentWeapon = (int)_Weapon1.ScopeVal;
            _characterWeapon.SightNumberSecondWeapon = (int)_Weapon2.ScopeVal;
            _characterWeapon.WeaponCurrentMagSize = GunStats.MAG_SIZE[(int)_Weapon1.GunVal];
            _characterWeapon.WeaponSecondMagSize = GunStats.MAG_SIZE[(int)_Weapon2.GunVal];
        }
        else
        {
            _characterWeapon.CurrentWeapon = (int)_Weapon2.GunVal;
            _characterWeapon.SecondWeapon = (int)_Weapon1.GunVal;
            _characterWeapon.CmdSyncWeaponStates((int)_Weapon2.GunVal, (int)_Weapon1.GunVal);
            _characterWeapon.SightNumberCurrentWeapon = (int)_Weapon2.ScopeVal;
            _characterWeapon.SightNumberSecondWeapon = (int)_Weapon1.ScopeVal;
            _characterWeapon.WeaponCurrentMagSize = GunStats.MAG_SIZE[(int)_Weapon2.GunVal];
            _characterWeapon.WeaponSecondMagSize = GunStats.MAG_SIZE[(int)_Weapon1.GunVal];
        }

        _characterWeapon.WeaponCurrentBullets = _characterWeapon.WeaponCurrentMagSize;
        _characterWeapon.WeaponSecondBullets = _characterWeapon.WeaponSecondMagSize;

        _characterWeapon.SetWeapon();
    }

    private void OnBuyItem(int weaponNum, int scopeNum, int i)
    {
        Destroy(_buyMenu.gameObject);

        if (weaponNum == 0 && (Money - _Weapon1.Scopes[i].Price) >= 0)
        {
            ScopeSlot1.GetComponentInChildren<Text>().text = GunStats.SCOPE_TEXTS[scopeNum];
            _Weapon1.ScopeVal = _Weapon1.Scopes[i].Scope;
            if (!_Weapon1.Scopes[i].bought) Money -= _Weapon1.Scopes[i].Price;
            _Weapon1.Scopes[i].bought = true;
        }
        else if (weaponNum == 1 && (Money - _Weapon2.Scopes[i].Price) >= 0)
        {
            ScopeSlot2.GetComponentInChildren<Text>().text = GunStats.SCOPE_TEXTS[scopeNum];
            _Weapon2.ScopeVal = _Weapon2.Scopes[i].Scope;
            if (!_Weapon2.Scopes[i].bought) Money -= _Weapon2.Scopes[i].Price;
            _Weapon2.Scopes[i].bought = true;
        }

        Text money = GameObject.Find("Money").GetComponentInChildren<Text>();
        money.text = "$" + Money;

        SetCharacterWeapons();
    }

    private void EquipWeaponSlot1(WeaponStats weaponStats)
    {
        if (_Weapon1.GunVal != GunVal.None)
            OwnedGuns[OwnedGuns.IndexOf(weaponStats)] = _Weapon1;
        else
            OwnedGuns.Remove(weaponStats);

        _Weapon1 = weaponStats;

        SetEquipedWeapons();
        SetOwnedGuns();
        SetCharacterWeapons();
    }

    private void EquipWeaponSlot2(WeaponStats weaponStats)
    {
        if (_Weapon2.GunVal != GunVal.None)
            OwnedGuns[OwnedGuns.IndexOf(weaponStats)] = _Weapon2;
        else
            OwnedGuns.Remove(weaponStats);

        _Weapon2 = weaponStats;

        SetEquipedWeapons();
        SetOwnedGuns();
        SetCharacterWeapons();
    }

    private void BuyWeapon(WeaponStats weaponStats)
    {
        if ((Money - weaponStats.Price) >= 0)
        {
            OwnedGuns.Add(weaponStats);
            StoreGuns.Remove(weaponStats);
            SetOwnedGuns();
            SetStoreGuns();

            Money -= weaponStats.Price;

            Text money = GameObject.Find("Money").GetComponentInChildren<Text>();
            money.text = "$" + Money;
        }
    }

    private void OpenCloseScreen()
    {
        _open = !_open;

        WeaponScreen.enabled = _open;
        HUD.enabled = !_open;

        _characterMovement.enabled = !_open;
        _cameraController.enabled = !_open;
        _characterWeapon.enabled = !_open;

        Cursor.visible = _open;

        if (_open)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }      
    }

    private void SetEquipedWeapons()
    {
        for (int i = 0; i < _Weapon1Images.Length; i++)
        {

            _Weapon1Images[i].enabled = (i == ((int)_Weapon1.GunVal));
            _Weapon2Images[i].enabled = (i == ((int)_Weapon2.GunVal));
        }

        if (_Weapon1.GunVal != 0)
        {
            ScopeSlot1.parent.gameObject.SetActive(true);
            GunNameSlot1.transform.parent.gameObject.SetActive(true);
            ScopeSlot1.GetComponentInChildren<Text>().text = GunStats.SCOPE_TEXTS[(int)_Weapon1.ScopeVal];
            GunNameSlot1.text = GunStats.WEAPON_NAMES[(int)_Weapon1.GunVal];
        }
        else
        {
            ScopeSlot1.parent.gameObject.SetActive(false);
            GunNameSlot1.transform.parent.gameObject.SetActive(false);
        }

        if (_Weapon2.GunVal != 0)
        {
            ScopeSlot2.parent.gameObject.SetActive(true);
            GunNameSlot2.transform.parent.gameObject.SetActive(true);
            ScopeSlot2.GetComponentInChildren<Text>().text = GunStats.SCOPE_TEXTS[(int)_Weapon2.ScopeVal];
            GunNameSlot2.text = GunStats.WEAPON_NAMES[(int)_Weapon2.GunVal];
        }
        else
        {
            ScopeSlot2.parent.gameObject.SetActive(false);
            GunNameSlot2.transform.parent.gameObject.SetActive(false);
            }
    }

    private void SetOwnedGuns()
    {
        Image[] ownedWeapons = OwnedWeaponsSlot.GetComponentsInChildren<Image>();

        for(int i = 2; i < ownedWeapons.Length; i++)
        {
            Destroy(ownedWeapons[i].gameObject);
        }

        for(int i = 0; i < OwnedGuns.Count; i++)
        {
            RectTransform ownedGunUI = Instantiate(OwnedWeaponPrefab, OwnedWeaponsSlot);
            ownedGunUI.anchoredPosition = new Vector3(-68, 377 + -(i * 100), 0);

            WeaponStats weaponStats = OwnedGuns[i];

            ownedGunUI.GetComponentInChildren<Text>().text = GunStats.WEAPON_NAMES[(int)OwnedGuns[i].GunVal];

            ownedGunUI.GetComponentsInChildren<Button>()[0].onClick.AddListener(() => EquipWeaponSlot1(weaponStats));
            ownedGunUI.GetComponentsInChildren<Button>()[1].onClick.AddListener(() => EquipWeaponSlot2(weaponStats));
        }
    }

    private void SetStoreGuns()
    {
        Image[] storeWeapons = StoreWeaponsSlot.GetComponentsInChildren<Image>();

        for (int i = 2; i < storeWeapons.Length; i++)
        {
            Destroy(storeWeapons[i].gameObject);
        }

        for (int i = 0; i < StoreGuns.Count; i++)
        {
            RectTransform storeGunUI = Instantiate(StoreWeaponPrefab, StoreWeaponsSlot);
            storeGunUI.anchoredPosition = new Vector3(-68, 377 + -(i * 100), 0);

            WeaponStats weaponStats = StoreGuns[i];

            storeGunUI.GetComponentInChildren<Text>().text = GunStats.WEAPON_NAMES[(int)StoreGuns[i].GunVal];
            storeGunUI.GetComponentsInChildren<Text>()[1].text = "$" + weaponStats.Price;

            storeGunUI.GetComponentInChildren<Button>().onClick.AddListener(() => BuyWeapon(weaponStats));
        }
    }
}
