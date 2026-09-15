using UnityEngine;

public enum EnemyStateType
{
    Normal,     //기본
    Chase,      //추격
    Stun,       //스턴
    Attack,     //공격

    Count,      //개수
}

/// <summary>
/// 적 상태 인터페이스
/// </summary>
public interface IEnemyState
{
    /// <summary>
    /// 상태 종류
    /// </summary>
    EnemyStateType StateType { get; }

    /// <summary>
    /// 상태 시작
    /// </summary>
    void Enter ();

    /// <summary>
    /// 상태 유지
    /// </summary>
    void Update ();

    /// <summary>
    /// 상태 종료
    /// </summary>
    void Exit ();
}

/// <summary>
/// 기본 상태
/// </summary>
public class NormalState : IEnemyState
{
    public EnemyStateType StateType => EnemyStateType.Normal;

    Enemy _enemy;
    float _attackSpan;
    float _timer;

    public NormalState ( Enemy enemy, float attackSpan )
    {
        _enemy = enemy;
        _attackSpan = attackSpan;
    }

    public void Enter ()
    {
        //타이머 초기화
        _timer = 0;
    }

    public void Exit ()
    {

    }

    public void Update ()
    {
        _timer += Time.deltaTime;

        //순찰(기본 이동)
        _enemy.HandlePatrol( );

        //공격 타이머 체크
        if ( _timer > _attackSpan )
        {
            //공격 상태 전환
            _enemy.ChangeState( EnemyStateType.Attack );
        }
    }
}

/// <summary>
/// 추격 상태
/// </summary>
public class ChaseState : IEnemyState
{
    public EnemyStateType StateType => EnemyStateType.Chase;

    Enemy _enemy;
    float _chaseSpeed;
    float _timer;

    public ChaseState ( Enemy enemy, float chaseSpeed )
    {
        _enemy = enemy;
        _chaseSpeed = chaseSpeed;
    }

    public void Enter ()
    {
        //타이머 초기화
        _timer = 0;

        //이동 속도 변경
        _enemy.ChangeMoveSpeed( _chaseSpeed );

        //Debug.Log( "추격 시작" );     //미사용: 추격 시작 디버그 로그
    }

    public void Exit ()
    {
        //이동 속도 복구
        _enemy.ChangeMoveSpeed( _enemy.OriginalMoveSpeed );

        //Debug.Log( "추격 종료" );     //미사용: 추격 종료 디버그 로그
    }

    public void Update ()
    {
        //타이머 갱신
        _timer += Time.deltaTime;

        //목표가 감지 영역을 나가면 기본 상태로 전환 후 종료
        if ( _enemy.FindTarget( ) == false )
        {
            _enemy.ChangeState( EnemyStateType.Normal );
            return;
        }

        //추격 지속 시간이 끝나면 종료
        if ( _timer >= _enemy.ChaseDuration )
        {
            _enemy.ChangeState( EnemyStateType.Normal );
            return;
        }

        //추격 중 접촉 시 아이템 몰수 후 종료
        if ( _enemy.CatchTarget( ) == true )
        {
            _enemy.TakeAllItems( );
            _enemy.ChangeState( EnemyStateType.Normal );
            return;
        }

        //추격 방향 가져오기
        float dir = _enemy.GetTargetDirection( );
        _enemy.Move( dir );
    }
}


/// <summary>
/// 스턴 상태
/// </summary>
public class StunState : IEnemyState
{
    public EnemyStateType StateType => EnemyStateType.Stun;

    Enemy _enemy;
    float _stunSpan;
    float _stunTimer;

    public StunState ( Enemy enemy, float stunSpan )
    {
        _enemy = enemy;
        _stunSpan = stunSpan;
    }

    public void Enter ()
    {
        //타이머 초기화
        _stunTimer = 0;

        //스턴 상태 처리
        _enemy.SetStun( true );

        //스턴 간격 가져오기
        _stunSpan = _enemy.StunSpan;

        //속도 초기화
        _enemy.ChangeMoveSpeed( 0 );

        //밀려나지 않게 하기
        _enemy.SetRigid( RigidbodyConstraints2D.FreezeAll );

        //애니메이션 처리
        _enemy.Anim.Play( "Idle" );
    }

    public void Exit ()
    {

    }

    public void Update ()
    {
        _stunTimer += Time.deltaTime;

        //스턴 종료 시
        if ( _stunTimer >= _stunSpan )
        {
            //스턴 상태 처리
            _enemy.SetStun( false );

            //이동 속도 복구
            _enemy.ChangeMoveSpeed( _enemy.OriginalMoveSpeed );

            //리지드바디 Constraints 설정 복구
            _enemy.SetRigid( RigidbodyConstraints2D.FreezeRotation );

            //기본 상태로 전환
            _enemy.ChangeState( EnemyStateType.Normal );

            return;
        }
    }
}

public class AttackState : IEnemyState
{
    public EnemyStateType StateType => EnemyStateType.Attack;

    Enemy _enemy;

    public AttackState ( Enemy enemy )
    {
        _enemy = enemy;
    }

    public void Enter ()
    {
        //애니메이터 파라미터 설정
        _enemy.Anim.SetTrigger( "Attack" );

        //공격
        _enemy.Attack( );

        //기본 상태로 전환
        _enemy.ChangeState( EnemyStateType.Normal );
    }

    public void Exit ()
    {

    }

    public void Update ()
    {

    }
}
