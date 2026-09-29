"""Create PrisM's original audio. Requires Python 3 + numpy; no source samples.

Run from any directory. The seed, arrangement and import GUIDs are deterministic.
"""
from pathlib import Path
import json
import uuid
import wave
import numpy as np

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets/Prism/Resources/Audio"
RNG = np.random.default_rng(27092026)


def midi(note):
    return 440.0 * 2.0 ** ((note - 69) / 12.0)


def save(name, data, rate, loop=False):
    data = np.asarray(data, dtype=np.float64)
    peak = float(np.max(np.abs(data)))
    assert np.isfinite(data).all() and 0 < peak < .8, (name, peak)
    assert abs(float(np.mean(data))) < .002, name
    if loop:
        seam = np.max(np.abs(data[0] - data[-1]))
        assert seam <= np.max(np.abs(np.diff(data, axis=0))) * 1.05, name
    else:
        assert np.max(np.abs(data[[0, -1]])) < 1e-8, name
    pcm = np.round(data * 32767).astype("<i2")
    path = OUT / (name + ".wav")
    with wave.open(str(path), "wb") as file:
        file.setnchannels(1 if data.ndim == 1 else data.shape[1])
        file.setsampwidth(2)
        file.setframerate(rate)
        file.writeframes(pcm.tobytes())
    guid = uuid.uuid5(uuid.NAMESPACE_URL, "prism-original-audio/" + name).hex
    path.with_suffix(".wav.meta").write_text(f"""fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 7
  defaultSettings:
    serializedVersion: 2
    loadType: {2 if loop else 0}
    sampleRateSetting: 0
    sampleRateOverride: {rate}
    compressionFormat: {1 if loop else 0}
    quality: 0.65
    conversionMode: 0
    preloadAudioData: 1
  platformSettingOverrides: {{}}
  forceToMono: {0 if loop else 1}
  normalize: 0
  loadInBackground: {1 if loop else 0}
  ambisonic: 0
  3D: 0
  userData: PrisM original deterministic synthesis
  assetBundleName:
  assetBundleVariant:
""", encoding="utf-8")
    return dict(name=name, seconds=len(data)/rate, channels=1 if data.ndim==1 else 2,
                peak=round(peak, 6), rms=round(float(np.sqrt(np.mean(data**2))), 6),
                bytes=path.stat().st_size,
                loop_boundary_delta=round(float(np.max(np.abs(data[0]-data[-1]))), 8))


def bell(t, frequency, decay, amplitude):
    attack = np.minimum(1, t/.004)
    return amplitude*attack*np.exp(-t/decay)*(np.sin(2*np.pi*frequency*t)
        + .18*np.sin(2*np.pi*frequency*2.76*t)*np.exp(-t/.07)
        + .06*np.sin(2*np.pi*frequency*5.4*t)*np.exp(-t/.025))


def sfx(name, seconds, notes, tap=0):
    rate = 44100
    t = np.arange(round(seconds*rate))/rate
    audio = np.zeros_like(t)
    for start, note, decay, amplitude in notes:
        local = np.maximum(0, t-start)
        audio += bell(local, midi(note), decay, amplitude)*(t>=start)
    if tap:
        noise = RNG.normal(0, 1, len(t))
        noise = np.convolve(noise, np.ones(9)/9, mode="same")
        audio += tap*noise*np.exp(-t/.008)*np.minimum(1,t/.001)
    audio *= np.minimum(1,t/.001)*np.minimum(1,(t[-1]-t)/.02)
    return save(name, audio, rate)


def ambience():
    rate, duration = 32000, 48
    t = np.arange(rate*duration)/rate
    stereo = np.zeros((len(t), 2))
    # Overlapping, slowly changing D-major / B-minor-family voicings.
    chords = [(50,57,61,64,66), (47,54,57,62,66), (43,50,54,57,62),
              (45,52,57,59,64), (40,47,54,55,62), (45,52,57,62,64)]
    for index, chord in enumerate(chords):
        distance = (t-index*8+duration/2)%duration-duration/2
        weight = np.where(np.abs(distance)<6, np.cos(np.pi*np.clip(distance,-6,6)/12)**2, 0)
        for voice, note in enumerate(chord):
            # Integer cycle counts ensure oscillator phases match across the seam.
            frequency = round(midi(note)*duration)/duration
            phase = 2*np.pi*frequency*t + voice*.7
            drift = 1+.08*np.sin(2*np.pi*(voice+1)*t/duration+index)
            pad = (np.sin(phase)+.16*np.sin(2*phase)+.035*np.sin(3*phase))*weight*drift*.018
            pan = .28+.11*voice
            stereo[:,0] += pad*np.sqrt(1-pan)
            stereo[:,1] += pad*np.sqrt(pan)
    # Sparse glass reflections with circular tails, no beat or repeated ostinato.
    for start, note, pan in [(3,74,.25),(11,76,.7),(21,69,.35),(29,78,.65),(38,71,.2),(45,76,.75)]:
        age = (t-start)%duration
        envelope = (1-np.exp(-age/.08))*np.exp(-age/2.3)*np.minimum(1,np.maximum(0,9-age))
        frequency = round(midi(note)*duration)/duration
        shimmer = np.sin(2*np.pi*frequency*t)*envelope*.018
        stereo[:,0] += shimmer*np.sqrt(1-pan)
        stereo[:,1] += shimmer*np.sqrt(pan)
    # Quiet periodic air, decorrelated between channels.
    for channel in range(2):
        for frequency in RNG.integers(24000,72000,32)/duration:
            stereo[:,channel] += np.sin(2*np.pi*frequency*t+RNG.uniform(0,2*np.pi))*.00007
    stereo *= 1.7
    return save("OpticalLaboratory", stereo, rate, loop=True)


if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    OUT.with_suffix(".meta").write_text("fileFormatVersion: 2\nguid: " +
        uuid.uuid5(uuid.NAMESPACE_URL,"prism-original-audio-folder").hex +
        "\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n",encoding="utf-8")
    metrics = [ambience(),
        sfx("UI", .12, [(0,81,.025,.27)], .06),
        sfx("Place", .32, [(0,69,.075,.27),(.016,81,.055,.10)], .09),
        sfx("Rotate", .10, [(0,76,.020,.20)], .035),
        sfx("Invalid", .28, [(0,57,.060,.24),(.085,54,.060,.18)]),
        sfx("Goal", .60, [(0,81,.13,.22),(.08,88,.16,.15)]),
        sfx("Complete", 1.2, [(0,74,.22,.22),(.12,78,.24,.19),(.24,81,.28,.18),(.36,86,.32,.16)])]
    (OUT / "audio-metrics.json").write_text(json.dumps(metrics,indent=2)+"\n",encoding="utf-8")
    (OUT / "audio-metrics.json.meta").write_text("fileFormatVersion: 2\nguid: " +
        uuid.uuid5(uuid.NAMESPACE_URL,"prism-original-audio-metrics").hex +
        "\nTextScriptImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n",encoding="utf-8")
    print(json.dumps(metrics,indent=2))
