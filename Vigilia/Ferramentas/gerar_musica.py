"""
Trilha do VIGÍLIA, gerada por código (sem samples de terceiros: pode entrar nos créditos como do grupo).

  musica_dia.wav   caixinha de música desafinada sobre um pad grave (inquietante, mas calmo)
  musica_noite.wav drone grave com batimento, trítonos que crescem e sinos esparsos com eco
  musica_fuga.wav  pulso rápido de percussão, cordas em cluster dissonante e um tom que sobe sem parar

Uso:  python Ferramentas/gerar_musica.py      (de dentro da pasta Vigilia)
Saída: Assets/_Vigilia/Resources/Audio/*.wav  (o Unity importa sozinho)
"""
import os
import wave
import numpy as np

AQUI = os.path.dirname(os.path.abspath(__file__))
SAIDA = os.path.join(AQUI, "..", "Assets", "_Vigilia", "Resources", "Audio")
SR = 22050
rng = np.random.default_rng(3)


def tempo(seg):
    return np.arange(int(seg * SR)) / SR


def nota(semitom, base=220.0):
    return base * 2 ** (semitom / 12)


def env(n, ataque, decaimento):
    t = np.arange(n) / SR
    a = np.clip(t / max(ataque, 1e-4), 0, 1)
    return a * np.exp(-np.maximum(t - ataque, 0) / decaimento)


def sino(freq, seg, decai=1.6, brilho=1.0):
    """Sino / caixinha de música: parciais inarmônicas com decaimento."""
    t = tempo(seg)
    s = np.zeros_like(t)
    for mult, amp in ((1, 1.0), (2.76, 0.45 * brilho), (5.4, 0.22 * brilho), (8.93, 0.1 * brilho)):
        s += amp * np.sin(2 * np.pi * freq * mult * t) * np.exp(-t * mult / (decai * 1.2))
    return s * env(len(t), 0.003, decai)


def saw(freq, t, harm=10):
    s = np.zeros_like(t)
    for k in range(1, harm + 1):
        s += np.sin(2 * np.pi * freq * k * t) / k
    return s


def passa_baixa(x, corte):
    """Filtro passa-baixa de 1 polo (vetorizado o suficiente para alguns segundos)."""
    a = np.exp(-2 * np.pi * corte / SR)
    y = np.empty_like(x)
    acc = 0.0
    for i in range(len(x)):
        acc = (1 - a) * x[i] + a * acc
        y[i] = acc
    return y


def reverb(x, seg=2.8, mix=0.35, seed=1):
    """Reverb por convolução com ruído decaindo (rápido com FFT)."""
    r = np.random.default_rng(seed)
    n = int(seg * SR)
    ir = r.standard_normal(n) * np.exp(-np.arange(n) / SR * (6.9 / seg))
    ir[0] = 0
    ir /= np.sqrt(np.sum(ir ** 2))
    tam = len(x) + n
    nfft = 1 << (tam - 1).bit_length()
    y = np.fft.irfft(np.fft.rfft(x, nfft) * np.fft.rfft(ir, nfft), nfft)[:tam]
    # dobra o rabo do reverb no começo para o loop emendar
    cauda = y[len(x):]
    y = y[:len(x)].copy()
    y[:len(cauda)] += cauda
    return (1 - mix) * x + mix * y * 0.6


def colocar(buf, som, inicio):
    i = int(inicio * SR)
    if i >= len(buf):
        return
    j = min(len(buf), i + len(som))
    buf[i:j] += som[:j - i]


def normalizar(x, pico=0.85):
    return x / (np.max(np.abs(x)) + 1e-9) * pico


def laco(x, seg=0.4):
    """Funde o fim com o começo para o loop não estalar."""
    n = int(seg * SR)
    r = np.linspace(0, 1, n)
    y = x[:-n].copy()
    y[:n] = x[:n] * r + x[-n:] * (1 - r)
    return y


def comprimir(x, forca=2.2, pico=0.95):
    """Compressão suave: sobe as partes baixas sem estourar os picos (música mais presente)."""
    x = x / (np.max(np.abs(x)) + 1e-9)
    return np.tanh(x * forca) / np.tanh(forca) * pico


def salvar(nome, x):
    os.makedirs(SAIDA, exist_ok=True)
    x = comprimir(laco(x))
    x = np.clip(x, -1, 1)
    dados = (x * 32767).astype(np.int16)
    with wave.open(os.path.join(SAIDA, nome), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(dados.tobytes())
    print("ok:", nome, f"{len(x) / SR:.0f}s")


# ----------------------------------------------------------------------------- faixas

def musica_dia(seg=48.0):
    t = tempo(seg)
    # pad grave em lá menor, respirando devagar
    pad = (saw(nota(-24), t, 6) + 0.7 * saw(nota(-17), t, 6) + 0.5 * saw(nota(-12) * 1.003, t, 6))
    pad = passa_baixa(pad, 500) * (0.55 + 0.45 * np.sin(2 * np.pi * t / 16) ** 2) * 0.12
    # caixinha de música: canção de ninar em lá menor, levemente desafinada
    melodia = [0, 3, 7, 5, 3, 2, 3, None, 0, 3, 8, 7, 5, 3, 2, None, 7, 5, 3, 2, 0, -2, 0, None]
    caixa = np.zeros_like(t)
    passo = seg / 48
    for k in range(48):
        s = melodia[k % len(melodia)]
        if s is None:
            continue
        desafina = 1 + rng.normal(0, 0.006)
        colocar(caixa, sino(nota(s + 12) * desafina, 2.5, decai=1.1, brilho=0.7) * 0.5, k * passo)
    x = pad + caixa
    return normalizar(reverb(x, 3.0, 0.4), 0.6)


def musica_noite(seg=64.0):
    t = tempo(seg)
    # drone: lá e mi graves desafinados, com batimento lento
    drone = saw(nota(-36), t, 8) + 0.8 * saw(nota(-36) * 1.007, t, 8) + 0.5 * saw(nota(-29), t, 6)
    drone = passa_baixa(drone, 220) * 0.22
    # trítono que cresce a cada 16 s (ré#)
    swell = np.zeros_like(t)
    for k in range(int(seg // 16)):
        ini = k * 16 + 6
        n = int(8 * SR)
        tt = np.arange(n) / SR
        s = (np.sin(2 * np.pi * nota(-18) * tt) + 0.6 * np.sin(2 * np.pi * nota(-6) * 1.01 * tt))
        s *= np.sin(np.pi * tt / 8) ** 2 * 0.25
        colocar(swell, s, ini)
    # sinos esparsos e distantes
    sinos = np.zeros_like(t)
    escala = [0, 1, 3, 6, 7, 10, 12, 13]   # frígio com trítono
    tt = 2.0
    while tt < seg - 4:
        colocar(sinos, sino(nota(rng.choice(escala) + 12), 5.0, decai=2.5) * rng.uniform(0.15, 0.35), tt)
        tt += rng.uniform(3.0, 7.0)
    # batimento grave, bem baixo
    pulso = np.zeros_like(t)
    for k in range(int(seg / 1.6)):
        for off, amp in ((0, 1.0), (0.24, 0.6)):
            n = int(0.25 * SR)
            ttt = np.arange(n) / SR
            colocar(pulso, np.sin(2 * np.pi * 48 * ttt) * np.exp(-ttt * 18) * amp * 0.25, k * 1.6 + off)
    x = drone + swell + reverb(sinos, 4.0, 0.7, 2) + pulso
    return normalizar(reverb(x, 2.5, 0.25), 0.7)


def musica_fuga(seg=32.0):
    t = tempo(seg)
    bpm = 150
    batida = 60 / bpm
    perc = np.zeros_like(t)
    for k in range(int(seg / batida)):
        n = int(0.3 * SR)
        tt = np.arange(n) / SR
        bumbo = np.sin(2 * np.pi * (90 * np.exp(-tt * 12) + 40) * tt) * np.exp(-tt * 9)
        colocar(perc, bumbo * (1.0 if k % 2 == 0 else 0.6), k * batida)
        if k % 4 == 3:  # golpe metálico
            ruido = rng.standard_normal(int(0.2 * SR)) * np.exp(-np.arange(int(0.2 * SR)) / SR * 25)
            colocar(perc, ruido * 0.35, k * batida + batida / 2)
    # cordas: cluster dissonante em staccato (colcheias)
    cordas = np.zeros_like(t)
    cluster = [0, 1, 6, 7]
    for k in range(int(seg / (batida / 2))):
        n = int(batida / 2 * 0.8 * SR)
        tt = np.arange(n) / SR
        s = sum(saw(nota(c - 12), tt, 8) for c in cluster) * np.exp(-tt * 6) * 0.07
        colocar(cordas, s, k * batida / 2)
    cordas = passa_baixa(cordas, 2500)
    # tom que sobe sem parar (ilusão de Shepard simplificada)
    shep = np.zeros_like(t)
    ciclo = 8.0
    for oit in range(4):
        fase = (t / ciclo + oit / 4) % 1.0
        f = 110 * 2 ** (fase * 4)
        amp = np.sin(np.pi * fase) ** 2
        shep += np.sin(2 * np.pi * np.cumsum(f) / SR) * amp * 0.08
    x = perc * 0.9 + cordas + shep
    return normalizar(reverb(x, 1.6, 0.2), 0.85)


def main():
    salvar("musica_dia.wav", musica_dia())
    salvar("musica_noite.wav", musica_noite())
    salvar("musica_fuga.wav", musica_fuga())


if __name__ == "__main__":
    main()
