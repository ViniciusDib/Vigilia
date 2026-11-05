using UnityEngine;

namespace Vigilia
{
    /// <summary>Marca onde Elias começa: "dia", "noite1", "noite2", "noite3". Mova no editor para mudar.</summary>
    public class PontoDeInicio : MonoBehaviour
    {
        public string id = "dia";

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
        }
    }
}
