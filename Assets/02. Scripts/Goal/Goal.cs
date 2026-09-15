using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 목표 - 아이템 제출 처리
/// </summary>
public class Goal : MonoBehaviour
{
    [Header( "----- 런타임 데이터 -----" )]
    [SerializeField] float _currentScore;       //현재 점수

    public void Init ()
    {
        _currentScore = 0;
    }

    /// <summary>
    /// 점수 변경 이벤트(현재 점수)
    /// </summary>
    public event Action<float> OnScoreChanged;
    /// <summary>
    /// 아이템 제출 이벤트(제출 아이템 수)
    /// </summary>
    public event Action<int> OnItemSubmitted;

    /// <summary>
    /// 인벤토리 아이템 제출
    /// </summary>
    /// <param name="inventory">인벤토리 아이템</param>
    /// <returns>획득 점수</returns>
    public void SubmitInventoryItem ( Dictionary<ItemData, int> inventory )
    {
        float score = 0;
        int submittedCount = 0;

        foreach ( var item in inventory )
        {
            //제출 점수 계산
            score += item.Key.Score * item.Value;

            //제출 아이템 수 누적
            submittedCount += item.Value;
        }

        //현재 점수 처리
        _currentScore += score;

        //점수 변경 이벤트 발행
        OnScoreChanged?.Invoke( _currentScore );

        //제출 아이템 수 이벤트 발행
        OnItemSubmitted?.Invoke( submittedCount );
    }

    /// <summary>
    /// 상호작용 아이템 제출
    /// </summary>
    /// <param name="item">아이템</param>
    /// <returns>획득 점수</returns>
    public void SubmitInteractItem ( Item item )
    {
        //아이템 제출 처리
        //제출 실패 시 종료
        if ( item.Submit( ) == false ) return;

        //현재 점수 처리
        _currentScore += item.ItemScore;

        //점수 변경 이벤트 발행
        OnScoreChanged?.Invoke( _currentScore );

        //제출 아이템 수 이벤트 발행
        OnItemSubmitted?.Invoke( 1 );
    }
}
