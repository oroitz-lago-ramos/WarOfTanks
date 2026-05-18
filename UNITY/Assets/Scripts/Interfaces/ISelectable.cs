using UnityEngine;
using System;

public interface ISelectable
{
    void SetSelected(bool selected);
    bool IsEnemy();
    Vector3 GetWorldPosition();

    event Action<bool> OnSelected;
}
