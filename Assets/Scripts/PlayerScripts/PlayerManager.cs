using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using System.Threading.Tasks;

public class PlayerManager : MonoBehaviour
{
    public delegate void inputDelegate(CallbackContext context);
    public event inputDelegate moveDelegate, lookDelegate, fireDelegate, jumpDelegate, openCloseMenuDelegate;

    public FloatPlayerManagerGameEvent changeIntensityEvent;
    public float intensity
    {
        get { return intensity; }
        protected set
        {
            changeIntensityEvent.Raise((value, this));
            intensity = value;
        }
    }

    [SerializeField] Camera cam;
    private const byte FRUSTRUM_EXTEND_FOV = 5;

    private Plane[] extendedPlanes;

    public void GetMove(CallbackContext moveContext)
    {
        moveDelegate?.Invoke(moveContext);
    }
    public void GetLook(CallbackContext lookContext)
    {
        lookDelegate?.Invoke(lookContext);
    }
    public void GetFire(CallbackContext attackContext)
    {
        fireDelegate?.Invoke(attackContext);
    }
    public void GetJump(CallbackContext jumpContext)
    {
        jumpDelegate?.Invoke(jumpContext);
    }

    public void OpenCloseMenu(CallbackContext openCloseMenuContext)
    {
        openCloseMenuDelegate?.Invoke(openCloseMenuContext);
    }

    private void Start()
    {
        intensity = 0;
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(2000);
                intensity--;//TODO change intensity decline based on other stuff
            }
        });
    }

    private void Update()
    {
        extendedPlanes = GetSizedUpPlanes();
    }


    public bool IsRendered(Renderer rend)
    {
        return rend.isVisible;
    }

    public bool IsInBiggerFrustrum(Renderer rend)
    {
        return GeometryUtility.TestPlanesAABB(extendedPlanes, rend.bounds);
    }
    public bool IsInBiggerFrustrum(Collider col)
    {
        return GeometryUtility.TestPlanesAABB(extendedPlanes, col.bounds);
    }

    private Plane[] GetSizedUpPlanes() {
        float angle = cam.fieldOfView / 2 + FRUSTRUM_EXTEND_FOV;
        Plane[] planes = new Plane[5];
        planes[0] = new Plane(Quaternion.Euler(0, -angle, 0) * cam.transform.forward, cam.transform.position);
        planes[1] = new Plane(Quaternion.Euler(0, angle, 0) * cam.transform.forward, cam.transform.position);
        planes[2] = new Plane(Quaternion.Euler(0, 0, angle) * cam.transform.forward, cam.transform.position);
        planes[3] = new Plane(Quaternion.Euler(0, 0, angle) * cam.transform.forward, cam.transform.position);
        planes[4] = new Plane(-cam.transform.forward, cam.transform.position + cam.transform.forward * cam.farClipPlane);
        return planes;
    }

    public bool IsNearby(GameObject otherObject, float distance = 10f)
    {
        return Vector3.Distance(otherObject.transform.position, transform.position) <= distance;
    }
}
