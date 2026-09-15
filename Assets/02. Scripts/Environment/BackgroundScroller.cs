using UnityEngine;

/// <summary>
/// 배경 스크롤(메터리얼 offset x 조절)
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] MeshRenderer _renderer;     //메쉬렌더러 컴포넌트
    [SerializeField] Transform _cameraTransform;     //카메라 트랜스폼 컴포넌트

    [Header( "----- 스크롤 -----" )]
    [SerializeField] float _scrollFactor;       //스크롤 계수

    private void LateUpdate ()
    {
        //메터리얼 offset 가져오기
        Vector2 offset = _renderer.material.GetTextureOffset( "_BaseMap" );

        //카메라의 x 좌표에 따라 offset x 값 결정
        //카메라가 자신 트랜스폼 로컬스케일 x 값만큼 이동하면 딱 한 바퀴 스크롤
        offset.x = _cameraTransform.position.x / transform.localScale.x;

        //스크롤 계수 적용
        offset.x *= _scrollFactor;

        //변경된 offset 값을 메터리얼에 적용
        _renderer.material.SetTextureOffset( "_BaseMap", offset );

        //카메라 따라오기
        TrackCamera( );
    }

    public void TrackCamera ()
    {
        //자기 위치 가져오기
        Vector3 pos = transform.position;

        //카메라 위치
        Vector3 camPos = _cameraTransform.position;

        pos.x = camPos.x;
        pos.y = camPos.y;

        transform.position = pos;
    }
}
