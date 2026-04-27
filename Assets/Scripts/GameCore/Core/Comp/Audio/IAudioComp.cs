using System;
using UnityEngine;

namespace GameCore.Core.Comp.Audio{
    public interface IAudioComp : IComp{
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="name">音乐名称</param>
        void PlayBGM(string name);

        /// <summary>
        /// 播放背景音乐（异步）
        /// </summary>
        /// <param name="name">音乐名称</param>
        /// <param name="callback">回调函数</param>
        void PlayBGMAsync(string name, Action callback = null);

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        void StopBGM();

        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        void PauseBGM();

        /// <summary>
        /// 恢复背景音乐
        /// </summary>
        void ResumeBGM();

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="name">音效名称</param>
        void PlaySfx(string name);

        /// <summary>
        /// 播放音效（指定位置）
        /// </summary>
        /// <param name="name">音效名称</param>
        /// <param name="position">播放位置</param>
        void PlaySfxAtPosition(string name, Vector3 position);

        /// <summary>
        /// 停止所有音效
        /// </summary>
        void StopAllSfx();

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume">音量（0-1）</param>
        void SetVolume(float volume);

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        /// <param name="volume">音量（0-1）</param>
        void SetBGMVolume(float volume);

        /// <summary>
        /// 设置音效音量
        /// </summary>
        /// <param name="volume">音量（0-1）</param>
        void SetSFXVolume(float volume);

        /// <summary>
        /// 获取背景音乐音量
        /// </summary>
        /// <returns>音量</returns>
        float GetBGMVolume();

        /// <summary>
        /// 获取音效音量
        /// </summary>
        /// <returns>音量</returns>
        float GetSFXVolume();

        /// <summary>
        /// 预加载音频
        /// </summary>
        /// <param name="name">音频名称</param>
        /// <param name="clip">音频片段</param>
        void PreloadAudio(string name, AudioClip clip);

        /// <summary>
        /// 卸载音频
        /// </summary>
        /// <param name="name">音频名称</param>
        void UnloadAudio(string name);

        /// <summary>
        /// 检查音频是否已加载
        /// </summary>
        /// <param name="name">音频名称</param>
        /// <returns>是否已加载</returns>
        bool IsAudioLoaded(string name);

        /// <summary>
        /// 检查背景音乐是否正在播放
        /// </summary>
        /// <returns>是否正在播放</returns>
        bool IsBGMPlaying();

        /// <summary>
        /// 获取已加载音频数量
        /// </summary>
        /// <returns>音频数量</returns>
        int GetLoadedAudioCount();
    }
}