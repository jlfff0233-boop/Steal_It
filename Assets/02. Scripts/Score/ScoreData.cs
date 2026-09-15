using UnityEngine;

/// <summary>
/// 점수 데이터
/// </summary>
[CreateAssetMenu( menuName = "GameSettings/ScoreData" )]
public class ScoreData : ScriptableObject
{
    [Header( "----- 설정 데이터 -----" )]
    [SerializeField] float _baseScore;      //기본 점수

    [SerializeField] float [ ] _valueMults;      //가치 배율 배열
    [SerializeField] float [ ] _maxWeights;      //타입별 무게 최대치 배열
    [SerializeField] float [ ] _weightMults;     //무게 배율 배열


    /// <summary>
    /// 가치, 무게에 따른 점수 계산
    /// </summary>
    /// <param name="weight">무게</param>
    /// <returns>점수</returns>
    public float CalculateScore ( ValueType valueType, float weight )
    {
        return _baseScore * GetValueMult( valueType ) * GetWeightMult( weight );
    }

    /// <summary>
    /// 가치별 배율 반환
    /// </summary>
    /// <param name="valueType">가치 타입</param>
    /// <returns>가치 배율</returns>
    public float GetValueMult ( ValueType valueType )
    {
        //인덱스 계산
        int index = ( int ) valueType;

        //배율 배열 범위 밖이면 기본 배율 반환
        if ( index < 0 || index >= _valueMults.Length ) return 1;

        return _valueMults [ index ];
    }

    /// <summary>
    /// 무게 구간에 따른 배율 반환(소, 중, 대)
    /// </summary>
    /// <param name="weight">무게</param>
    /// <returns>무게 배율</returns>
    public float GetWeightMult ( float weight )
    {
        for ( int i = 0; i < _maxWeights.Length; i++ )
        {
            //무게 배율 배열 범위를 넘으면 기본 배율 반환
            if ( i >= _weightMults.Length ) return 1;

            //weight가 _maxWeights[i]보다 같거나 작으면
            if ( weight <= _maxWeights [ i ] )
            {
                return _weightMults [ i ];
            }
        }

        //무게 배율이 없으면 기본 배율 반환
        if ( _weightMults.Length == 0 ) return 1;

        //마지막 요소 반환
        return _weightMults [ _weightMults.Length - 1 ];
    }
}
