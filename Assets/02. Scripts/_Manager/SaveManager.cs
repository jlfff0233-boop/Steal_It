using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    [SerializeField] float _bestScore;        //최고 점수
    [SerializeField] int _clearCount;       //클리어 횟수

    [SerializeField] int _collectedItemTotalCount;      //아이템 누적 획득 수
    [SerializeField] int _submittedItemTotalCount;        //아이템 누적 제출 수

    [SerializeField] float _bonusTimeTotalAmount;       //보너스 시간 누적 수
    [SerializeField] int _bonusLifeTotalCount;      //보너스 목숨 누적 수
    [SerializeField] int _enemyStunTotalCount;        //에너미 스턴 누적 수
    [SerializeField] bool _hasSeenTutorial;      //튜토리얼 확인 여부

    public float BestScore => _bestScore;
    public int ClearCount => _clearCount;

    public int CollectedItemTotalCount => _collectedItemTotalCount;
    public int SubmittedItemTotalCount => _submittedItemTotalCount;

    public float BonusTimeTotalAmount => _bonusTimeTotalAmount;
    public int BonusLifeTotalCount => _bonusLifeTotalCount;
    public int EnemyStunTotalCount => _enemyStunTotalCount;
    public bool HasSeenTutorial => _hasSeenTutorial;


    public SaveData ()
    {

    }

    /// <summary>
    /// 최고 점수 설정
    /// </summary>
    /// <param name="score"></param>
    public void SetBestScore ( float score )
    {
        _bestScore = score;
    }

    /// <summary>
    /// 클리어 횟수 증가
    /// </summary>
    public void AddClearCount ()
    {
        _clearCount++;
    }

    /// <summary>
    /// 누적 획득 아이템 수 증가
    /// </summary>
    public void AddCollectedItemTotalCount ()
    {
        _collectedItemTotalCount++;
    }

    /// <summary>
    /// 누적 제출 아이템 수 증가
    /// </summary>
    public void AddSubmittedItemTotalCount ( int count )
    {
        _submittedItemTotalCount += count;
    }

    /// <summary>
    /// 누적 보너스 시간 증가
    /// </summary>
    public void AddBonusTimeTotalAmount ( float bonusTime )
    {
        _bonusTimeTotalAmount += bonusTime;
    }

    /// <summary>
    /// 누적 보너스 목숨 수 증가
    /// </summary>
    public void AddBonusLifeTotalCount ()
    {
        _bonusLifeTotalCount++;
    }

    /// <summary>
    /// 누적 적 스턴 수 증가
    /// </summary>
    public void AddEnemyStunTotalCount ()
    {
        _enemyStunTotalCount++;
    }

    /// <summary>
    /// 튜토리얼 확인 상태 설정
    /// </summary>
    public void SetHasSeenTutorial ()
    {
        _hasSeenTutorial = true;
    }
}

/// <summary>
/// 데이터 저장 관리
/// </summary>
public class SaveManager : MonoBehaviour
{
    [Header( "----- 세이브 데이터 -----" )]
    [SerializeField] SaveData _data;

    [Header( "----- Json 문자열 -----" )]
    [TextArea( 5, 10 )][SerializeField] string _json;

    [Header( "----- 저장 경로 -----" )]
    [SerializeField] string _saveFileName = "GameSaveData.json";

    string _savePath;       //저장 경로

    public float BestScore => _data.BestScore;
    public int ClearCount => _data.ClearCount;
    public int CollectedItemTotalCount => _data.CollectedItemTotalCount;
    public int SubmittedItemTotalCount => _data.SubmittedItemTotalCount;
    public float BonusTimeTotalAmount => _data.BonusTimeTotalAmount;
    public int BonusLifeTotalCount => _data.BonusLifeTotalCount;
    public int EnemyStunTotalCount => _data.EnemyStunTotalCount;
    public bool HasSeenTutorial => _data.HasSeenTutorial;
    public bool HasSaveData => File.Exists( GetSavePath( ) );


    public void Init ()
    {
        //저장 경로 설정
        GetSavePath( );

        //기본 데이터가 없으면 생성
        if ( _data == null )
        {
            _data = new SaveData( );
        }
    }

    /// <summary>
    /// 저장 경로 반환
    /// </summary>
    /// <returns>저장 경로</returns>
    string GetSavePath ()
    {
        //저장 경로가 없으면 생성
        if ( string.IsNullOrEmpty( _savePath ) == true )
        {
            _savePath = Path.Combine( Application.persistentDataPath, _saveFileName );
        }

        return _savePath;
    }


    /// <summary>
    /// Json 문자열 파일로 저장
    /// </summary>
    public void Save ()
    {
        //_data 변수가 가리키는 객체를 Json 문자열 형식으로 변환
        string json = JsonUtility.ToJson( _data );

        //파일로 저장
        File.WriteAllText( _savePath, json );
    }

    /// <summary>
    /// 저장 데이터 읽기
    /// </summary>
    /// <returns>저장 데이터</returns>
    public SaveData GetPreviewData ()
    {
        //세이브 파일이 없으면 빈 데이터 반환
        if ( File.Exists( _savePath ) == false )
        {
            return new SaveData( );
        }

        //세이브 파일 읽기
        string json = File.ReadAllText( _savePath );

        //저장 데이터 반환
        return JsonUtility.FromJson<SaveData>( json );
    }

    /// <summary>
    /// 불러오기
    /// </summary>
    public void Load ()
    {
        //세이브 파일 존재 여부 확인
        if ( File.Exists( _savePath ) == false )
        {
            Debug.Log( "세이브 파일이 없습니다." );
            return;
        }

        //세이브 파일에서 문자열 읽어 오기
        string json = File.ReadAllText( _savePath );

        //Json 문자열을 객체로 변환
        _data = JsonUtility.FromJson<SaveData>( json );
        Debug.Log( $"불러오기 완료!\n{json}" );
    }

    /// <summary>
    /// 기록 삭제
    /// </summary>
    public void DeleteData ()
    {
        //기록 있으면 삭제
        if ( File.Exists( _savePath ) == true )
        {
            File.Delete( _savePath );
        }
        
        //새 데이터 생성
        _data = new SaveData( );
    }

    /// <summary>
    /// 최고 기록 및 누적 기록 저장
    /// </summary>
    /// <param name="score">현재 점수</param>
    /// <param name="submittedItemCount">이번 판 제출 아이템 수</param>
    /// <param name="isClear">클리어 여부</param>
    public void SaveBestResult ( float score, int submittedItemCount, bool isClear )
    {
        //현재 점수가 최고 점수보다 높으면 최고 점수 갱신
        if ( score > _data.BestScore )
        {
            _data.SetBestScore( score );
        }

        //클리어 시 클리어 횟수 증가
        if ( isClear == true )
        {
            _data.AddClearCount( );
        }

        //누적 제출 아이템 수 증가
        _data.AddSubmittedItemTotalCount( submittedItemCount );

        //파일로 저장
        Save( );
    }

    /// <summary>
    /// 누적 획득 아이템 수 증가
    /// </summary>
    public void AddCollectedItemCount ()
    {
        _data.AddCollectedItemTotalCount( );
    }

    /// <summary>
    /// 누적 보너스 시간 증가
    /// </summary>
    public void AddBonusTimeAmount ( float bonusTime )
    {
        _data.AddBonusTimeTotalAmount( bonusTime );
    }

    /// <summary>
    /// 누적 보너스 목숨 수 증가
    /// </summary>
    public void AddBonusLifeCount ()
    {
        _data.AddBonusLifeTotalCount( );
    }

    /// <summary>
    /// 누적 적 스턴 수 증가
    /// </summary>
    public void AddEnemyStunCount ()
    {
        _data.AddEnemyStunTotalCount( );
    }

    /// <summary>
    /// 튜토리얼 확인 상태 저장
    /// </summary>
    public void SetHasSeenTutorial ()
    {
        _data.SetHasSeenTutorial( );
    }
}
