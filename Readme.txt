// --------------------------------------------------------------------------------
// README / Notes (end of code doc)
// --------------------------------------------------------------------------------
// How this works & next steps:
// 1) This project uses ffmpeg.exe to convert arbitrary audio/video files to .ogg (Vorbis).
// - FFmpeg must be present either on the user's PATH or in the application's folder (ffmpeg.exe).
// - You can bundle ffmpeg binaries with your installer to avoid requiring users to install anything.
// 2) If you want a pure-managed encoder (no external binary): implement encoding using NAudio to extract PCM
// and a managed Ogg Vorbis encoder library (e.g. OggVorbisEncoder). That requires additional NuGet packages
// and more code for sample conversion — I can provide that implementation in a follow-up if you want.
// 3) Visual Studio 2022: create a new WPF App (.NET) project, replace the files with those in this document,
// add the NAudio NuGet package (already in csproj). If you rely on ffmpeg only, NAudio is optional and may be removed.
// 4) Packaging: include ffmpeg.exe in output folder or add a setting letting user locate ffmpeg.
// 5) Stellar-specific notes: Stellaris expects Ogg Vorbis (.ogg) for music. The conversion here uses libvorbis
// with quality scale 5 (-qscale:a 5). If you need specific bitrate/compatibility tweak -qscale:a or use -b:a.


// If you'd like, I can now:
// - (A) Provide a single-file printout variant where every class is concatenated with dashed separators (you requested this option),
// - (B) Replace ffmpeg-based conversion with a pure-managed pipeline using NAudio + OggVorbisEncoder,
// - (C) Create an installer/packaging script that bundles ffmpeg and places converted files into Stellaris music folders,
// - (D) Add features: batch renaming, preview play, automatic metadata generation for Stellaris, or drag & drop into the game's folder.


// Tell me which of A/B/C/D you prefer and I'll produce the full code or the single-file printout accordingly.