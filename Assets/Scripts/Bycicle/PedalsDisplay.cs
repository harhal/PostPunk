using UnityEngine;
using UnityEngine.UI;

public class ByciclePedalsDisplay : MonoBehaviour
{
    [SerializeField]
    private Image leftPedalIndicator;

    [SerializeField]
    private Image rightPedalIndicator;

    [SerializeField]
    private Image rotationIndicator;
    
    [SerializeField]
    private BycicleInputGear inputWheel;

    // Update is called once per frame
    void Update()
    {
        int leftPedalEfficiency = inputWheel.GetPedalEfficiency(PedalSide.Left);
        int rightPedalEfficiency = inputWheel.GetPedalEfficiency(PedalSide.Right);
        leftPedalIndicator.gameObject.SetActive(leftPedalEfficiency >= 3);
        rightPedalIndicator.gameObject.SetActive(rightPedalEfficiency >= 3);

        /*Gamepad.current.SetMotorSpeeds(
            leftPedalEfficiency >= 3 ? 0.5f : .0f, 
            rightPedalEfficiency >= 3 ? 0.5f : .0f);*/
        
        rotationIndicator.rectTransform.localPosition = Quaternion.Euler(.0f, .0f, inputWheel._getRotatedAngle()) * Vector3.up * 100.0f;
    }

    void OnDestroy()
    {
        //Gamepad.current.SetMotorSpeeds(.0f, .0f);
    }
}
