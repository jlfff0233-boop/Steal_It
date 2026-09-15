using UnityEngine;

/// <summary>
/// 남은 시간 추가 아이템
/// </summary>
public class BonusTimeItem : MonoBehaviour
{
    [Header ( "----- 설정 데이터 -----" )]
    [SerializeField] float _bonusTime;      //추가 시간

    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] Collider2D _collider;


    private void OnTriggerEnter2D ( Collider2D collision )
    {
        if ( collision.CompareTag ( "Player" ) )
        {
            //컴포넌트 가져오기
            Hero hero = collision.GetComponent<Hero> ( );

            if ( hero != null )
            {
                hero.GetBonusTime ( _bonusTime );

                //효과음 재생
                GameManager.Instance.SoundManager.PlaySfx( SfxType.Item );

                Destroy ( gameObject );
            }
        }
    }
}
