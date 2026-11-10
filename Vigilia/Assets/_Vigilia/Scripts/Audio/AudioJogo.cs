using System.Collections.Generic;
using UnityEngine;

namespace Vigilia
{
    public enum SomId { Passo, Lanterna, Pegar, Porta, Pancada, Toque, Esconder, Escrever, Sussurro, Transbordo, Dialogo, Caldeira, Stinger }

    /// <summary>
    /// Som do jogo. Camadas em laço que crescem com a possessão (sugestão do Q-12):
    /// vento (sempre à noite), batimento (volume e ritmo sobem com possessao, P-10) e sussurros
    /// (sobem com possessao e com um possuído perto). Efeitos curtos por Tocar(SomId).
    /// Para trocar por sons reais, preencha os campos de clipe no Inspector: os gerados só entram se o campo estiver vazio.
    /// </summary>
    public class AudioJogo : MonoBehaviour
    {
        [Header("Clipes (vazio = gerado por código)")]
        public AudioClip vento;
        public AudioClip batimento;
        public AudioClip sussurro;
        public AudioClip passo, lanterna, pegar, porta, pancada, toque, esconder, escrever, transbordo, dialogo, caldeira, stinger;

        [Header("Música (Resources/Audio, gerada por Ferramentas/gerar_musica.py)")]
        public AudioClip musicaDia;
        public AudioClip musicaNoite;
        public AudioClip musicaFuga;

        AudioSource fonteVento, fonteBatimento, fonteSussurro, efeitos;
        AudioSource musicaA, musicaB;   // duas fontes para trocar de faixa com fade cruzado
        AudioClip faixaAtual;
        readonly Dictionary<SomId, AudioClip> mapa = new Dictionary<SomId, AudioClip>();
        bool camadasAtivas = true;

        void Awake()
        {
            if (vento == null) vento = SomProcedural.Vento(8f);
            if (batimento == null) batimento = SomProcedural.Batimento();
            if (sussurro == null) sussurro = SomProcedural.Sussurro(4f, 7);
            if (passo == null) passo = SomProcedural.Passo();
            if (lanterna == null) lanterna = SomProcedural.Clique();
            if (pegar == null) pegar = SomProcedural.Tom(880f, 0.25f, 0.25f, "pegar");
            if (porta == null) porta = SomProcedural.Tom(160f, 0.5f, 0.4f, "porta");
            if (pancada == null) pancada = SomProcedural.Pancada();
            if (toque == null) toque = SomProcedural.Stinger();
            if (esconder == null) esconder = SomProcedural.Passo();
            if (escrever == null) escrever = SomProcedural.Escrita();
            if (transbordo == null) transbordo = SomProcedural.Stinger();
            if (dialogo == null) dialogo = SomProcedural.Tom(440f, 0.08f, 0.15f, "dialogo");
            if (caldeira == null) caldeira = SomProcedural.Caldeira();
            if (stinger == null) stinger = SomProcedural.Stinger();

            mapa[SomId.Passo] = passo; mapa[SomId.Lanterna] = lanterna; mapa[SomId.Pegar] = pegar;
            mapa[SomId.Porta] = porta; mapa[SomId.Pancada] = pancada; mapa[SomId.Toque] = toque;
            mapa[SomId.Esconder] = esconder; mapa[SomId.Escrever] = escrever; mapa[SomId.Sussurro] = sussurro;
            mapa[SomId.Transbordo] = transbordo; mapa[SomId.Dialogo] = dialogo; mapa[SomId.Caldeira] = caldeira;
            mapa[SomId.Stinger] = stinger;

            fonteVento = Laco(vento);
            fonteBatimento = Laco(batimento);
            fonteSussurro = Laco(sussurro);
            if (musicaDia == null) musicaDia = Resources.Load<AudioClip>("Audio/musica_dia");
            if (musicaNoite == null) musicaNoite = Resources.Load<AudioClip>("Audio/musica_noite");
            if (musicaFuga == null) musicaFuga = Resources.Load<AudioClip>("Audio/musica_fuga");
            musicaA = Laco(null);
            musicaB = Laco(null);
            efeitos = gameObject.AddComponent<AudioSource>();
        }

        AudioSource Laco(AudioClip c)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.clip = c; s.loop = true; s.volume = 0f; s.Play();
            return s;
        }

        public void Tocar(SomId id)
        {
            if (!mapa.TryGetValue(id, out var c) || c == null) return;
            float vol = id == SomId.Passo ? Jogo.Cfg.volumePassos : id == SomId.Toque || id == SomId.Stinger || id == SomId.Transbordo ? 0.9f : 0.6f;
            efeitos.PlayOneShot(c, vol);
        }

        public void PararCamadas()
        {
            camadasAtivas = false;
            fonteVento.volume = fonteBatimento.volume = fonteSussurro.volume = 0f;
        }

        /// <summary>Escolhe a faixa (dia, noite, fuga) e faz fade cruzado quando ela muda.</summary>
        void AtualizarMusica(Jogo j)
        {
            AudioClip alvo = j.noites != null && j.noites.EmFuga ? musicaFuga : j.EhNoite ? musicaNoite : musicaDia;
            if (alvo != faixaAtual && alvo != null)
            {
                faixaAtual = alvo;
                (musicaA, musicaB) = (musicaB, musicaA);       // A passa a ser a nova
                musicaA.clip = alvo;
                musicaA.volume = 0f;
                musicaA.Play();
            }
            float vol = Jogo.Cfg.volumeMusica;
            if (j.encontro != null && j.encontro.Tocando) vol *= 0.15f;   // o susto fica sozinho
            if (!camadasAtivas) vol *= 0.6f;                               // tela final: mais baixo
            float k = Time.unscaledDeltaTime * 0.8f;
            musicaA.volume = Mathf.MoveTowards(musicaA.volume, vol, k);
            musicaB.volume = Mathf.MoveTowards(musicaB.volume, 0f, k);
            if (musicaB.volume <= 0f && musicaB.isPlaying) musicaB.Stop();
        }

        void Update()
        {
            var j = Jogo.I;
            if (j == null) return;
            AtualizarMusica(j);
            if (!camadasAtivas || j.possessao == null) return;
            float nivel = j.possessao.Nivel01;
            bool noite = j.EhNoite;
            float k = Time.deltaTime * 2f;

            fonteVento.volume = Mathf.Lerp(fonteVento.volume, noite ? 0.45f : 0.15f, k);
            fonteBatimento.volume = Mathf.Lerp(fonteBatimento.volume, noite ? Mathf.SmoothStep(0f, 0.9f, Mathf.InverseLerp(0.25f, 1f, nivel)) : 0f, k);
            fonteBatimento.pitch = Mathf.Lerp(0.9f, 1.9f, nivel);

            float perto = j.jogador != null ? EnemyAI.DistanciaPerseguidorMaisProximo(j.jogador.Posicao) : 99f;
            float proximidade = Mathf.InverseLerp(8f, 1.5f, perto);
            float alvoSussurro = noite ? Mathf.Clamp01(proximidade * 0.6f + Mathf.InverseLerp(0.5f, 1f, nivel) * 0.5f) : 0f;
            fonteSussurro.volume = Mathf.Lerp(fonteSussurro.volume, alvoSussurro, k);
        }
    }
}
