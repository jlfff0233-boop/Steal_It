using TMPro;
using UnityEngine;

/// <summary>
/// 클리어씬뷰 - UI 갱신
/// </summary>
public class ClearSceneView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] TMP_Text _bestScoreText;       //최고 점수 텍스트
    [SerializeField] TMP_Text _clearCountText;      //클리어 횟수 텍스트
    [SerializeField] TMP_Text _collectedItemCountText;      //아이템 누적 획득 수
    [SerializeField] TMP_Text _submittedItemCountText;      //아이템 누적 제출 수
    [SerializeField] TMP_Text _bonusTimeText;       //누적 보너스 시간
    [SerializeField] TMP_Text _bonusLifeText;       //누적 보너스 목슴
    [SerializeField] TMP_Text _enemyStunCountText;      //적 누적 스턴 수


    /// <summary>
    /// 최고 점수 텍스트 갱신
    /// </summary>
    /// <param name="score"></param>
    public void UpdateBestScore ( float score )
    {
        _bestScoreText.text = $"최고 점수: {score}";
    }

    /// <summary>
    /// 클리어 횟수 텍스트 갱신
    /// </summary>
    /// <param name="count">클리어 횟수</param>
    public void UpdateClearCount ( int count )
    {
        _clearCountText.text = $"클리어 횟수: {count}";
    }

    /// <summary>
    /// 누적 획득 아이템 수 갱신
    /// </summary>
    /// <param name="count">누적 획득 아이템 수</param>
    public void UpdateCollectedItemCount ( int count )
    {
        _collectedItemCountText.text = $"아이템 누적 획득 수: {count}";
    }

    /// <summary>
    /// 누적 제출 아이템 수 갱신
    /// </summary>
    /// <param name="count">누적 제출 아이템 수</param>
    public void UpdateSubmittedItemCount ( int count )
    {
        _submittedItemCountText.text = $"아이템 누적 제출 수: {count}";
    }

    /// <summary>
    /// 누적 보너스 시간 갱신
    /// </summary>
    /// <param name="time">누적 보너스 시간</param>
    public void UpdateBonusTime ( float time )
    {
        _bonusTimeText.text = $"누적 보너스 시간: {time:F1}초";
    }

    /// <summary>
    /// 누적 보너스 목숨 수 갱신
    /// </summary>
    /// <param name="count">누적 보너스 목숨 수</param>
    public void UpdateBonusLifeCount ( int count )
    {
        _bonusLifeText.text = $"누적 보너스 목숨 수: {count}";
    }

    /// <summary>
    /// 누적 적 스턴 수 갱신
    /// </summary>
    /// <param name="count">누적 적 스턴 수</param>
    public void UpdateEnemyStunCount ( int count )
    {
        _enemyStunCountText.text = $"누적 적 스턴 횟수: {count}";
    }

    /// <summary>
    /// 클리어 기록 전체 갱신
    /// </summary>
    /// <param name="bestScore">최고 점수</param>
    /// <param name="clearCount">클리어 횟수</param>
    /// <param name="collectedItemCount">누적 획득 아이템 수</param>
    /// <param name="submittedItemCount">누적 제출 아이템 수</param>
    /// <param name="bonusTime">누적 보너스 시간</param>
    /// <param name="bonusLifeCount">누적 보너스 목숨 수</param>
    /// <param name="enemyStunCount">누적 적 스턴 수</param>
    public void UpdateAllRecord (
        float bestScore, int clearCount,
        int collectedItemCount, int submittedItemCount,
        float bonusTime, int bonusLifeCount, int enemyStunCount )
    {
        //최고 점수 갱신
        UpdateBestScore( bestScore );

        //클리어 횟수 갱신
        UpdateClearCount( clearCount );

        //누적 획득 아이템 수 갱신
        UpdateCollectedItemCount( collectedItemCount );

        //누적 제출 아이템 수 갱신
        UpdateSubmittedItemCount( submittedItemCount );

        //누적 보너스 시간 갱신
        UpdateBonusTime( bonusTime );

        //누적 보너스 목숨 수 갱신
        UpdateBonusLifeCount( bonusLifeCount );

        //누적 적 스턴 수 갱신
        UpdateEnemyStunCount( enemyStunCount );
    }
}
