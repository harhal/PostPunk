using UnityEngine;
using UnityEngine.InputSystem;

public class Cursor : MonoBehaviour
{
    [SerializeField]
    private GameObject cursor;

    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private City city;

    [SerializeField]
    private Address hitAddress;

    [SerializeField]
    private Route route;

    private int cursorShaderPropertyId;

    private void Start()
    {
        cursorShaderPropertyId = Shader.PropertyToID("_Cursor");
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }
    }

    // Update is called once per frame
    private void OnCursor(InputValue input)
    {
        Vector2 cursorLocation = input.Get<Vector2>();

        Ray coursorRay = playerInput.camera.ScreenPointToRay(new Vector3(cursorLocation.x, cursorLocation.y, 0));

        RaycastHit2D hit = Physics2D.Raycast(coursorRay.origin, coursorRay.direction, playerInput.camera.farClipPlane);
        Vector2 worldCursorLocation = hit.point;
        Shader.SetGlobalVector(cursorShaderPropertyId, worldCursorLocation);
        cursor.transform.position = worldCursorLocation;

        hitAddress = hit.rigidbody.GetComponent<Address>();
    }

    // Update is called once per frame
    private void OnSelect(InputValue input)
    {
        if (hitAddress != null)
        {
        }
    }
}
