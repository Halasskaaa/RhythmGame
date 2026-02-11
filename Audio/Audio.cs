using System;
using System.IO;
using SDL3;

namespace wah.Audio
{
    internal class Audio
    {
        private IntPtr mixer;
        private IntPtr audio;
        private IntPtr track;

        public TimeSpan PlayBackPosition
        {
            get
            {
                var position = Mixer.GetTrackPlaybackPosition(track);
                if (position == -1)
                    SDL.LogError(SDL.LogCategory.Audio, $"failed to get playback position: {SDL.GetError()}");

                return TimeSpan.FromMilliseconds(Mixer.TrackFramesToMS(track, position));
            }
            set
            {
                if (!Mixer.SetTrackPlaybackPosition(track, Mixer.TrackMSToFrames(track, value.Milliseconds)))
                    SDL.LogError(SDL.LogCategory.Audio, $"failed to set playback position: {SDL.GetError()}");
            }
        }

        public TimeSpan Length
        {
            get
            {
                var length = Mixer.GetAudioDuration(audio);
                if (length == -1) SDL.LogError(SDL.LogCategory.Audio, $"failed to get audio length: {SDL.GetError()}");

                return TimeSpan.FromMilliseconds(Mixer.TrackFramesToMS(track, length));
            }
        }

        public Audio(FileInfo file)
        {
            mixer = Mixer.CreateMixerDevice(SDL.AudioDeviceDefaultPlayback, IntPtr.Zero);
            if (mixer == IntPtr.Zero)
            {
                SDL.LogError(SDL.LogCategory.Audio, $"failed to create mixer device: {SDL.GetError()}");
                return;
            }

            audio = Mixer.LoadAudio(mixer, file.FullName, true);
            if (audio == IntPtr.Zero)
            {
                SDL.LogError(SDL.LogCategory.Audio, $"failed to load audio from \"{file.FullName}\": {SDL.GetError()}");
                return;
            }

            track = Mixer.CreateTrack(mixer);
            if (track == IntPtr.Zero)
            {
                SDL.LogError(SDL.LogCategory.Audio, $"failed to create track device: {SDL.GetError()}");
                return;
            }

            if (!Mixer.SetTrackAudio(track, audio))
            {
                SDL.LogError(SDL.LogCategory.Audio, $"failed to set track audio: {SDL.GetError()}");
            }
        }

        public void Play()
        {
            if (!Mixer.PlayTrack(track, 0))
                SDL.LogError(SDL.LogCategory.Audio, $"failed to start track: {SDL.GetError()}");
        }

        public void Stop()
        {
            if (!Mixer.StopTrack(track, 0))
                SDL.LogError(SDL.LogCategory.Audio, $"failed to stop track: {SDL.GetError()}");
        }

        public void Pause()
        {
            if (!Mixer.PauseTrack(track))
                SDL.LogError(SDL.LogCategory.Audio, $"failed to pause track: {SDL.GetError()}");
        }

        public void Resume()
        {
            if (!Mixer.ResumeTrack(track))
                SDL.LogError(SDL.LogCategory.Audio, $"failed to resume track: {SDL.GetError()}");
        }

        ~Audio()
        {
            Stop();

            if (track != IntPtr.Zero)
                Mixer.DestroyTrack(track);
            if (audio != IntPtr.Zero)
                Mixer.DestroyAudio(audio);
            if (mixer != IntPtr.Zero)
                Mixer.DestroyMixer(mixer);
        }
    }
}
