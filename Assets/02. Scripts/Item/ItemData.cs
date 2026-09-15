using UnityEngine;

#region ----- 타입 -----

public enum ShapeType
{
    None,       //없음(운반 타입: 인벤토리)
    Circle,     //원
    Square,     //사각형
    Triangle,       //삼각형
}

public enum ValueType
{
    Normal,     //기본
    Rare,       //희귀
    Legend,     //전설
}

public enum CarryType
{
    Inventory,
    Interact,
}
#endregion

/// <summary>
/// 아이템 데이터
/// </summary>
[CreateAssetMenu ( menuName = "GameSettings/ItemData" )]
public class ItemData : ScriptableObject
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] string _name;      //이름
    [SerializeField] Sprite _iconSprite;        //아이콘

    [SerializeField] CarryType _carryType;      //운반 타입
    [SerializeField] float _weight;     //무게

    [SerializeField] ShapeType _shapeType;      //모양 타입
    [SerializeField] ValueType _valueType;      //가치 타입
    [SerializeField] ScoreData _scoreData;      //점수 데이터

    [SerializeField] float _spawnRate;      //스폰 확률


    #region ----- 프로퍼티 -----
    public string Name => _name;
    public Sprite IconSprite => _iconSprite;

    public float Weight => _weight;

    /// <summary>
    /// 점수(가치, 무게에 따른 점수 계산)
    /// </summary>
    public float Score => _scoreData.CalculateScore ( _valueType , _weight );
    public float SpawnRate => _spawnRate;

    public CarryType CarryType => _carryType;
    public ShapeType ShapeType => _shapeType;
    public ValueType ValueType => _valueType;
    #endregion

}
