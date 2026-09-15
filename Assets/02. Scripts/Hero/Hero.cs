using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 히어로 상태
/// </summary>
public enum HeroState
{
    Normal,     //일반 상태
    Dash,       //대시 상태
    Aim,        //조준 상태
}

/// <summary>
/// 히어로 - 이동, 공격, 아이템 획득
/// </summary>
public class Hero : MonoBehaviour
{
    [Header( "----- 런타임 데이터 -----" )]
    [SerializeField] HeroModel _model;

    [Header( "----- 설정 데이터 -----" )]
    [SerializeField] Transform _respawnPoint;      //리스폰 포인트

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Rigidbody2D _rigid;        //리지드바디
    [SerializeField] Collider2D _collider;      //콜라이더
    [SerializeField] Animator _anim;        //애니메이터
    [SerializeField] SpriteRenderer _renderer;        //렌더러

    Vector2 _originColliderSize;        //히어로 원본 콜라이더 크기
    Vector2 _originColliderOffset;      //히어로 원본 콜라이더 오프셋

    [Header( "----- 상호작용 핸들러 -----" )]
    [SerializeField] Interacter _interacter;      //상호작용 핸들러

    [Header( "----- 상태 -----" )]
    [SerializeField] HeroState _heroState;      //히어로 현재 상태

    #region ----- 이동 -----
    [Header( "----- 이동 -----" )]
    [SerializeField] float _moveSpeed;         //이동 속도
    [SerializeField] float _lookDir;        //바라보는 방향
    Vector2 _moveInput;     //이동 인풋

    [Header( "----- 점프 -----" )]
    [SerializeField] float _jumpPower;      //점프력
    [SerializeField] bool _isGrounded;      //착지 여부
    [SerializeField] LayerMask _groundLayerMask;       //지면 체크 레이어마스크
    [SerializeField] float _groundCheckRadius;      //지면 확인 반지름

    [Header( "----- 대시 -----" )]
    [SerializeField] float _dashSpeed;          //대시 속력
    [SerializeField] float _dashDuration;       //대시 지속 시간
    [SerializeField] float _dashTimer;      //대시 타이머
    [SerializeField] float _dashCooltime;       //대시 쿨타임
    [SerializeField] float _dashCooltimeTimer;      //대시 쿨타임 타이머
    #endregion


    public HeroModel Model => _model;
    public Interacter Interacter => _interacter;

    /// <summary>
    /// 인벤토리 조회
    /// </summary>
    public Dictionary<ItemData, int> Inventory => _model.Inventory;

    #region ----- 이벤트 -----
    /// <summary>
    /// 무게 변경 이벤트
    /// </summary>
    public event Action<float, float> OnWeightChanged;
    /// <summary>
    /// 인벤토리 아이템 제출 이벤트
    /// </summary>
    public event Action<Dictionary<ItemData, int>> OnInventoryItemSubmitted;
    /// <summary>
    /// 상호작용 아이템 제출 이벤트
    /// </summary>
    public event Action<Item> OnInteractItemSubmitted;
    /// <summary>
    /// 인벤토리 변경 이벤트
    /// </summary>
    public event Action<Dictionary<ItemData, int>> OnInventoryChanged;
    /// <summary>
    /// 목숨 변경 이벤트
    /// </summary>
    public event Action<int> OnLifeChanged;
    /// <summary>
    /// 보너스 시간 획득 이벤트
    /// </summary>
    public event Action<float> OnGetBonusTime;

    /// <summary>
    /// 아이템 획득 이벤트
    /// </summary>
    public event Action OnItemCollected;
    /// <summary>
    /// 보너스 목숨 획득 이벤트
    /// </summary>
    public event Action OnGetBonusLife;
    /// <summary>
    /// 점프 성공 이벤트
    /// </summary>
    public event Action OnJumped;
    /// <summary>
    /// 빠른 하강 성공 이벤트
    /// </summary>
    public event Action OnFastFalled;
    /// <summary>
    /// 대시 성공 이벤트
    /// </summary>
    public event Action OnDashed;
    /// <summary>
    /// 공격 성공 이벤트
    /// </summary>
    public event Action OnAttacked;
    /// <summary>
    /// 아이템 들기 성공 이벤트
    /// </summary>
    public event Action OnHoldItem;
    /// <summary>
    /// 아이템 내려놓기 성공 이벤트
    /// </summary>
    public event Action OnPutDownItem;
    /// <summary>
    /// 아이템 던지기 성공 이벤트
    /// </summary>
    public event Action OnThrownItem;
    /// <summary>
    /// 아이템 당기기 성공 이벤트
    /// </summary>
    public event Action OnPulledItem;
    #endregion


    /// <summary>
    /// 초기화
    /// </summary>
    /// <param name="goalScore">목표 점수</param>
    public void Init ()
    {
        //공격 타이머 초기화
        _model.AttackTimer = 0;

        //대시 쿨타임 타이머 초기화
        _dashCooltimeTimer = 0;

        //던지기 쿨타임 타이머 초기화
        _model.ThrowTimer = 0;

        //콜라이더 가져오기
        CapsuleCollider2D collider = _collider as CapsuleCollider2D;

        //콜라이더 원본값 저장
        _originColliderSize = collider.size;
        _originColliderOffset = collider.offset;

        //Interacter 제출 이벤트를 Hero 제출 이벤트로 중재
        _interacter.OnInventoryItemsSubmitted += SubmitInventoryItem;
        _interacter.OnInteractItemSubmitted += SubmitInteractItem;
        _interacter.OnHoldItem += HoldItem;
        _interacter.OnPutDownItem += PutDownItem;
        _interacter.OnThrownItem += ThrownItem;
        _interacter.OnPulledItem += PulledItem;
        _interacter.OnHoldItemChanged += ExpandColliderByItem;
        _interacter.OnHoldItemCleared += RestoreCollider;
    }

    /// <summary>
    /// 메인 맵 리스폰 포인트 설정
    /// </summary>
    /// <param name="respawnPoint">메인 맵 리스폰 포인트</param>
    public void SetRespawnPoint ( Transform respawnPoint )
    {
        //리스폰 포인트 갱신
        _respawnPoint = respawnPoint;

        //히어로 위치 이동
        transform.position = _respawnPoint.position;
    }

    private void Update ()
    {
        UpdateDashCooltime( );
        UpdateAttackTimer( );
        UpdateThrowCooltime( );

        switch ( _heroState )
        {
            case HeroState.Normal:
                HandleMove( );
                break;
            case HeroState.Dash:
                HandleDash( );
                break;
            case HeroState.Aim:
                HandleMove( );
                break;
        }

    }

    private void FixedUpdate ()
    {
        //지면 체크
        CheckIsGround( );
    }

    #region ----- 핸들/상태 전환 -----
    public void HandleMove ()
    {
        Move( _moveInput.x );
    }

    public void HandleDash ()
    {
        //대시 시간 증가
        _dashTimer += Time.deltaTime;

        //바라보는 방향으로 빠르게 이동
        _rigid.linearVelocityX = _lookDir * _dashSpeed;

        //대시 시간이 지속 시간보다 짧으면 종료
        if ( _dashTimer < _dashDuration ) return;

        //종료 시 속도 초기화
        _rigid.linearVelocityX = 0;

        //일반 상태로 전환
        ChangeState( HeroState.Normal );
    }

    /// <summary>
    /// 우선순위에 따라 상호작용
    /// </summary>
    public void HandleInteract ()
    {
        //조준 상태면 종료
        if ( _heroState == HeroState.Aim ) return;

        //아이템 제출 성공 시 종료
        if ( SubmitItems( ) == true ) return;

        //근처 아이템과 상호작용 성공 시 종료
        if ( InteractWithItem( ) == true ) return;

    }


    #region --- 상태 전환 ---
    /// <summary>
    /// 히어로 상태 변경
    /// </summary>
    /// <param name="state">변경할 상태</param>
    public void ChangeState ( HeroState state )
    {
        //현재 상태 변경
        _heroState = state;
    }

    #endregion

    #endregion

    #region ----- 이동/인풋 시스템 -----
    #region --- 이동, 지면 체크, 점프 ---
    /// <summary>
    /// 이동
    /// </summary>
    /// <param name="dir">이동 방향 값</param>
    public void Move ( float dir )
    {
        _rigid.linearVelocityX = dir * _moveSpeed;

        //바라보는 방향에 따라 렌더러 플립
        //오른쪽을 볼 때
        if ( dir > 0 )
        {
            _lookDir = 1;
            _renderer.flipX = false;
        }
        //왼쪽을 볼 때 
        else if ( dir < 0 )
        {
            _lookDir = -1;
            _renderer.flipX = true;
        }

        //애니메이터 파라미터 설정
        _anim.SetFloat( "MoveSpeed", Mathf.Abs( _rigid.linearVelocityX ) );

    }

    /// <summary>
    /// 착지 여부 확인
    /// </summary>
    public void CheckIsGround ()
    {
        //_groundLayerMask가 포함하는 레이어의 콜라이어와 닿아 있는지 여부
        //닿아 있다면
        if ( _isGrounded = _rigid.IsTouchingLayers( _groundLayerMask ) == true )
        {
            //자신 게임오브젝트 위치를 중심으로 _groundCheckRadius만큼의 반지름을 가지는 원을 그리고
            //거기에 겹쳐지는 _groundLayerMask에 포함된 레이어의 콜라이더가 있는 경우
            if ( Physics2D.OverlapCircle( transform.position, _groundCheckRadius, _groundLayerMask ) )
            {
                _isGrounded = true;
            }
            else
            {
                //벽에 비비는 거
                _isGrounded = false;
            }

        }
        //안 닿아 있으면
        else
        {
            _isGrounded = false;
        }

        //애니메이터 파라미터 설정
        _anim.SetBool( "IsGrounded", _isGrounded );
    }

    /// <summary>
    /// 점프
    /// </summary>
    public void Jump ()
    {
        //공중이라면 종료
        if ( _isGrounded == false ) return;

        ////보류: 들고 있는 아이템 위쪽 공간 검사
        ////if ( CanJumpWithHoldItem( ) == false ) return;

        //리지드바디 상하 방향으로 즉각적인 힘 적용
        _rigid.AddForceY( _jumpPower, ForceMode2D.Impulse );

        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.Jump );

        //애니메이션 파라미터 설정
        _anim.SetTrigger( "Jump" );

        //점프 성공 이벤트 발행
        OnJumped?.Invoke( );
    }

    /// <summary>
    /// 점프 중 빠른 하강
    /// </summary>
    public void FastFall ()
    {
        //착지 중이라면 종료
        if ( _isGrounded == true ) return;

        //-y 방향으로 점프력 적용
        _rigid.linearVelocityY = -_jumpPower;

        //빠른 하강 성공 이벤트 발행
        OnFastFalled?.Invoke( );
    }

    /// <summary>
    /// 대시
    /// </summary>
    public void Dash ()
    {
        //이미 대시 중이면 종료
        if ( _heroState == HeroState.Dash ) return;

        //조준 상태면 대시 불가
        if ( _heroState == HeroState.Aim ) return;

        //쿨타임이 덜 찼으면 종료
        if ( _dashCooltimeTimer < _dashCooltime ) return;

        //대시 상태로 전환
        ChangeState( HeroState.Dash );

        //바라보는 방향으로 대시 속도 적용
        _rigid.linearVelocityX = _lookDir * _dashSpeed;

        //대시 타이머 초기화
        _dashTimer = 0;

        //대시 쿨타임 타이머 초기화
        _dashCooltimeTimer = 0;

        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.Dash );

        //대시 애니메이션 재생
        _anim.SetTrigger( "Dash" );

        //대시 성공 이벤트 발행
        OnDashed?.Invoke( );
    }

    /// <summary>
    /// 대시 쿨타임 갱신
    /// </summary>
    public void UpdateDashCooltime ()
    {
        //쿨타임이 이미 찼으면 종료
        if ( _dashCooltimeTimer >= _dashCooltime ) return;

        //대시 쿨타임 타이머 증가
        _dashCooltimeTimer += Time.deltaTime;
    }
    #endregion

    #region --- 인풋 시스템 ---
    public void OnMove ( InputValue inputValue )
    {
        //인풋 시스템 벡터2 값 가져오기
        _moveInput = inputValue.Get<Vector2>( );
    }

    public void OnJump ( InputValue inputValue )
    {
        //안 눌렸으면 종료
        if ( inputValue.isPressed == false ) return;

        Jump( );
    }

    public void OnAttack ( InputValue inputValue )
    {
        if ( inputValue.isPressed == false ) return;
        //조준 상태면 종료
        if ( _heroState == HeroState.Aim ) return;
        //아이템을 든 상태면 종료
        if ( _interacter.HasHoldItem == true ) return;

        Attack( );
    }

    public void OnDash ( InputValue inputValue )
    {
        if ( inputValue.isPressed == false ) return;

        //대시 실행
        Dash( );
    }

    public void OnFastFall ( InputValue inputValue )
    {
        if ( inputValue.isPressed == false ) return;

        FastFall( );
    }

    public void OnInteract ( InputValue inputValue )
    {
        if ( inputValue.isPressed == false ) return;

        HandleInteract( );
    }

    public void OnPutDown ( InputValue inputValue )
    {
        if ( inputValue.isPressed == false ) return;

        //조준 상태면 조준 취소
        if ( _heroState == HeroState.Aim )
        {
            CancelAim( );
            return;
        }

        //아이템 내려놓기
        _interacter.PutDownItem( _lookDir, _model.PutDownDist, _model.PutDownCheckRadius, _model.PutDownBlockLayerMask );
    }

    /// <summary>
    /// f 클릭 시 조준 상태 전환
    /// </summary>
    /// <param name="inputValue"></param>
    public void OnAim ( InputValue inputValue )
    {
        if ( inputValue.isPressed == false ) return;

        StartAim( );
    }

    /// <summary>
    /// 우클릭 시 아이템 던지기
    /// </summary>
    /// <param name="inputValue"></param>
    public void OnThrow ( InputValue inputValue )
    {
        if ( inputValue.isPressed == false ) return;

        ThrowItem( );
    }

    public void OnPull ( InputValue inputValue )
    {
        if ( inputValue.isPressed == false ) return;
        //대시 중이면 종료
        if ( _heroState == HeroState.Dash ) return;
        //조준 중이면 종료
        if ( _heroState == HeroState.Aim ) return;

        PullItem( );
    }
    #endregion

    #endregion

    #region ----- 공격 -----
    /// <summary>
    /// 공격 쿨타임 갱신
    /// </summary>
    public void UpdateAttackTimer ()
    {
        //이미 차 있으면 종료
        if ( _model.AttackTimer >= _model.AttackCooltime ) return;

        //공격 쿨타임 타이머 증가
        _model.AttackTimer += Time.deltaTime;
    }

    /// <summary>
    /// 목표를 공격한다
    /// </summary>
    public void Attack ()
    {
        //쿨타임 돌고 있으면 종료
        if ( _model.AttackTimer < _model.AttackCooltime ) return;

        //타이머 초기화
        _model.AttackTimer = 0;

        //애니메이터 파라미터 설정
        _anim.SetTrigger( "Attack" );

        //공격 사운드 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.Attack );

        //공격 성공 이벤트 발행
        OnAttacked?.Invoke( );

        //레이캐스트로 ItemBox 레이어인 콜라이더가 감지되면 공격 판정
        //레이캐스트 시작점
        Vector2 origin = _model.Center.position;

        //레이캐스트 기본 방향(오른쪽)
        Vector2 dir = Vector2.right * _lookDir;

        //레이캐스트 감지 정보 가져오기
        //origin에서 레이저 광선을 dir 방향으로 _attackRange 길이만큼 발사
        //거기에 _targetLayerMast에 포함되는 레이어 콜라이더가 감지될 경우
        RaycastHit2D hit = Physics2D.Raycast( origin, dir, _model.AttackRange, _model.TargetLayerMask );

        //감지 정보가 있다면
        if ( hit == true )
        {
            //감지된 상대 콜라이더에서 ItemBox 컴포넌트를 가져온다
            ItemBox itemBox = hit.collider.GetComponent<ItemBox>( );

            //ItemBox가 있다면
            if ( itemBox != null )
            {
                //공격
                itemBox.TakeHit( _model.Damage );
            }
        }
    }

    /// <summary>
    /// 던지기 쿨타임 갱신
    /// </summary>
    public void UpdateThrowCooltime ()
    {
        //쿨타임이 이미 찼으면 종료
        if ( _model.ThrowTimer >= _model.ThrowCooltime ) return;

        //던지기 타이머 증가
        _model.ThrowTimer += Time.deltaTime;
    }

    /// <summary>
    /// 아이템을 던진다
    /// </summary>
    public void ThrowItem ()
    {
        //조준 상태가 아니면 종료
        if ( _heroState != HeroState.Aim ) return;

        //던지기
        _interacter.ThrowItem( );

        //쿨타임 초기화
        _model.ThrowTimer = 0;

        //일반 상태로 전환
        ChangeState( HeroState.Normal );
    }

    /// <summary>
    /// 조준 시작
    /// </summary>
    public void StartAim ()
    {
        //들고 있는 아이템이 없으면 종료
        if ( _interacter.HasHoldItem == false ) return;

        //쿨타임이 덜 찼으면 종료
        if ( _model.ThrowTimer < _model.ThrowCooltime ) return;

        //상태 변경
        ChangeState( HeroState.Aim );
    }

    /// <summary>
    /// 조준 취소
    /// </summary>
    public void CancelAim ()
    {
        //조준 상태가 아니면 종료
        if ( _heroState != HeroState.Aim ) return;

        //일반 상태로 전환
        ChangeState( HeroState.Normal );
    }

    #endregion

    #region ----- 피격 -----
    /// <summary>
    /// 인벤토리를 비운다
    /// </summary>
    public void Caught ()
    {
        //인벤토리 비우기
        _model.ClearInventory( );

        //무게 변경 이벤트 발행
        OnWeightChanged?.Invoke( _model.CurrentWeight, _model.MaxWeight );
        //인벤토리 변경 이벤트 발행
        OnInventoryChanged?.Invoke( _model.Inventory );
    }

    /// <summary>
    /// 목숨을 잃는다
    /// </summary>
    public void LoseLives ()
    {
        _model.LoseLife( );

        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.Hit );

        //목숨 변경 이벤트 발행
        OnLifeChanged?.Invoke( _model.CurrentLives );
    }

    public void AddLives ()
    {
        _model.AddLife( );

        //목숨 변경 이벤트 발행
        OnLifeChanged?.Invoke( _model.CurrentLives );

        //보너스 목숨 획득 이벤트 발행
        OnGetBonusLife?.Invoke( );
    }

    #endregion

    #region ----- 아이템 -----
    /// <summary>
    /// 인벤토리/상호작용 아이템 제출 처리
    /// </summary>
    /// <returns>제출 성공 여부</returns>
    public bool SubmitItems ()
    {
        //인벤토리 제출 시도
        bool invenSubmitted = _interacter.SubmitInventoryItem( _model.Inventory );

        //인벤토리 제출에 성공하면 인벤토리 초기화
        if ( invenSubmitted == true )
        {
            //인벤토리 비우기
            _model.ClearInventory( );
            //무게 변경 이벤트 발행
            OnWeightChanged?.Invoke( _model.CurrentWeight, _model.MaxWeight );
            //인벤토리 변경 이벤트 발행
            OnInventoryChanged?.Invoke( _model.Inventory );
        }

        //들고 있는 아이템 제출 시도
        bool interactSubmitted = _interacter.SubmitInteractItem( );

        //둘 중 하나라도 성공이면 성공
        return invenSubmitted == true || interactSubmitted == true;
    }


    /// <summary>
    /// 근처 아이템과 상호작용
    /// </summary>
    /// <returns>상호작용 성공 여부</returns>
    public bool InteractWithItem ()
    {
        //근처 아이템 찾기
        Item item = _interacter.FindItem( );

        //아이템이 없으면 종료
        if ( item == null ) return false;

        //인벤토리 아이템이면 인벤토리에 추가
        if ( item.CarryType == CarryType.Inventory )
        {
            if ( _model.AddItem( item.ItemData ) == true )
            {
                //무게 변경 이벤트 발행
                OnWeightChanged?.Invoke( _model.CurrentWeight, _model.MaxWeight );

                //인벤토리 변경 이벤트 발행
                OnInventoryChanged?.Invoke( _model.Inventory );

                //아이템 획득 이벤트 발행
                OnItemCollected?.Invoke( );

                //제거
                item.RemoveItem( );
            }

            return true;
        }

        //상호작용 아이템이면 들기
        if ( item.CarryType == CarryType.Interact )
        {
            return _interacter.HoldItem( item );
        }

        return false;
    }

    /// <summary>
    /// 아이템을 당긴다
    /// </summary>
    public void PullItem ()
    {
        //영역 안에 있는 아이템 찾기
        Item item = _interacter.FindItem( );

        //아이템이 없으면 종료
        if ( item == null ) return;

        //바라보는 방향에 아이템이 없으면 종료
        if ( CheckFrontItem( item ) == false ) return;

        //당기기
        _interacter.PullItem( item, _lookDir );
    }

    /// <summary>
    /// 바라보는 방향에 아이템이 있는지 확인
    /// </summary>
    /// <param name="item">아이템</param>
    /// <returns></returns>
    public bool CheckFrontItem ( Item item )
    {
        //아이템이 없으면 종료
        if ( item == null ) return false;

        //방향 계산
        float dir = Mathf.Sign( item.transform.position.x - transform.position.x );

        //히어로가 바라보는 방향에 아이템이 있으면 true
        return dir == Mathf.Sign( _lookDir );
    }

    /// <summary>
    /// 추가 시간 획득
    /// </summary>
    /// <param name="bonus"></param>
    public void GetBonusTime ( float bonus )
    {
        //보너스 타임 획득 이벤트 발행
        OnGetBonusTime?.Invoke( bonus );
    }

    /// <summary>
    /// 들고 있는 아이템 높이만큼 콜라이더 확장
    /// </summary>
    /// <param name="item">들고 있는 아이템</param>
    void ExpandColliderByItem ( Item item )
    {
        //캡슐 콜라이더 가져오기
        CapsuleCollider2D collider = _collider as CapsuleCollider2D;

        //아이템 세로 길이 가져오기
        float itemHeight = item.Collider.bounds.size.y;

        //원본 크기 기준으로 세로 길이 확장
        Vector2 size = _originColliderSize;
        size.y += itemHeight;

        //아래쪽 위치는 유지하고 위쪽으로만 확장
        Vector2 offset = _originColliderOffset;
        offset.y += itemHeight * 0.5f;

        //콜라이더 적용
        collider.size = size;
        collider.offset = offset;
    }

    /// <summary>
    /// 콜라이더를 원래 크기로 복구
    /// </summary>
    void RestoreCollider ()
    {
        //캡슐 콜라이더 가져오기
        CapsuleCollider2D collider = _collider as CapsuleCollider2D;

        //원본 크기 복구
        collider.size = _originColliderSize;

        //원본 오프셋 복구
        collider.offset = _originColliderOffset;
    }

    /// <summary>
    /// 인벤토리 아이템 제출 이벤트 발행
    /// </summary>
    /// <param name="inventory">제출할 인벤토리</param>
    void SubmitInventoryItem ( Dictionary<ItemData, int> inventory )
    {
        OnInventoryItemSubmitted?.Invoke( inventory );
    }

    /// <summary>
    /// 상호작용 아이템 제출 이벤트 발행
    /// </summary>
    /// <param name="item">제출할 아이템</param>
    void SubmitInteractItem ( Item item )
    {
        OnInteractItemSubmitted?.Invoke( item );
    }

    /// <summary>
    /// 아이템 들기 이벤트 발행
    /// </summary>
    void HoldItem ()
    {
        OnHoldItem?.Invoke( );
    }

    /// <summary>
    /// 아이템 내려놓기 이벤트 발행
    /// </summary>
    void PutDownItem ()
    {
        OnPutDownItem?.Invoke( );
    }

    /// <summary>
    /// 아이템 던지기 이벤트 발행
    /// </summary>
    void ThrownItem ()
    {
        OnThrownItem?.Invoke( );
    }

    /// <summary>
    /// 아이템 당기기 이벤트 발행
    /// </summary>
    void PulledItem ()
    {
        OnPulledItem?.Invoke( );
    }

    #endregion

    #region ----- 추락 -----

    /// <summary>
    /// 목숨, 아이템을 잃고 시작 위치로 돌아간다
    /// </summary>
    public void BackToStart ()
    {
        if ( _model.CurrentLives <= 0 ) return;

        LoseLives( );

        //인벤토리 비우기
        _model.ClearInventory( );

        //들고 있던 아이템 없애기
        _interacter.ClearHoldItem( );

        //무게 변경 이벤트 발행
        OnWeightChanged?.Invoke( _model.CurrentWeight, _model.MaxWeight );
        //인벤토리 변경 이벤트 발행
        OnInventoryChanged?.Invoke( _model.Inventory );

        transform.position = _respawnPoint.position;
    }

    private void OnTriggerEnter2D ( Collider2D collision )
    {
        if ( collision.CompareTag( "FallBox" ) )
        {
            BackToStart( );
        }
    }
    #endregion

    #region ----- 기즈모 -----
    private void OnDrawGizmos ()
    {
        //지면 감지
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere( transform.position, _groundCheckRadius );

        //공격 범위
        if ( _model.Center != null )
        {
            Gizmos.color = Color.red;
            //오른쪽 * _lookDir방향으로 _attackRange 길이만큼
            Gizmos.DrawLine( _model.Center.position, _model.Center.position + Vector3.right * _lookDir * _model.AttackRange );

        }

        //아이템 체크 범위
        Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere( transform.position, _itemCheckRadius );

    }
    #endregion
}
