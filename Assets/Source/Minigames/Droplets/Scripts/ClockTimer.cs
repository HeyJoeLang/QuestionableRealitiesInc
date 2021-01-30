using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClockTimer : MonoBehaviour
{
    public Image progressImage;
    public Transform clockHandTransform;
    public void UpdateProgress(float progress)
    {
        progressImage.fillAmount = 1 - progress;
        clockHandTransform.localRotation = Quaternion.Euler(0, 0, 360 * progress + 6f);
    }
}
