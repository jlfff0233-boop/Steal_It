using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary>
/// 상호작용 아이템 - 들기, 당기기, 놓기, 던지기
/// </summary>
public class InteractableItem : Item
{
    [Header( "----- 런타임 데이터(상호작용) -----" )]
    [SerializeField] int _usableCount;      //사용 가능 횟수
    [SerializeField] bool _isHeld;      //들고 있는지 여부

    [SerializeField] bool _isThrown;        //던져졌는지 여부
    [SerializeField] float _stunSpan;       //스턴 간격

    [SerializeField] bool _isPulling;       //당기는 중인지 여부
    [SerializeField] float _pullDuration;          //기본 당기기 시간
    [SerializeField] float _pullMaxDuration;        //최대 당기기 시간
    [SerializeField] float _pullDurationRatio;   //무게당 추가 시간

    [Header( "----- 던지기 -----" )]
    [SerializeField] float _minThrowSpeed;          //최소 던지기 속도
    [SerializeField] float _maxThrowSpeed;         //최대 던지기 속도
    [SerializeField] float _throwWeightRatio;    //무게당 던지기 속도 감소 비율

    [Header( "----- 들기 -----" )]
    [SerializeField] Transform _holdPoint;       //들고 있을 때 따라갈 위치
    [SerializeField] float _originGravityScale;      //원래 중력 값
    [SerializeField] RigidbodyConstraints2D _originConstraints;     //원래 리지드바디 제약
    [SerializeField] bool _originIsTrigger;      //원래 콜라이더 트리거 여부

    Coroutine _pullRoutine;

    public void SetRigidVelocity ()
    {
        //기존 속도 초기화
        _rigid.linearVelocity = Vector2.zero;
        _rigid.angularVelocity = 0f;
    }

    /// <summary>
    /// 아이템 들림
    /// </summary>
    /// <param name="holdPoint">들려질 위치</param>
    public void Hold ( Transform holdPoint )
    {
        //레이어 처리
        SetHeldLayer( );

        //들림 상태 처리
        _isHeld = true;

        //홀드 포인트 저장
        _holdPoint = holdPoint;

        //부모 설정
        transform.SetParent( holdPoint );

        //원래 중력 값 저장
        _originGravityScale = _rigid.gravityScale;

        //원래 리지드바디 제약 저장
        _originConstraints = _rigid.constraints;

        //원래 콜라이더 트리거 여부 저장
        _originIsTrigger = _collider.isTrigger;

        //들고 있는 동안 떨어지지 않도록 중력 제거
        _rigid.gravityScale = 0f;

        //들고 있는 동안 회전하지 않도록 제약 추가
        _rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

        //들고 있는 동안 홀드 포인트를 따라가도록 물리 비활성화
        _rigid.simulated = false;

        //속도 초기화
        SetRigidVelocity( );

        //위치 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.Item );
    }

    /// <summary>
    /// 아이템 내려놓음
    /// </summary>
    /// <param name="pos">내려놓을 위치</param>
    public void PutDown ( Vector3 pos )
    {
        //레이어 처리
        SetItemLayer( );

        //부모 해제
        transform.SetParent( null );

        //위치 설정
        transform.position = pos;

        //회전 초기화
        transform.rotation = Quaternion.identity;

        //들고 있는 상태 해제
        ClearHold( );

        //리지드바디 복구
        _rigid.simulated = true;

        //속도 초기화
        SetRigidVelocity( );
    }

    public override bool Submit ()
    {
        //아이템 레이어 처리
        SetItemLayer( );

        //들고 있는 상태 해제
        ClearHold( );

        //던져짐 상태 처리
        _isThrown = false;

        return base.Submit( );
    }

    /// <summary>
    /// 아이템 던짐
    /// </summary>
    /// <param name="dir">던질 방향</param>
    public void Throw ( Vector2 dir )
    {
        //사용 횟수가 없으면 종료
        if ( _usableCount <= 0 ) return;

        //부모 해제
        transform.SetParent( null );

        //들고 있는 상태 해제
        ClearHold( );

        //던져짐 상태 처리
        _isThrown = true;

        //레이어 처리
        SetThrownLayer( );

        //리지드바디 복구
        _rigid.simulated = true;

        //속도 초기화
        SetRigidVelocity( );

        //무게에 따른 최종 던지기 속도 계산
        float throwSpeed = GetThrowSpeed( );

        //속도 적용
        _rigid.linearVelocity = dir * throwSpeed;
    }

    /// <summary>
    /// 무게에 따른 던지기 속도 계산
    /// </summary>
    /// <returns>보정된 던지기 속도</returns> 
    public float GetThrowSpeed ()
    {
        //무게가 커질수록 던지기 속도 감소
        float throwSpeed = _maxThrowSpeed - ItemWeight * _throwWeightRatio;

        //최소/최대 속도 제한
        return Mathf.Clamp( throwSpeed, _minThrowSpeed, _maxThrowSpeed );
    }

    /// <summary>
    /// 사용 횟수 갱신
    /// </summary>
    public void UpdateUsableCount ()
    {
        _usableCount--;

        //사용 횟수가 닳으면 일정 시간 후 파괴
        if ( _usableCount <= 0 )
        {
            RemoveItem( );
        }
    }

    /// <summary>
    /// 아이템 당기기
    /// </summary>
    /// <param name="dir">이동 방향</param>
    /// <param name="dist">이동 거리</param>
    /// <param name="duration">이동 시간</param>
    /// <param name="dir">이동 방향</param>
    /// <param name="dist">이동 거리</param>
    /// <returns>목표 위치</returns>
    public Vector2 GetPullTargetPos ( Vector2 dir, float dist )
    {
        //현재 위치에서 이동 방향과 거리만큼 떨어진 위치 반환
        return _rigid.position + dir.normalized * dist;
    }

    /// <summary>
    /// 아이템 당기기
    /// </summary>
    /// <param name="targetPos">목표 위치</param>
    /// <param name="duration">이동 시간</param>
    public void Pull ( Vector2 targetPos, float duration )
    {
        //당기기
        _isPulling = true;

        //속도 초기화
        SetRigidVelocity( );

        //지속 시간 동안 목표 위치로 이동
        //종료 후 _isPulling 처리
        _rigid.DOMove( targetPos, duration ).SetEase( Ease.OutExpo ).OnComplete( () => { _isPulling = false; } );
    }


    /// <summary>
    /// 당기기 가능 여부 확인
    /// </summary>
    /// <returns></returns>
    public bool CheckCanPull ()
    {
        //모양 타입이 사각형이 아니면 종료
        if ( ShapeType != ShapeType.Square ) return false;
        //당기는 중이면 종료
        if ( _isPulling == true ) return false;
        //들려 있는 상태면 종료
        if ( _isHeld == true ) return false;
        //던져진 상태면 종료
        if ( _isThrown == true ) return false;

        return true;
    }

    /// <summary>
    /// 무게에 따른 당기기 시간 계산
    /// </summary>
    /// <returns>당기기 시간</returns>
    public float GetPullDuration ()
    {
        //당기기 시간 계산
        float duration = _pullDuration + ItemWeight * _pullDurationRatio;

        //둘 중 작은 값 반환
        return Mathf.Min( duration, _pullMaxDuration );
    }

    /// <summary>
    /// 아이템 콜라이더 높이 반절만 반환
    /// </summary>
    /// <returns>콜라이더 반높이</returns>
    public float GetHalfHeight ()
    {
        //콜라이더 높이 반절 반환
        return _collider.bounds.extents.y;
    }

    /// <summary>
    /// 아이템 제거
    /// </summary>
    public override void RemoveItem ()
    {
        //비활성화
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 아이템 레리어로 변경
    /// </summary>
    void SetItemLayer ()
    {
        gameObject.layer = LayerMask.NameToLayer( "Item" );
    }

    /// <summary>
    /// 던져진 아이템 레이어로 변경
    /// </summary>
    void SetThrownLayer ()
    {
        gameObject.layer = LayerMask.NameToLayer( "ThrownItem" );
    }

    /// <summary>
    /// 들고 있는 아이템 레이어로 변경
    /// </summary>
    void SetHeldLayer ()
    {
        gameObject.layer = LayerMask.NameToLayer( "HeldItem" );
    }

    /// <summary>
    /// 들림 상태 해제
    /// </summary>
    void ClearHold ()
    {
        //들림 상태 해제
        _isHeld = false;

        //홀드 포인트 해제
        _holdPoint = null;

        //중력 복구
        _rigid.gravityScale = _originGravityScale;

        //리지드바디 제약 복구
        _rigid.constraints = _originConstraints;

        //콜라이더 트리거 여부 복구
        _collider.isTrigger = _originIsTrigger;
    }

    /// <summary>
    /// 던짐 상태 종료
    /// </summary>
    public void FinishThrow ()
    {
        //던져짐 상태 해제
        _isThrown = false;

        //레이어 처리
        SetItemLayer( );

        //속도 초기화
        SetRigidVelocity( );
    }

    private void OnCollisionEnter2D ( Collision2D collision )
    {
        //던져진 상태가 아니면 종료
        if ( _isThrown == false ) return;

        if ( collision.gameObject.CompareTag( "Enemy" ) )
        {
            //컴포넌트 가져오기
            //에너미 스턴
            Enemy enemy = collision.gameObject.GetComponent<Enemy>( );

            //에너미가 없으면 종료
            if ( enemy == null ) return;

            //에너미 스턴 상태 가져오기
            bool isStunned = enemy.IsStunned;

            //던짐 상태 종료
            FinishThrow( );

            //에너미가 이미 스턴 상태면 종료
            if ( isStunned == true ) return;

            //에너미 스턴 처리
            enemy.Stunned( _stunSpan );

            //사용 횟수 감소
            UpdateUsableCount( );
        }

        //땅과 충돌 시 레이어 처리
        if ( collision.gameObject.layer == LayerMask.NameToLayer( "Ground" ) )
        {
            FinishThrow( );
        }
    }

}
