using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 카메라 제어 - 시네머신 추적 대상 변경
/// </summary>
public class CameraCtrl : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] CinemachineCamera _camera;      //시네머신 카메라
    [SerializeField] Transform _heroTransform;        //히어로 위치

    /// <summary>
    /// 카메라 추적 대상 설정
    /// </summary>
    /// <param name="target">추적 대상</param>
    public void SetFollowTarget ( Transform target )
    {
        //시네머신 카메라가 없으면 종료
        if ( _camera == null ) return;

        //추적 대상이 없으면 종료
        if ( target == null ) return;

        //추적 대상 설정
        _camera.Follow = target;
    }

    /// <summary>
    /// 히어로를 카메라 추적 대상으로 설정
    /// </summary>
    public void FollowHero ()
    {
        //히어로가 없으면 종료
        if ( _heroTransform == null ) return;

        //히어로 추적
        SetFollowTarget( _heroTransform );
    }
}
