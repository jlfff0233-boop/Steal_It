using UnityEngine;

/// <summary>
/// 투사체 - 공격, 파괴
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header( "----- 설정 데이터 -----" )]
    [SerializeField] Vector2 _dir;
    [SerializeField] float _moveSpeed;
    [SerializeField] float _span;
    [SerializeField] float _timer;

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Rigidbody2D _rigid;
    [SerializeField] Collider2D _collider;


    public void Init ( Vector2 dir, float moveSpeed )
    {
        _dir = dir;
        _moveSpeed = moveSpeed;

        Move( );
    }

    private void Update ()
    {
        _timer += Time.deltaTime;

        if ( _timer >= _span )
        {
            Destroy( gameObject );
        }

    }

    public void Move ()
    {
        _rigid.linearVelocity = _dir * _moveSpeed;
    }

    private void OnTriggerEnter2D ( Collider2D collision )
    {
        //땅에 닿으면 파괴
        if ( collision.CompareTag( "Ground" ) )
        {
            Destroy( gameObject );
        }

        if ( collision.CompareTag( "Player" ) )
        {
            Hero target = collision.GetComponent<Hero>( );

            if ( target != null )
            {
                //공격
                target.LoseLives( );

                //파괴
                Destroy( gameObject );
            }
        }
    }
}
