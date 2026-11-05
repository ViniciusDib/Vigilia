using UnityEngine;

namespace Vigilia
{
    /// <summary>Câmera de cima que segue Elias com suavização e treme em sustos.</summary>
    public class CameraSegue : MonoBehaviour
    {
        public Transform alvo;
        public float suavidade = 6f;
        [Tooltip("Quanto a câmera se adianta na direção da mira.")]
        public float avancoMira = 1.2f;

        float tremor, tremorAte;

        public void Tremer(float intensidade, float duracao)
        {
            tremor = intensidade;
            tremorAte = Time.unscaledTime + duracao;
        }

        void LateUpdate()
        {
            if (alvo == null) return;
            Vector3 p = alvo.position;
            var j = Jogo.I;
            if (j != null && j.jogador != null) p += (Vector3)(j.jogador.Direcao * avancoMira);
            p.z = transform.position.z;
            transform.position = Vector3.Lerp(transform.position, p, 1f - Mathf.Exp(-suavidade * Time.unscaledDeltaTime));
            if (Time.unscaledTime < tremorAte)
                transform.position += (Vector3)(Random.insideUnitCircle * tremor);
        }

        /// <summary>Pula direto para o alvo (troca de dia/noite).</summary>
        public void Encaixar()
        {
            if (alvo == null) return;
            transform.position = new Vector3(alvo.position.x, alvo.position.y, transform.position.z);
        }
    }
}
