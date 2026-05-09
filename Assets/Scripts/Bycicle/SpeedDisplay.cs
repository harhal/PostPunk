
using System;
using TMPro;
using UnityEngine;

public class BycicleSpeedDisplay : MonoBehaviour
{
    [SerializeField]
    public BycicleEngine engine;

    [SerializeField]
    public BycicleTransmission transmission;

    [SerializeField]
    public BycicleWheel inputWheel;

    [SerializeField]
    public TextMeshProUGUI inputSpeedDisplay;

    [SerializeField]
    public TextMeshProUGUI velocityDisplay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        velocityDisplay.SetText(String.Format("{0:F2} T{1} x{2:F2}", engine.DisplayVelocity.z / 1000f * 3600f, transmission.GetCurrentTransmission(), transmission.GetCurrentRatio()));
        inputSpeedDisplay.SetText(String.Format("{0:F2}", inputWheel.DisplayAngSpeed));
    }
}
