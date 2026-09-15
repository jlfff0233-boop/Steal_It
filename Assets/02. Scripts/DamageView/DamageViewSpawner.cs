using UnityEngine;

/// <summary>
/// 대미지 뷰 스포너
/// </summary>
public class DamageViewSpawner : MonoBehaviour
{
    [Header( "----- 프리팹 -----" )]
    [SerializeField] DamageView _prefab;


    public void SpawnDamageView ( float damage )
    {
        DamageView damageView = Instantiate( _prefab, transform );

        damageView.SetDamageText( damage, Vector2.zero );
    }
}
