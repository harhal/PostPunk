using UnityEngine;
using UnityEngine.InputSystem;

public class InputSwitcher : MonoBehaviour
{
    [SerializeField]
    Courier courier;

    [SerializeField]
    PlayerInput courierInput;

    [SerializeField]
    Canvas courierUI;

    [SerializeField]
    BycicleEngine bycicle;

    [SerializeField]
    PlayerInput bycicleInput;

    [SerializeField]
    Canvas bycicleUI;

    void Update()
    {
        syncBehaviourState(courier, courierInput);
        syncBehaviourState(courier, courierUI);
        syncBehaviourState(bycicle, bycicleInput);
        syncBehaviourState(bycicle, bycicleUI);
    }

    void syncBehaviourState(Component master, Component slave)
    {
        if (slave == null)
        {
            return;
        }

        slave.gameObject.SetActive(master != null && master.gameObject.activeSelf);
    }
}
