using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponAnimator : MonoBehaviour
{
    [SerializeField]
    Animator anim;
    const string DRAW_ANIM_TRIGGER = "draw", HOLSTER_ANIM_TRIGGER = "holster", PICK_UP_ANIM_TRIGGER = "pickUp", RELOAD_ANIM_TRIGGER = "reload", UNLOAD_ANIM_TRIGGER = "unload", SET_FIRE_MODE_ANIM_TRIGGER="setFireMode",
        FIRE1_ANIM_TRIGGER = "fire1", FIRE2_ANIM_TRIGGER = "fire2", FIRE3_ANIM_TRIGGER = "fire3", FIRE4_ANIM_TRIGGER = "fire4";
    [HideInInspector]
    public UnityEvent endHolsterEvent, endDrawEvent, endPickUpEvent, endReloadEvent, endUnloadEvent, endFireEvent, endSetFireModeEvent,
        partialHolsterEvent, partialDrawEvent, partialPickUpEvent, partialReloadEvent, partialUnloadEvent, partialFireEvent, partialSetFireModeEvent;

    public void SetAnimationTrigger(string animTrigger)
    {
        anim.SetTrigger(animTrigger);
    }

    public void Fire(int weapon, int module, int fireMode)
    {
        //Set some sort of trigger
        anim.SetTrigger(FIRE1_ANIM_TRIGGER);
    }

    public void PartialFire()
    {
        partialFireEvent.Invoke();
    }

    public void EndFire()
    {
        endFireEvent.Invoke();
    }

    public void Holster()
    {
        anim.SetTrigger(HOLSTER_ANIM_TRIGGER);
    }

    public void PartialHolster()
    {
        partialHolsterEvent.Invoke();
    }

    public void EndHolster()
    {
        endHolsterEvent.Invoke();
    }

    public void Draw()
    {
        anim.SetTrigger(DRAW_ANIM_TRIGGER);

    }

    public void PartialDrawEvent()
    {
        partialDrawEvent.Invoke();
    }

    public void EndDraw()
    {
        endDrawEvent.Invoke();
    }

    public void PickUp()
    {
        anim.SetTrigger(PICK_UP_ANIM_TRIGGER);
    }
    public void PartialPickUp()
    {
        partialPickUpEvent.Invoke();
    }

    public void EndPickUp()
    {
        endPickUpEvent.Invoke();
    }

    public void Reload()
    {
        anim.SetTrigger(RELOAD_ANIM_TRIGGER);
    }

    public void PartialReload()
    {
        partialReloadEvent.Invoke();
    }

    public void EndReload()
    {
        endReloadEvent.Invoke();
    }
    public void Unload()
    {
        anim.SetTrigger(UNLOAD_ANIM_TRIGGER);
    }

    public void PartialUnload()
    {
        partialUnloadEvent.Invoke();
    }

    public void EndUnload()
    {
        endUnloadEvent.Invoke();
    }

    public void SetFireMode()
    {
        anim.SetTrigger(SET_FIRE_MODE_ANIM_TRIGGER);
    }

    public void PartialSetFireMode()
    {
        partialUnloadEvent.Invoke();
    }

    public void EndSetFireMode()
    {
        endSetFireModeEvent.Invoke();
    }


    public string FindCorrectFireAnimation(int weapon, int module, int fireMode)
    {
        //TODO Figure out what is the right animation based on weapon ID, module, and firemode
        //Make the code something that can be edited in inspector
        return FIRE1_ANIM_TRIGGER;
    }

    public string FindCorrectReloadAnimation(int weapon, int module, int fireMode)
    {
        //TODO Figure out what is the right animation based on weapon ID, module, and firemode
        //Make the code something that can be edited in inspector
        return FIRE1_ANIM_TRIGGER;
    }

    public string FindCorrectPickUpAnimation(int weapon, int module, int fireMode)
    {
        //TODO Figure out what is the right animation based on weapon ID, module, and firemode
        //Make the code something that can be edited in inspector
        return FIRE1_ANIM_TRIGGER;
    }
    public string FindCorrectDrawAnimation(int weapon, int module, int fireMode)
    {
        //TODO Figure out what is the right animation based on weapon ID, module, and firemode
        //Make the code something that can be edited in inspector
        return FIRE1_ANIM_TRIGGER;
    }
    public string FindCorrectHolsterAnimation(int weapon, int module, int fireMode)
    {
        //TODO Figure out what is the right animation based on weapon ID, module, and firemode
        //Make the code something that can be edited in inspector
        return FIRE1_ANIM_TRIGGER;
    }
}
