using UnityEngine;

public class MachineGun : RaycastWeapon
{
    protected override bool IsFireInputActive()
    {
        return Input.GetMouseButton(0);
    }

    protected override void OnShotFired()
    {
        Debug.Log("Machine gun fired.", this);
    }
}
