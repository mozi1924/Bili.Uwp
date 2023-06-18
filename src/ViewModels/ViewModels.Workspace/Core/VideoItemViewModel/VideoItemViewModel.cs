﻿// Copyright (c) Richasy. All rights reserved.

using System;
using System.Threading.Tasks;
using Bili.DI.Container;
using Bili.Lib.Interfaces;
using Bili.Models.Data.Video;
using Bili.Models.Enums;
using Bili.Toolkit.Interfaces;
using Bili.ViewModels.Interfaces.Account;
using Bili.ViewModels.Interfaces.Core;
using Bili.ViewModels.Interfaces.Video;
using CommunityToolkit.Mvvm.Input;

namespace Bili.ViewModels.Workspace.Core
{
    /// <summary>
    /// 视频条目视图模型.
    /// </summary>
    public sealed partial class VideoItemViewModel : ViewModelBase, IVideoItemViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoItemViewModel"/> class.
        /// </summary>
        public VideoItemViewModel(
            INumberToolkit numberToolkit,
            IAccountProvider accountProvider,
            IAuthorizeProvider authorizeProvider,
            IFavoriteProvider favoriteProvider,
            IResourceToolkit resourceToolkit,
            ICallerViewModel callerViewModel)
        {
            _numberToolkit = numberToolkit;
            _accountProvider = accountProvider;
            _authorizeProvider = authorizeProvider;
            _favoriteProvider = favoriteProvider;
            _resourceToolkit = resourceToolkit;
            _callerViewModel = callerViewModel;

            AddToViewLaterCommand = new AsyncRelayCommand(AddToViewLaterAsync);
            RemoveFromViewLaterCommand = new AsyncRelayCommand(RemoveFromViewLaterAsync);
            RemoveFromHistoryCommand = new AsyncRelayCommand(RemoveFromHistoryAsync);
            RemoveFromFavoriteCommand = new AsyncRelayCommand(RemoveFromFavoriteAsync);

            AttachExceptionHandlerToAsyncCommand(
                LogException,
                RemoveFromFavoriteCommand);
        }

        /// <inheritdoc/>
        public void InjectData(VideoInformation information)
        {
            Data = information;
            InitializeData();
        }

        /// <inheritdoc/>
        public void InjectAction(Action<IVideoItemViewModel> action)
            => _additionalAction = action;

        /// <inheritdoc/>
        public void SetAdditionalData(object data)
            => _additionalData = data;

        private void InitializeData()
        {
            IsShowCommunity = Data.CommunityInformation != null;
            var userVM = Locator.Instance.GetService<IUserItemViewModel>();
            userVM.SetProfile(Data.Publisher);
            Publisher = userVM;
            if (IsShowCommunity)
            {
                PlayCountText = _numberToolkit.GetCountText(Data.CommunityInformation.PlayCount);
                DanmakuCountText = _numberToolkit.GetCountText(Data.CommunityInformation.DanmakuCount);
                LikeCountText = _numberToolkit.GetCountText(Data.CommunityInformation.LikeCount);

                IsShowScore = Data.CommunityInformation?.Score > 0;
                ScoreText = IsShowScore ?
                    Data.CommunityInformation.Score.ToString("0")
                    : default;
            }

            if (Data.Identifier.Duration > 0)
            {
                DurationText = _numberToolkit.GetDurationText(TimeSpan.FromSeconds(Data.Identifier.Duration));
            }
        }

        [RelayCommand]
        private Task PlayAsync()
            => Utilities.PlayVideoWithIdAsync(Data.Identifier.Id);

        private async Task AddToViewLaterAsync()
        {
            if (_authorizeProvider.State == AuthorizeState.SignedIn)
            {
                var result = await _accountProvider.AddVideoToViewLaterAsync(Data.Identifier.Id);
                if (result)
                {
                    // 显示添加成功的消息.
                    _callerViewModel.ShowTip(
                        _resourceToolkit.GetLocaleString(LanguageNames.AddViewLaterSucceseded),
                        Models.Enums.App.InfoType.Success);
                }
                else
                {
                    // 显示添加失败的消息.
                    _callerViewModel.ShowTip(
                        _resourceToolkit.GetLocaleString(LanguageNames.AddViewLaterFailed),
                        Models.Enums.App.InfoType.Error);
                }
            }
            else
            {
                // 显示需要登录的消息.
                _callerViewModel.ShowTip(
                        _resourceToolkit.GetLocaleString(LanguageNames.NeedLoginFirst),
                        Models.Enums.App.InfoType.Warning);
            }
        }

        private async Task RemoveFromViewLaterAsync()
        {
            if (_authorizeProvider.State == AuthorizeState.SignedIn)
            {
                var result = await _accountProvider.RemoveVideoFromViewLaterAsync(Data.Identifier.Id);
                if (!result)
                {
                    // 显示移除失败的消息.
                    _callerViewModel.ShowTip(
                        _resourceToolkit.GetLocaleString(LanguageNames.RemoveViewLaterFailed),
                        Models.Enums.App.InfoType.Error);
                }
                else
                {
                    _additionalAction?.Invoke(this);
                }
            }
            else
            {
                // 显示需要登录的消息.
                _callerViewModel.ShowTip(
                        _resourceToolkit.GetLocaleString(LanguageNames.NeedLoginFirst),
                        Models.Enums.App.InfoType.Warning);
            }
        }

        private async Task RemoveFromHistoryAsync()
        {
            if (_authorizeProvider.State == AuthorizeState.SignedIn)
            {
                var result = await _accountProvider.RemoveHistoryItemAsync(Data.Identifier.Id);
                if (!result)
                {
                    // 显示移除失败的消息.
                    _callerViewModel.ShowTip(
                        _resourceToolkit.GetLocaleString(LanguageNames.FailedToRemoveVideoFromHistory),
                        Models.Enums.App.InfoType.Error);
                }
                else
                {
                    _additionalAction?.Invoke(this);
                }
            }
            else
            {
                // 显示需要登录的消息.
                _callerViewModel.ShowTip(
                        _resourceToolkit.GetLocaleString(LanguageNames.NeedLoginFirst),
                        Models.Enums.App.InfoType.Warning);
            }
        }

        private async Task RemoveFromFavoriteAsync()
        {
            if (_authorizeProvider.State == AuthorizeState.SignedIn)
            {
                var folderId = _additionalData.ToString();
                var result = await _favoriteProvider.RemoveFavoriteVideoAsync(folderId, Data.Identifier.Id);
                if (!result)
                {
                    // 显示移除失败的消息.
                    _callerViewModel.ShowTip(
                        _resourceToolkit.GetLocaleString(LanguageNames.FailedToRemoveVideoFromFavorite),
                        Models.Enums.App.InfoType.Error);
                }
                else
                {
                    _additionalAction?.Invoke(this);
                }
            }
            else
            {
                // 显示需要登录的消息.
                _callerViewModel.ShowTip(
                        _resourceToolkit.GetLocaleString(LanguageNames.NeedLoginFirst),
                        Models.Enums.App.InfoType.Warning);
            }
        }
    }
}