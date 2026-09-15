using UnityEngine;
using System;
using System.Collections.Generic;


/// <summary>
/// 아이템 상호작용 - 들기, 놓기, 던지기, 제출
/// </summary>
public class Interacter : MonoBehaviour
{
    [Header( "----- 설정 데이터 -----" )]
    [SerializeField] float _itemCheckRadius;        //타겟 체크 반지름
    [SerializeField] LayerMask _itemLayerMask;     //아이템 레이어마스크
    [SerializeField] float _goalCheckRadius;        //목표 체크 반지름
    [SerializeField] LayerMask _goalLayerMask;      //목표 레이어마스크

    [Header( "--- 상호작용 ---" )]
    [SerializeField] Transform _holdPoint;        //아이템을 들고 있는 위치
    [SerializeField] Item _holdItem;        //들고 있는 아이템

    [SerializeField] float _pullDist;       //당기기 거리
    [SerializeField] float _pullCheckRadius;        //당기기 체크 반지름
    [SerializeField] LayerMask _pullObstacleLayerMask;      //당기기 장애물 체크 레이어마스크

    [Header( "--- 내려놓기 ---" )]
    [SerializeField] float _putDownGroundCheckDist;      //내려놓기 지면 탐색 거리
    [SerializeField] LayerMask _putDownGroundLayerMask;      //내려놓기 지면 레이어마스크


    /// <summary>
    /// 아이템을 들고 있는지 여부
    /// </summary>
    public bool HasHoldItem => _holdItem != null;

    #region ----- 이벤트 -----
    /// <summary>
    /// 인벤토리 아이템 제출 이벤트
    /// </summary>
    public event Action<Dictionary<ItemData, int>> OnInventoryItemsSubmitted;
    /// <summary>
    /// 상호작용 아이템 제출 이벤트
    /// </summary>
    public event Action<Item> OnInteractItemSubmitted;

    /// <summary>
    /// 아이템 들기 이벤트
    /// </summary>
    public event Action OnHoldItem;
    /// <summary>
    /// 아이템 내려놓기 이벤트
    /// </summary>
    public event Action OnPutDownItem;
    /// <summary>
    /// 아이템 던지기 이벤트
    /// </summary>
    public event Action OnThrownItem;
    /// <summary>
    /// 아이템 당기기 이벤트
    /// </summary>
    public event Action OnPulledItem;
    /// <summary>
    /// 들고 있는 아이템 변경 이벤트
    /// </summary>
    public event Action<Item> OnHoldItemChanged;
    /// <summary>
    /// 들고 있는 아이템 해제 이벤트
    /// </summary>
    public event Action OnHoldItemCleared;
    #endregion

    /// <summary>
    /// 인벤토리 아이템 제출
    /// </summary>
    /// <returns>제출 성공 여부</returns>
    public bool SubmitInventoryItem ( Dictionary<ItemData, int> inventory )
    {
        //Goal이 영역 안에 없으면 종료
        if ( IsGoalInRange( ) == false ) return false;

        //낼 게 없으면 종료
        if ( inventory.Count == 0 ) return false;

        //인벤토리 아이템 제출 이벤트 발행
        OnInventoryItemsSubmitted?.Invoke( inventory );

        return true;
    }

    /// <summary>
    /// 상호작용 아이템 제출
    /// </summary>
    /// <returns>제출 성공 여부</returns>
    public bool SubmitInteractItem ()
    {
        //Goal이 영역 안에 없으면 종료
        if ( IsGoalInRange( ) == false ) return false;

        //낼 게 없으면 종료
        if ( _holdItem == null ) return false;

        //상호작용 아이템 제출 이벤트 발행
        OnInteractItemSubmitted?.Invoke( _holdItem );

        //아이템 처리
        _holdItem = null;

        //들고 있는 아이템 해제 이벤트 발행
        OnHoldItemCleared?.Invoke( );

        return true;
    }

    /// <summary>
    /// Goal이 영역 안에 있는지 확인
    /// </summary>
    /// <returns>Goal 영역 안에 있는지 여부</returns>
    public bool IsGoalInRange ()
    {
        //목표가 영역 안에 있는지 확인
        Collider2D goal = Physics2D.OverlapCircle( transform.position, _goalCheckRadius, _goalLayerMask );

        //목표가 없다면 종료
        if ( goal == null ) return false;

        return true;
    }

    /// <summary>
    /// 근처 아이템 중 가장 가까이에 있는 아이템 찾기
    /// </summary>
    /// <returns>감지된 아이템</returns>
    public Item FindItem ()
    {
        //아이템 찾기
        Collider2D [ ] items = Physics2D.OverlapCircleAll( transform.position, _itemCheckRadius, _itemLayerMask );

        //없으면 종료
        if ( items.Length == 0 ) return null;

        //아이템과의 거리 계산
        Item nearestItem = null;
        float nearestDist = float.MaxValue;

        foreach ( var itemCollider in items )
        {
            //아이템 컴포넌트 가져오기
            Item item = itemCollider.GetComponent<Item>( );
            //아이템이 없으면 종료
            if ( item == null ) continue;

            //거리 구하기
            float dist = Vector2.Distance( transform.position, item.transform.position );

            //dist가 nearestDist보다 작다면
            if ( dist < nearestDist )
            {
                nearestDist = dist;
                nearestItem = item;
            }
        }

        //가까이에 있는 아이템 반환
        return nearestItem;
    }

    /// <summary>
    /// 아이템을 든다
    /// </summary>
    /// <param name="item">들 아이템</param>
    /// <returns>성공 여부</returns>
    public bool HoldItem ( Item item )
    {
        //이미 들고 있는 아이템이 있으면 종료
        if ( _holdItem != null ) return false;

        //상호작용 아이템이 아니면 종료
        if ( item is InteractableItem holdItem == false ) return false;

        //현재 들고 있는 아이템 할당
        _holdItem = item;

        //아이템 들림 상태 처리
        holdItem.Hold( _holdPoint );

        //들고 있는 아이템 변경 이벤트 발행
        OnHoldItemChanged?.Invoke( _holdItem );

        //아이템 들기 이벤트 발행
        OnHoldItem?.Invoke( );

        return true;
    }

    /// <summary>
    /// 아이템을 내려놓는다
    /// </summary>
    public void PutDownItem (
        float lookDir, float putDownDist, float putDownCheckRadius, LayerMask obstacleLayerMask )
    {
        //들고 있는 아이템이 없으면 종료
        if ( _holdItem == null ) return;

        //들고 있는 아이템이 상호작용 아이템이 아니면 종료
        if ( _holdItem is InteractableItem holdItem == false ) return;

        //내려놓을 위치 계산
        Vector3 pos = GetPutDownPos( lookDir, putDownDist );

        //지면 기준 위치 보정
        pos = GetGroundedPutDownPos( pos, holdItem );

        //내려놓을 수 없으면 종료
        if ( CanPutDownItem( pos, putDownCheckRadius, obstacleLayerMask ) == false ) return;

        //아이템 내려놓기
        holdItem.PutDown( pos );

        //들고 있는 아이템 비우기
        _holdItem = null;

        //들고 있는 아이템 해제 이벤트 발행
        OnHoldItemCleared?.Invoke( );

        //아이템 내려놓기 이벤트 발행
        OnPutDownItem?.Invoke( );
    }

    /// <summary>
    /// 아이템을 내려놓을 위치 반환
    /// </summary>
    /// <returns></returns>
    public Vector3 GetPutDownPos ( float lookDir, float putDownDist )
    {
        //바라보는 방향으로 putDownDist만큼 떨어진 위치 반환
        return transform.position + Vector3.right * lookDir * putDownDist;
    }

    /// <summary>
    /// 지면 기준 내려놓기 위치를 반환한다
    /// </summary>
    /// <param name="basePos">기본 내려놓기 위치</param>
    /// <param name="item">내려놓을 아이템</param>
    /// <returns>지면 보정 위치</returns>
    public Vector3 GetGroundedPutDownPos ( Vector3 basePos, InteractableItem item )
    {
        //아이템 반높이 가져오기
        float halfHeight = item.GetHalfHeight( );

        //아이템 높이만큼 위에서 아래쪽 지면 탐색
        Vector3 rayStartPos = basePos + Vector3.up * halfHeight;

        //위에서 아래 방향으로 지면 탐색
        RaycastHit2D hit = Physics2D.Raycast(
            rayStartPos,
            Vector2.down,
            _putDownGroundCheckDist + halfHeight,
            _putDownGroundLayerMask );

        //지면이 없으면 기본 위치 반환
        if ( hit.collider == null ) return basePos;

        //아이템 반높이만큼 올려서 바닥 아래로 들어가지 않게 보정
        basePos.y = hit.point.y + halfHeight;

        return basePos;
    }

    /// <summary>
    /// 아이템 내려놓기 가능 여부 확인
    /// </summary>
    /// <param name="pos">내려놓을 위치</param>
    /// <returns>가능 여부</returns>
    public bool CanPutDownItem ( Vector3 pos, float putDownCheckRadius, LayerMask obstacleLayerMask )
    {
        Collider2D obstacle = Physics2D.OverlapCircle( pos, putDownCheckRadius, obstacleLayerMask );

        //장애물이 있으면 종료
        if ( obstacle != null ) return false;

        return true;
    }

    /// <summary>
    /// 아이템을 던진다
    /// </summary>
    public void ThrowItem ()
    {
        //들고 있는 아이템이 없으면 종료
        if ( _holdItem == null ) return;

        //조준점 설정
        //마우스 위치 가져오기
        Vector3 mousePos = Input.mousePosition;

        //z값 설정
        mousePos.z = Mathf.Abs( Camera.main.transform.position.z );

        //월드 좌표로 변환
        Vector3 worldPos = Camera.main.ScreenToWorldPoint( mousePos );

        //방향 설정
        Vector2 dir = ( worldPos - _holdItem.transform.position ).normalized;

        if ( _holdItem is InteractableItem holdItem )
        {
            //리지드바디 초기화
            holdItem.SetRigidVelocity( );

            //던지기
            holdItem.Throw( dir );

            //들고 있는 아이템 비우기
            _holdItem = null;

            //들고 있는 아이템 해제 이벤트 발행
            OnHoldItemCleared?.Invoke( );

            //아이템 던지기 이벤트 발행
            OnThrownItem?.Invoke( );
        }
    }

    /// <summary>
    /// 들고 있는 아이템을 제거한다
    /// </summary>
    public void ClearHoldItem ()
    {
        //들고 있는 아이템이 없으면 종료
        if ( _holdItem == null ) return;

        //들고 있던 아이템 제거
        _holdItem.RemoveItem( );

        //들고 있는 아이템 비우기
        _holdItem = null;

        //들고 있는 아이템 해제 이벤트 발행
        OnHoldItemCleared?.Invoke( );
    }

    /// <summary>
    /// 아이템을 당긴다
    /// </summary>
    /// <param name="lookDir">바라보는 방향</param>
    public void PullItem ( Item item, float lookDir )
    {
        //아이템이 없으면 종료
        if ( item == null ) return;

        //상호작용 아이템이 아니면 종료
        if ( item is InteractableItem interactableItem == false ) return;

        //당길 수 없으면 종료
        if ( interactableItem.CheckCanPull( ) == false ) return;

        //당기기 방향
        Vector2 pullDir = Vector2.left * lookDir;

        //목표 위치 가져오기
        Vector2 targetPos = interactableItem.GetPullTargetPos( pullDir, _pullDist );

        //장애물 확인
        Collider2D obstacle = Physics2D.OverlapCircle( targetPos, _pullCheckRadius, _pullObstacleLayerMask );

        //장애물이 있으면 종료
        if ( obstacle != null ) return;

        //무게에 따른 당기기 시간 가져오기
        float duration = interactableItem.GetPullDuration( );

        //아이템 당기기
        interactableItem.Pull( targetPos, duration );

        //아이템 당기기 이벤트 발행
        OnPulledItem?.Invoke( );
    }

}
