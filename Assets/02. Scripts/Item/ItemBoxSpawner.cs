using System.Collections;
using UnityEngine;

/// <summary>
/// 아이템 박스 스폰 위치별 리스폰 시간 설정
/// </summary>
[System.Serializable]
public class ItemBoxSpawnSetting
{
    public Transform SpawnPoint;        //스폰 위치
    public float RespawnTime;       //리스폰 시간
}

/// <summary>
/// 아이템 박스 스폰
/// </summary>
public class ItemBoxSpawner : MonoBehaviour
{
    [Header( "----- 프리팹 -----" )]
    [SerializeField] GameObject _itemBox;       //아이템 박스 프리팹

    [Header( "----- 스폰 -----" )]
    [SerializeField] ItemBoxSpawnSetting [ ] _spawnSettings;       //위치별 스폰 설정

    /// <summary>
    /// 아이템 박스를 스폰한다
    /// </summary>
    public void SpawnItemBox ()
    {
        //위치별 스폰 설정 존재 시 위치별 설정으로 스폰
        if ( _spawnSettings.Length > 0 )
        {
            for ( int i = 0; i < _spawnSettings.Length; i++ )
            {
                //위치별 리스폰 코루틴 시작
                StartCoroutine( RespawnRoutine(
                    _spawnSettings [ i ].SpawnPoint,
                    _spawnSettings [ i ].RespawnTime ) );
            }

            return;
        }
    }

    /// <summary>
    /// 아이템 박스를 반복해서 스폰
    /// </summary>
    /// <param name="spawnPoint">스폰 위치</param>
    /// <param name="respawnTime">리스폰 시간</param>
    IEnumerator RespawnRoutine ( Transform spawnPoint, float respawnTime )
    {
        while ( true )
        {
            //아이템 박스 생성
            GameObject itemBox = Instantiate( _itemBox, spawnPoint );

            //초기화
            itemBox.GetComponent<ItemBox>( ).Init( );

            //현재 아이템 박스가 사라질 때까지 대기
            yield return new WaitUntil( () => itemBox == null );

            //설정된 리스폰 시간만큼 대기
            yield return new WaitForSeconds( respawnTime );
        }
    }
}
