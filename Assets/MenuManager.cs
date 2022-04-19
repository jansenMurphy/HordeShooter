using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    private bool isMenuOpen = false, isPressing=false, hasOpenedMenu;
    [SerializeField] private GameObject menu;
    public void OpenCloseMenu(CallbackContext openMenuContext)
    {
        if (!(isPressing = openMenuContext.ReadValueAsButton()))
        {
            if (!hasOpenedMenu)
            {
                hasOpenedMenu = true;
                if (isMenuOpen)
                {
                    menu.SetActive(false);
                    playerInput.SwitchCurrentActionMap("Player");
                }
                else
                {
                    playerInput.SwitchCurrentActionMap("UI");
                    menu.SetActive(true);
                }
                isMenuOpen = !isMenuOpen;
            }
        }
        else
        {
            hasOpenedMenu = false;
        }
    }
}
