using UnityEngine;

public enum CharacterMode
{
    Courier,
    Bycicle
}

public class CharacterModeSwitcher : MonoBehaviour
{
    [SerializeField]
    Rigidbody bycicle;

    [SerializeField]
    Transform bycicleMeshParent;

    [SerializeField]
    Rigidbody courier;

    [SerializeField]
    Transform courierMeshParent;

    [SerializeField]
    Transform mesh;

    [SerializeField]
    string bycicleDissmisedAnimatorTrigger = "BycicleDissmised";

    CharacterMode currentMode = CharacterMode.Courier;

    void Awake()
    {
        mesh.parent = courierMeshParent;
        mesh.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        bycicle.gameObject.SetActive(false);
        courier.gameObject.SetActive(true);
        transform.parent = courier.transform;
    }

    public void SetCharacterMode(CharacterMode mode)
    {
        if (currentMode == mode)
        {
            return;
        }
        
        switch (mode)
        {
            case CharacterMode.Courier:
                courier.transform.SetPositionAndRotation(bycicle.position, Quaternion.Euler(0f, bycicle.rotation.eulerAngles.y, 0f));
                courier.linearVelocity = bycicle.linearVelocity;
                mesh.parent = courierMeshParent;
                mesh.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                bycicle.gameObject.SetActive(false);
                courier.gameObject.SetActive(true);
                mesh.GetComponent<Animator>().SetTrigger(bycicleDissmisedAnimatorTrigger);
                transform.parent = courier.transform;
            break;
            case CharacterMode.Bycicle:
                bycicle.transform.SetPositionAndRotation(courier.position, courier.rotation);
                bycicle.linearVelocity = courier.linearVelocity;
                mesh.parent = bycicleMeshParent;
                mesh.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                courier.gameObject.SetActive(false);
                bycicle.gameObject.SetActive(true);
                transform.parent = bycicle.transform;
            break;
            default:
            break;
        }

        currentMode = mode;
    }
}
