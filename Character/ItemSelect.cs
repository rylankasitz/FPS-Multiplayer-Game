using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static PiUI;
using static PiUIManager;

public class ItemSelect : NetworkBehaviour
{

    public Image ItemIcon;

    [Serializable]
    public struct Item
    {
        public string name;
        public GameObject prefab;
    }

    public Item[] Items;

    private PiUI _inventoryMenu;
    private CharacterMovement _characterMovement;
    private CharacterWeapon _characterWeapon;
    private CameraController _cameraController;
    private GameObject _currentItem;
    private GameObject _spawnedItem;

    void Start()
    {
        if (!isLocalPlayer) return;

        _inventoryMenu = GetComponentInChildren<PiUI>();
        _characterMovement = GetComponent<CharacterMovement>();
        _characterWeapon = GetComponent<CharacterWeapon>();
        _cameraController = GetComponent<CameraController>();
        SetItem(1);

        _inventoryMenu.dynamicallyScaleToResolution = true;
    }

    
    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetButtonDown(InputConstants.ITEM_SCREEN))
        {
            _inventoryMenu.OpenMenu(new Vector2(825, 460));
            _characterMovement.enabled = false;
            _characterWeapon.enabled = false;
            _cameraController.enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            foreach (PiData data in _inventoryMenu.piData)
            {
                data.highlightedColor = Color.cyan;
                data.nonHighlightedColor = Color.white;
            }
        }
        else if (Input.GetButtonUp(InputConstants.ITEM_SCREEN))
        {
            _inventoryMenu.CloseMenu();
            _characterMovement.enabled = true;
            _characterWeapon.enabled = true;
            _cameraController.enabled = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        Spawn();
    }

    public void SetItem(int index)
    {
        index--;
        ItemIcon.sprite = _inventoryMenu.piData[index].icon;
        ItemIcon.GetComponentInChildren<Text>().text = _inventoryMenu.piData[index].sliceLabel;
        _inventoryMenu.CloseMenu();
        _currentItem = findItem(_inventoryMenu.piData[index].sliceLabel);
    }

    private void Spawn()
    {
        if (Input.GetButtonDown(InputConstants.SPAWN_ITEM))
        {
            _spawnedItem = Instantiate(_currentItem, transform);
            _spawnedItem.GetComponent<Rigidbody>().useGravity = false;
            _spawnedItem.GetComponent<Collider>().enabled = false;
        }
        else if (Input.GetButtonUp(InputConstants.SPAWN_ITEM))
        {
            _spawnedItem.GetComponent<Rigidbody>().useGravity = true;
            _spawnedItem.transform.SetParent(null);
            _spawnedItem.GetComponent<Collider>().enabled = true;

            if (isServer)
            {
                NetworkServer.Spawn(_spawnedItem);
            }
            else if (isClient)
            {
                NetworkServer.SpawnWithClientAuthority(_spawnedItem, connectionToClient);
            }
        }      
    }

    private GameObject findItem(string name)
    {
        foreach(Item item in Items)
        {
            if (item.name == name)
                return item.prefab;
        }
        return null;
    }
}
