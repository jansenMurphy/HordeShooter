using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlertTextLogic : MonoBehaviour
{
    TextMesh txt;
    private void Start()
    {
        txt = GetComponentInChildren<TextMesh>();
    }
    public void DecayAfter(string text, int delaySeconds, int decayTime)
    {
        txt.text = text;
        IEnumerator decay = Decay(delaySeconds, decayTime);
        StartCoroutine(decay);
    }

    private IEnumerator Decay(int delaySeconds, int delayTime)
    {
        txt.color = Color.black;
        yield return new WaitForSeconds(delaySeconds);
        float time=0;
        while (time < delayTime)
        {
            txt.color = new Color(0, 0, 0, (1 - time / delayTime));
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
    }
}
