using UnityEngine;
using Mirror;

namespace Ignorance.Examples.PongChamp
{
    public class AtariPongBall : NetworkBehaviour
    {
        public float speed = 100;
        private Rigidbody2D rigidbody2d;

        private void Awake()
        {
            rigidbody2d = GetComponent<Rigidbody2D>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            // only simulate ball physics on server
            rigidbody2d.simulated = true;

            // Serve the ball from left player
            rigidbody2d.velocity = Vector2.right * speed;
        }

        float HitFactor(Vector2 ballPos, Vector2 racketPos, float racketHeight)
        {
            // ascii art:
            // ||  1 <- at the top of the racket
            // ||
            // ||  0 <- at the middle of the racket
            // ||
            // || -1 <- at the bottom of the racket
            return (ballPos.y - racketPos.y) / racketHeight;
        }

        // only call this on server
        [ServerCallback]
        void OnCollisionEnter2D(Collision2D col)
        {
            // Note: 'col' holds the collision information. If the
            // Ball collided with a racket, then:
            //   col.gameObject is the racket
            //   col.transform.position is the racket's position
            //   col.collider is the racket's collider

            // did we hit a racket? then we need to calculate the hit factor
            if (col.transform.GetComponent<AtariPongBall>())
            {
                // Calculate y Snapshots via hit Factor
                float y = HitFactor(transform.position,
                                    col.transform.position,
                                    col.collider.bounds.size.y);

                // Calculate x Snapshots via opposite collision
                float x = col.relativeVelocity.x > 0 ? 1 : -1;

                // Calculate Snapshots, make length=1 via .normalized
                Vector2 dir = new Vector2(x, y).normalized;

                // Set Velocity with dir * speed
                rigidbody2d.velocity = dir * speed;
            }
        }
    }
}
