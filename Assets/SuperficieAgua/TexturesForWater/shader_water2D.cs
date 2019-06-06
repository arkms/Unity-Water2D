using UnityEngine;

public class shader_water2D : MonoBehaviour
{
    static bool AlreadyInit;

	void Start ()
	{
        Renderer renderer_ = GetComponent<Renderer>();

        //Aseguramos que solo sea una vez, porque modifcamos el material original
        if (AlreadyInit) return;
        AlreadyInit = true;

        //Generamos una textura procesural para el shader del agua //Crea que cada ejecutada no sea siempre igual como se ve el agua
        Material mat = renderer_.sharedMaterial;
        Texture2D text_Noise = new Texture2D(256, 256);
        Color[] pix= new Color[256 * 256];

        float y = 0.0f;
        while (y < text_Noise.height)
        {
            float x = 0.0f;
            while (x < text_Noise.width)
            {
                float xCoord = x / text_Noise.width * 3f; //Podemos cambiar el 5 para que tan marcado es el ruido
                float yCoord = y / text_Noise.height * 3f;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                pix[(int)(y * text_Noise.width + x)] = new Color(sample, sample, sample);
                x++;
            }
            y++;
        }
        text_Noise.SetPixels(pix);
        text_Noise.Apply(); //guardamos textura
        //asginamos
        mat.SetTexture("_NoiseTex", text_Noise);
    }

    void OnDestroy()
    {
        if(AlreadyInit)
        {
            GetComponent<Renderer>().sharedMaterial.SetTexture("_NoiseTex", null); //regresamos a normal
        }
        AlreadyInit = false;
    }
}
