using System.Media;

namespace GameAutomation.Core;

public class SoundNotifier
{
    private readonly SoundPlayer _player;

    public SoundNotifier()
    {
        _player = new SoundPlayer();
    }

    public void Play(SoundType soundType)
    {
        _player.Stream = soundType switch
        {
            SoundType.MissionComplete => Sounds.MissionComplete,
            SoundType.HereWeGoAgain => Sounds.HereWeGoAgain,
            _ => throw new NotImplementedException(),
        };

        _player.LoadAsync();

        _player.Play();

        _player.Stream.Close();
        _player.Stream.Dispose();
    }
}
