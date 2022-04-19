using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveLoadControls : MonoBehaviour
{
    private bool dirty = false;
    public PlayerInput playerInput;

    private void Awake()
    {
        Load();
    }

    private void Save()
    {
        Debug.Log("UAHGF");
        dirty = false;
        string bindings = playerInput.actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("bindings", bindings);
    }

    private void Load()
    {
        string bindings = PlayerPrefs.GetString("bindings", string.Empty);
        if (string.IsNullOrEmpty(bindings))
        {
            Debug.Log("no bindings");
            return;
        }
        playerInput.actions.LoadBindingOverridesFromJson(bindings);
    }

    public void SetToDefaults()
    {
        playerInput.actions.RemoveAllBindingOverrides();
    }

    public void MarkDirtyControls()
    {
        dirty = true;
    }

    private void OnDisable()
    {
        if (dirty) Save();//TODO Replace this with a menu
    }
}
