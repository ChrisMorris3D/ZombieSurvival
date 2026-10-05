using UnityEngine;

public class Pistol : RaycastWeapon
{
    protected override bool IsFireInputActive()
    {
        return Input.GetMouseButtonDown(0);
    }
}
