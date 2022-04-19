using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PopAlerts : MonoBehaviour
{
    public AssetReference assetReference;
    RectTransform rt;
    [SerializeField] private const int ALERT_DECAY_DELAY = 5, ALERT_DECAY_TIME = 3;
    private void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    public void PopAlert(string alertText)
    {
        ObjectPools.Spawn(assetReference, (go) =>
        {
            go.GetComponent<AlertTextLogic>().DecayAfter(alertText, ALERT_DECAY_DELAY, ALERT_DECAY_TIME);
            go.transform.SetParent(transform);
            rt.anchoredPosition = new Vector2(0, rt.rect.height);
            return true;
        });
    }
}
