using UnityEngine;

/// <summary>
/// 미니맵 카메라 컨트롤러
/// </summary>
public class MiniMapCamCtrl : MonoBehaviour
{
    [Header ( "----- 타겟 -----" )]
    [SerializeField] Transform _target;

    [SerializeField] private float zPosition = -10f;

    private void LateUpdate ( )
    {
        if ( _target == null )
            return;

        Vector3 pos = _target.position;
        pos.z = zPosition;

        transform.position = pos;
    }
}
