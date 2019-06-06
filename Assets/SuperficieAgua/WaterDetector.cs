using UnityEngine;

public class WaterDetector : MonoBehaviour
{
    const float factorSplash = 0.03f;

    void OnTriggerEnter2D(Collider2D _col)
    {
        if (_col.GetComponent<Rigidbody2D>() != null)
        {
            //Efecto splash
            transform.parent.GetComponent<Water>().Splash(transform.position.x, _col.GetComponent<Rigidbody2D>().velocity.y * factorSplash);
            //Splash tomando la masa
            //transform.parent.GetComponent<Water>().Splash(transform.position.x, _col.GetComponent<Rigidbody2D>().velocity.y * _col.GetComponent<Rigidbody2D>().mass / 40f);
        }
    }

    //---------------Descomentar para hacer splash contario de salida
    /*void OnTriggerExit2D(Collider2D _col)
    {
        if (_col.rigidbody2D != null)
        {
            //verificamos si fue por arriba
            if (AMath.DirectionY(transform, _col.transform) > 0f) //Fue por arriba
            {
                transform.parent.GetComponent<Water>().Splash(transform.position.x, -_col.rigidbody2D.velocity.y * _col.rigidbody2D.mass / 40f);

                _col.rigidbody2D.gravityScale = _col.rigidbody2D.gravityScale / 2f;
                _col.rigidbody2D.velocity = new Vector2(_col.rigidbody2D.velocity.x, _col.rigidbody2D.velocity.y / 2f);
            }
        }
        
    }*/

}
