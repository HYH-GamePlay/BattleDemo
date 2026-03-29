namespace GameCore{
    public interface IAudioComp : IComp{
        void PlayBGM(string name);
        void PlaySfx(string name);
        void SetVolume(float volume);
    }
}