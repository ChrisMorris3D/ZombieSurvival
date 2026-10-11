using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(WeaponController))]
public sealed class PlayerWeaponInput : MonoBehaviour
{
    WeaponController weaponController;
    InputAction fireAction;
    InputAction reloadAction;

    void Awake()
    {
        weaponController = GetComponent<WeaponController>();
        CreateActions();
    }

    void OnEnable()
    {
        fireAction.started += OnFireStarted;
        fireAction.canceled += OnFireCanceled;
        reloadAction.performed += OnReloadPerformed;

        fireAction.Enable();
        reloadAction.Enable();
    }

    void OnDisable()
    {
        fireAction.Disable();
        reloadAction.Disable();

        fireAction.started -= OnFireStarted;
        fireAction.canceled -= OnFireCanceled;
        reloadAction.performed -= OnReloadPerformed;

        weaponController.TriggerReleased();
    }

    void OnDestroy()
    {
        fireAction.Dispose();
        reloadAction.Dispose();
    }

    void CreateActions()
    {
        fireAction = new InputAction("Fire", InputActionType.Button);
        fireAction.AddBinding("<Mouse>/leftButton");
        fireAction.AddBinding("<Gamepad>/rightTrigger");

        reloadAction = new InputAction("Reload", InputActionType.Button);
        reloadAction.AddBinding("<Keyboard>/r");
        reloadAction.AddBinding("<Gamepad>/buttonWest");
    }

    void OnFireStarted(InputAction.CallbackContext context)
    {
        weaponController.TriggerPressed();
    }

    void OnFireCanceled(InputAction.CallbackContext context)
    {
        weaponController.TriggerReleased();
    }

    void OnReloadPerformed(InputAction.CallbackContext context)
    {
        weaponController.TryReload();
    }
}
