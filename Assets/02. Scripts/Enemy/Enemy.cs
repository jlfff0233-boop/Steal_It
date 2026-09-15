using System.Collections;
using UnityEngine;

/// <summary>
/// 에너미 - 순찰, 추격, 공격, 상태 변경
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header( "----- 의존성 -----" )]
    [SerializeField] Hero _target;

    [Header( "----- 런타임 데이터 -----" )]
    [SerializeField] EnemyModel _model;

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Rigidbody2D _rigid;        //리지드바디
    [SerializeField] Collider2D _collider;      //콜라이더
    [SerializeField] SpriteRenderer _renderer;      //렌더러
    [SerializeField] Animator _anim;        //애니메이터
    [SerializeField] Transform _groundSensor;       //땅 감지
    [SerializeField] Transform _wallSensor;     //벽 감지
    [SerializeField] Transform _firePoint;      //발사 지점

    [Header( "----- 리소스 -----" )]
    [SerializeField] Projectile _projectile;        //투사체

    [Header( "----- 이동 -----" )]
    [SerializeField] float _moveSpeed;      //이동 속도
    [SerializeField] float _dir;        //방향

    [SerializeField] bool _isGrounded;      //착지 여부
    [SerializeField] float _groundCheckRadius;      //지면 체크 반지름
    [SerializeField] LayerMask _groundLayerMask;     //지면 체크 레이어마스크

    [Header( "----- 공격 -----" )]
    [SerializeField] float _attackSpan;     //공격 간격
    [SerializeField] float _projSpeed;      //투사체 이동 속도
    [SerializeField] float _projMinTargetDist;      //투사체 최소 목표 거리

    [Header( "----- 튜토리얼 -----" )]
    [SerializeField] bool _isTutorialIdle;      //튜토리얼 정지 여부


    //상태
    IEnemyState _state;
    //상태 객체 배열
    IEnemyState [ ] _states = new IEnemyState [ ( int ) EnemyStateType.Count ];

    #region ----- 프로퍼티 -----
    public float ChaseDuration => _model.ChaseDuration;
    public float OriginalMoveSpeed => _model.OriginalMoveSpeed;
    public float StunSpan => _model.StunSpan;
    public bool IsStunned => _model.IsStunned;
    public Animator Anim => _anim;
    #endregion

    #region ----- 시작 -----
    public virtual void Init ( Hero hero )
    {
        _target = hero;
        _model.Init( hero, _moveSpeed );

        //상태 객체 생성
        _states [ ( int ) EnemyStateType.Normal ] = new NormalState( this, _attackSpan );
        _states [ ( int ) EnemyStateType.Chase ] = new ChaseState( this, _model.ChaseSpeed );
        _states [ ( int ) EnemyStateType.Stun ] = new StunState( this, _model.StunSpan );
        _states [ ( int ) EnemyStateType.Attack ] = new AttackState( this );

        //일반 상태로 시작
        ChangeState( EnemyStateType.Normal );
    }

    private void OnEnable ()
    {
        //아이템 상자 이벤트 구독
        ItemBox.OnHasHit += KnowBoxHit;
        ItemBox.OnOpened += KnowBoxOpened;
    }

    private void OnDisable ()
    {
        //아이템 상자 이벤트 해제
        ItemBox.OnHasHit -= KnowBoxHit;
        ItemBox.OnOpened -= KnowBoxOpened;
    }
    #endregion

    private void Update ()
    {
        //튜토리얼 정지 상태면 종료
        if ( _isTutorialIdle == true ) return;

        //지면 체크
        CheckIsGrounded( );

        if ( _state != null )
            //현재 상태의 업데이트 함수 호출
            _state.Update( );
    }

    #region ----- 핸들 -----
    public void HandlePatrol ()
    {
        //가끔 멈춤/랜덤 방향 전환 코루틴은 기본 상태일 때만 실행

        //지면에 닿아 있지 않으면 종료
        if ( _isGrounded == false ) return;

        //진행 방향에 벽이 있거나, 진행 방향 전방에 지면이 없으면
        if ( CheckFrontObstacles( _dir ) == true )
        {
            _dir *= -1;     //반대 방향으로
        }

        //방향대로 이동
        Move( _dir );
    }
    #endregion

    #region ----- 이동 -----

    /// <summary>
    /// 이동
    /// </summary>
    public void Move ( float dir )
    {
        //이동 속력
        _rigid.linearVelocityX = dir * _moveSpeed;

        //이동 방향에 따라 스프라이트 좌우 반전
        if ( dir > 0 )      //오른쪽
        {
            //트랜스폼 스케일 가져오기
            Vector3 scale = _renderer.transform.localScale;

            //스케일 x 값 설정
            scale.x = dir;

            //반영
            _renderer.transform.localScale = scale;
        }
        if ( dir < 0 )      //왼쪽일 때
        {
            //트랜프폼 스케일 가져오기
            Vector3 scale = _renderer.transform.localScale;

            //스케일 x 값 설정
            scale.x = dir;

            //반영
            _renderer.transform.localScale = scale;
        }

        //애니메이션 파라미터 설정
        //현재 속도여야 하니까 무브스피드x
        _anim.SetFloat( "MoveSpeed", Mathf.Abs( _rigid.linearVelocityX ) );
    }

    /// <summary>
    /// 착지 여부 확인
    /// </summary>
    public void CheckIsGrounded ()
    {
        //_groundLayerMask가 포함하는 레이어의 콜라이더와 닿아 있는지(IsTouchingLayers) 여부
        //가 참일 때
        if ( _isGrounded = _rigid.IsTouchingLayers( _groundLayerMask ) == true )
        {
            //자신 게임오브젝트 위치를 중심으로 _groundCheckRadius 반지름을 갖는 원을 그리고
            //거기에 겹쳐지는 _groundLayerMask에 포함된 레이어의 콜라이더가 있는 경우
            if ( Physics2D.OverlapCircle( transform.position, _groundCheckRadius, _groundLayerMask ) )
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;        //벽에 비비는 거
            }
        }

        //애니메이션 파라미터 설정
        _anim.SetBool( "IsGrounded", _isGrounded );
    }

    /// <summary>
    /// 전방 바닥/벽 확인
    /// </summary>
    public bool CheckFrontObstacles ( float dir )
    {
        //전방 벽 존재 여부
        bool hasFrontWall = Physics2D.Raycast( _wallSensor.position, Vector2.right * dir, 1.0f, _groundLayerMask );

        //전방 지면 존재 여부
        bool hasFrontGound = Physics2D.Raycast( _groundSensor.position, Vector2.down, 1.0f, _groundLayerMask );

        if ( hasFrontWall == true ) return true;
        if ( hasFrontGound == false ) return true;

        return false;
    }

    /// <summary>
    /// 이동 속도 변경
    /// </summary>
    /// <param name="moveSpeed"></param>
    public void ChangeMoveSpeed ( float moveSpeed )
    {
        //기존 속도 초기화
        _rigid.linearVelocity = Vector2.zero;

        _moveSpeed = moveSpeed;
    }

    /// <summary>
    /// 튜토리얼 정지 여부를 설정한다
    /// </summary>
    /// <param name="isIdle">정지 여부</param>
    public void SetTutorialIdle ( bool isIdle )
    {
        //튜토리얼 정지 상태 설정
        _isTutorialIdle = isIdle;

        //정지 상태가 아니면 종료
        if ( _isTutorialIdle == false ) return;

        //속도 정지
        _rigid.linearVelocity = Vector2.zero;

        //이동 애니메이션 정지
        _anim.SetFloat( "MoveSpeed", 0f );

        //착지 상태로 고정
        _anim.SetBool( "IsGrounded", true );

        //대기 애니메이션 재생
        _anim.Play( "Idle" );
    }

    #endregion

    #region ----- 상태 전환 -----
    /// <summary>
    /// 상태 변경
    /// </summary>
    /// <param name="stateType">상태 종류</param>
    public void ChangeState ( EnemyStateType stateType )
    {
        //기존 상태 null 체크
        if ( _state != null )
        {
            //기존 상태와 새 상태가 같은 경우 종료
            if ( _state.StateType == stateType ) return;

            //기존 상태 종료
            _state.Exit( );
        }

        //현재 상태를 새 상태로 변경
        _state = _states [ ( int ) stateType ];

        //현재 상태 시작 실행
        _state.Enter( );
    }
    #endregion

    #region ----- 추격 -----
    /// <summary>
    /// 목표 감지 후 추격 대상 갱신
    /// </summary>
    /// <returns></returns>
    public bool FindTarget ()
    {
        Collider2D target = Physics2D.OverlapCircle( transform.position, _model.TargetCheckRadius, _model.TargetCheckLayerMask );

        //있으면 true 반환
        _model.IsTargetIn = target != null;

        //목표가 있으면
        if ( _model.IsTargetIn == true )
        {
            //컴포넌트 가져오기
            _target = target.GetComponent<Hero>( );
        }

        //목표 존재 여부 반환
        return _model.IsTargetIn;
    }

    /// <summary>
    /// 아이템 상자가 영역 안에 있는지 여부 반환
    /// </summary>
    /// <returns></returns>
    public bool IsItemBoxInRange ( ItemBox itemBox )
    {
        //아이템 상자와의 거리 계산
        float dist = Vector2.Distance( transform.position, itemBox.transform.position );

        //영역 안에 있는지
        bool isInRange = dist <= _model.ItemBoxCheckRadius;

        return isInRange;
    }

    /// <summary>
    /// 목표 방향 반환
    /// </summary>
    /// <returns></returns>
    public float GetTargetDirection ()
    {
        //목표가 오른쪽에 있다면
        if ( _target.transform.position.x > transform.position.x ) return 1;

        return -1;
    }

    /// <summary>
    /// 추격 중 목표와 접촉했는지 여부 반환
    /// </summary>
    public bool CatchTarget ()
    {
        bool isCatched = _collider.IsTouchingLayers( LayerMask.GetMask( "Hero" ) );

        return isCatched;
    }

    /// <summary>
    /// 목표의 아이템을 전부 가져간다
    /// </summary>
    public void TakeAllItems ()
    {
        _target.Caught( );
    }

    /// <summary>
    /// 상자 피격 시 추격 상태 전환
    /// </summary>
    /// <param name="itemBox"></param>
    public void KnowBoxHit ( ItemBox itemBox )
    {
        //목표가 없으면 종료
        if ( FindTarget( ) == false ) return;
        //상자가 영역 안에 없으면 종료
        if ( IsItemBoxInRange( itemBox ) == false ) return;
        //박스 피격 횟수가 3 미만이면 종료
        if ( itemBox.HitCount < 3 ) return;

        //상자 피격 횟수 초기화
        itemBox.ResetHitCount( );

        //추격 상태로 전환
        ChangeState( EnemyStateType.Chase );
    }

    /// <summary>
    /// 상자 개봉 시 추격 상태 전환
    /// </summary>
    public void KnowBoxOpened ( ItemBox itemBox )
    {
        //목표가 없으면 종료
        if ( FindTarget( ) == false ) return;
        //상자가 영역 안에 없으면 종료
        if ( IsItemBoxInRange( itemBox ) == false ) return;

        ChangeState( EnemyStateType.Chase );
    }
    #endregion

    #region ----- 피격 -----

    /// <summary>
    /// 스턴 상태로 변경
    /// </summary>
    public void Stunned ( float stunSpan )
    {
        //스턴 간격 설정
        _model.StunSpan = stunSpan;

        //누적 적 스턴 수 저장
        GameManager.Instance.SaveManager.AddEnemyStunCount( );

        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.Hit );

        //상태 전환
        ChangeState( EnemyStateType.Stun );
    }

    /// <summary>
    /// 리지드바디 constraints 설정
    /// </summary>
    /// <param name="constraints"></param>
    public void SetRigid ( RigidbodyConstraints2D constraints )
    {
        _rigid.constraints = constraints;
    }

    public void SetStun ( bool isStunned )
    {
        _model.IsStunned = isStunned;
    }
    #endregion

    #region ----- 공격 -----

    public void Attack ()
    {
        //발사 방향 구하기
        Vector2 dir = ( GetTargetPoint( ) - ( Vector2 ) _firePoint.position ).normalized;

        //투사체 복제 생성
        Projectile proj = Instantiate( _projectile, _firePoint.position, Quaternion.identity );

        //투사체 초기화
        proj.Init( dir, _projSpeed );
    }

    /// <summary>
    /// 투사체 목표 지점 반환
    /// </summary>
    /// <returns>목표 지점</returns>
    public Vector2 GetTargetPoint ()
    {
        //최대 거리 설정
        float maxDist = _model.TargetCheckRadius;

        //적이 바라보는 방향 랜덤 x 값
        float randomX = Random.Range( _projMinTargetDist, maxDist ) * Mathf.Sign( _dir );

        //랜덤 y 값 설정
        float randomY = Random.Range( _projMinTargetDist, maxDist );

        //목표 지점 설정
        Vector2 targetPos = ( Vector2 ) transform.position + new Vector2( randomX, randomY );

        return targetPos;
    }
    #endregion

    #region ----- 기즈모 -----
    private void OnDrawGizmos ()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere( transform.position, _model.TargetCheckRadius );

        //벽 감지
        if ( _wallSensor != null )
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine( _wallSensor.position, _wallSensor.position + Vector3.right * _dir * 1.0f );
        }
        //바닥 감지
        if ( _groundSensor != null )
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine( _groundSensor.position, _groundSensor.position + Vector3.down * 1.0f );
        }
    }
    #endregion
}

