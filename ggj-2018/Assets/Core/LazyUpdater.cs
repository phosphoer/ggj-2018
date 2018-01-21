using UnityEngine;
using System.Collections.Generic;

public class LazyUpdater : Singleton<MonoBehaviour>
{
  public enum UpdateChannelType
  {
    Default,
  }

  public interface ILazyUpdate
  {
    void LazyUpdate();
  }

  private class UpdateChannel
  {
    public List<ILazyUpdate> UpdateList = new List<ILazyUpdate>();
    public int UpdateIndex = 0;
  }

  private UpdateChannel[] _updateChannels;

  private void Start()
  {
    _updateChannels = new UpdateChannel[System.Enum.GetValues(typeof(UpdateChannelType)).Length];
    for (int i = 0; i < _updateChannels.Length; ++i)
    {
      _updateChannels[i] = new UpdateChannel();
    }
  }

  private void Update()
  {
    for (int i = 0; i < _updateChannels.Length; ++i)
    {
      UpdateChannel channel = _updateChannels[i];
      if (channel.UpdateIndex < channel.UpdateList.Count)
      {
        channel.UpdateList[channel.UpdateIndex].LazyUpdate();
        ++channel.UpdateIndex;
      }
      else
      {
        channel.UpdateIndex = 0;
      }
    }
  }

  public void AddLazyUpdate(ILazyUpdate lazyUpdate, UpdateChannelType toChannel)
  {
    int index = (int)toChannel;
    _updateChannels[index].UpdateList.Add(lazyUpdate);
  }

  public void RemoveLazyUpdate(ILazyUpdate lazyUpdate, UpdateChannelType fromChannel)
  {
    int index = (int)fromChannel;
    _updateChannels[index].UpdateList.Remove(lazyUpdate);
  }
}