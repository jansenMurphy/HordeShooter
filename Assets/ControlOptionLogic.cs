using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Text;

public class ControlOptionLogic : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private InputActionReference inputAction;
    //[SerializeField] private PlayerController pc;
    [SerializeField] private int targetBinding =0;

    private void Start()
    {
        FixText();
    }

    public void StartRebinding()
    {
        text.text = "...";
        inputAction.action.PerformInteractiveRebinding()
            .WithControlsExcluding("<Keyboard>/escape")
            .WithControlsExcluding("<Keyboard>/anyKey")
            .WithTargetBinding(inputAction.action.GetBindingIndexForControl(inputAction.action.controls[targetBinding]))
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(x => RebindComplete(x))
            .Start();
    }

    /*
    private bool ResolveActionAndBinding(out int bindingIndex)
    {
        bindingIndex = -1;

        var bindingId = new Guid
    }
    */

    private void RebindComplete(InputActionRebindingExtensions.RebindingOperation x)
    {
        x.Dispose();
        x = null;
        FixText();
        //switch player controls back to Gameplay
    }

    public void FixText()
    {
        int bindingIndex = inputAction.action.GetBindingIndexForControl(inputAction.action.controls[targetBinding]);
        text.text = InputControlPath.ToHumanReadableString(inputAction.action.bindings[bindingIndex].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
    }
}
