using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임이 시작될 때 적을 소환한다
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header( "----- 히어로 -----" )]
    [SerializeField] Hero _hero;        //히어로

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Transform [ ] _spawnPoints;        //스폰 포인트 배열

    [Header( "----- 프리팹 -----" )]
    [SerializeField] Enemy [ ] _prefabs;       //에너미 프리팹 배열

    [Header( "----- 런타임 데이터 -----" )]
    [SerializeField] List<Enemy> _enemies = new List<Enemy>( );       //생성된 적 목록

    /// <summary>
    /// 생성된 적 제거
    /// </summary>
    public void RemoveEnemies ()
    {
        for ( int i = 0; i < _enemies.Count; i++ )
        {

            //생성된 적 제거
            Destroy( _enemies [ i ].gameObject );
        }

        //목록 초기화
        _enemies.Clear( );
    }

    /// <summary>
    /// 모든 스폰 위치에 랜덤 에너미 스폰
    /// </summary>
    public void SpawnEnemy ()
    {
        foreach ( Transform point in _spawnPoints )
        {
            //랜덤 인덱스
            int randomIndex = Random.Range( 0, _prefabs.Length );

            //적 복제 생성
            Enemy enemy = Instantiate( _prefabs [ randomIndex ], point );

            //생성된 적 목록에 추가
            _enemies.Add( enemy );

            //적 초기화
            enemy.Init( _hero );
        }
    }
}
