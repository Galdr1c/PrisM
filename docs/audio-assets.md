# Original PrisM audio

All seven WAV assets are original offline synthesis created for this project. They contain no borrowed samples, recordings, vocals, or third-party music. No external audio licence or attribution is required. The deterministic arrangement and seed live in `Tools/Generate-PrismAudio.py` (Python 3 + NumPy); running that file regenerates the audio, stable Unity import GUIDs, and waveform measurements.

| Asset | Duration | Character / event |
| --- | ---: | --- |
| OpticalLaboratory | 48 s | Stereo ambient loop: six slow overlapping chord voicings, sparse glass reflections, quiet air; no percussion |
| UI | 0.12 s | Short bright, softened control tick |
| Place | 0.32 s | Glass placement with a quiet tactile transient |
| Rotate | 0.10 s | Lighter, quieter rotation tick; limited to one cue per 90 ms |
| Invalid | 0.28 s | Low descending pair |
| Goal | 0.60 s | Small rising sparkle; limited to one cue per 120 ms |
| Complete | 1.20 s | Longer ascending four-note resolve |

`PrismFeedback` retains Click/Invalid/Complete and adds Place/Rotate/Goal. AudioEnabled/SetAudio controls effects; MusicEnabled/SetMusic controls music. Settings are independent and persisted. When first introduced, music inherits an existing disabled audio preference. BGM fades in over one second, loops on a separate lower-priority source, pauses on app suspension or focus loss, and resumes at its retained position. Effects stop on suspension/focus loss. Haptics remain completion-only with a one-second cooldown.

Source music is 32 kHz stereo PCM16 (6.14 MB). Unity imports it as streaming Vorbis at quality 0.65, preserving stereo and source sample rate; this avoids holding the full decoded loop in memory. The six effects are 44.1 kHz mono PCM16 (about 231 KB total), imported as preloaded PCM / Decompress On Load for quick cues. `PrismAudioImporter` enforces these settings only inside the audio folder; WAV metadata also records them.

The generator checks finite samples, DC offset, headroom, silent effect endpoints, and music wrap continuity before writing files. Maximum source peak is 0.375 (completion); music peak is 0.119, RMS 0.0268. The source-loop boundary delta is 0.000754, below its ordinary largest adjacent-sample change. A separate PCM16 read-back check passed for all seven files; the quantized music seam is 0.000763 versus a largest adjacent step of 0.00760. Periodic oscillator frequencies and circular envelopes retain continuity across the wrap. `audio-metrics.json` stores the exact measurements. Runtime and importer C# compile against the installed Unity 6000.6.0f1 module libraries with Android symbols, zero errors and zero warnings, without launching Unity.

Release validation still needs Unity import/playback, a listen through headphones and phone speakers, the decoded Vorbis loop seam, and device pause/resume testing. Mathematical checks cannot judge timbre or device loudness. Import preloading behavior follows the [Unity AudioImporterSampleSettings API](https://docs.unity.com/en-us/engine/6000.5/script-reference/unityeditor/audioimportersamplesettings/preloadaudiodata).
