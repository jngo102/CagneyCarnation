using UnityEngine;
using Random = UnityEngine.Random;

namespace CagneyCarnation
{
    public class Pollen : MonoBehaviour
    {
        private const float VelocityYMax = 6;
        private const float VelocityYMin = -6;
        
        private float _velocityY;
        private bool _movingUp = true; 

        private Rigidbody2D _rb;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            _rb.velocity = new Vector2(_rb.velocity.x, Random.Range(VelocityYMin, VelocityYMax));
        }

        private void FixedUpdate()
        {
            Vector2 velocity = _rb.velocity;
            _velocityY = velocity.y;
            if (_movingUp)
            {
                if (_velocityY >= VelocityYMax)
                {
                    _movingUp = false;
                }
                else
                {
                    _velocityY += 0.25f;
                }
            }
            else if (!_movingUp)
            {
                if (_velocityY <= VelocityYMin)
                {
                    _movingUp = true;
                }
                else
                {
                    _velocityY -= 0.25f;
                }
            }
            
            _rb.velocity = new Vector2(velocity.x, _velocityY);
        }
    }
}