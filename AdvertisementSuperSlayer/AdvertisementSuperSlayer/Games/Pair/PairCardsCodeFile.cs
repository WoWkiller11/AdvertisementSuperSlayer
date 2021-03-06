﻿using SkiaSharp;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using AdvertisementSuperSlayer.Helpers;
using System.Diagnostics;
using AdvertisementSuperSlayer.Browser;

namespace AdvertisementSuperSlayer.Games.Pair
{
    public partial class PairCardsPage : ContentPage
    {
        private double WindowWidth { get; set; }
        private double WindowHeight { get; set; }
        private int Rows, Cols;
        private int ErrorCounter = 0;
        private int RightCount = 0;
        private SKBitmap TileBitmap;
        private SKBitmap CheckMark;
        private BusyBehavior _Busy;
        private double tileSize;
        private PhotoHalfPairTile[][] tiles;
        private event EventHandler<ConfigEventArgs> RightConfiguration;
        private event EventHandler<ConfigEventArgs> WrongConfiguration;
        private SKImageInfo ImInfo;
        private IAudio[] wins;
        private IAudio[] defeats;
        private Random rnd;
        private Stopwatch Timer;

        private async void ExecuteRotation(PhotoHalfPairTile tmp)
        {
            const float degreeStep = 15.0f;
            const int delayMicrosecond = 15;
            _Busy.Take(tmp);
            tmp.IsRotating = true;
            tmp.Flipped = true;
            while (tmp.Deg <= 90 - degreeStep)
            {
                await Task.Delay(delayMicrosecond);
                await Device.InvokeOnMainThreadAsync(() => tmp.Deg = tmp.Deg + degreeStep);
            }
            while (tmp.Deg >= 0 + degreeStep)
            {
                await Task.Delay(delayMicrosecond);
                await Device.InvokeOnMainThreadAsync(() => tmp.Deg = tmp.Deg - degreeStep);
            }

            tmp.Deg = 0;
            tmp.IsRotating = false;


            if (_Busy.State.Equals(BusyStates.Filled) || _Busy.State.Equals(BusyStates.Right))
            {
                int SRow = _Busy.Second.Row;
                int SCol = _Busy.Second.Col;
                while (tmp.IsRotating || tiles[SRow][SCol].IsRotating)
                {
                    continue;
                }
            }
            if (_Busy.State.Equals(BusyStates.Right))
            {
                ConfigEventArgs args = new ConfigEventArgs
                {
                    FRow = _Busy.First.Row,
                    FCol = _Busy.First.Col,
                    SRow = _Busy.Second.Row,
                    SCol = _Busy.Second.Col
                };
                RightConfiguration?.Invoke(this, args);
            }
            if (_Busy.State.Equals(BusyStates.Filled))
            {
                ConfigEventArgs args = new ConfigEventArgs
                {
                    FRow = _Busy.First.Row,
                    FCol = _Busy.First.Col,
                    SRow = _Busy.Second.Row,
                    SCol = _Busy.Second.Col
                };
                WrongConfiguration?.Invoke(this, args);
            }
        }


        private bool CanExecuteRotation(object sender)
        {
            bool cond1 = _Busy.State.Equals(BusyStates.AllFree);
            bool cond2 = _Busy.State.Equals(BusyStates.OneFree);
            bool cond3 = (sender as PhotoHalfPairTile).Flipped;

            if ((cond1 || cond2) && !cond3)
                return true;
            return false;
        }

        private async void OnRightConfiguration(object sender, ConfigEventArgs args)
        {
            RightCount++;
            int FRow = args.FRow;
            int FCol = args.FCol;
            int SRow = args.SRow;
            int SCol = args.SCol;
            bool cond1 = tiles[FRow][FCol].IsRotating;
            bool cond2 = tiles[SRow][SCol].IsRotating;
            while (cond1 || cond2)
            {
                continue;
            }
            int soundId = rnd.Next(wins.Length);
            await wins[soundId].PlaySoundAsync();
            await Task.Delay(200);
            tiles[FRow][FCol].GestureRecognizers.Clear();
            tiles[SRow][SCol].GestureRecognizers.Clear();
            tiles[FRow][FCol].SetOk(CheckMark);
            tiles[SRow][SCol].SetOk(CheckMark);
            _Busy.Release();

            if (RightCount == Rows * Cols / 2)
            {
                SaveResult();
                await DisplayAlert("Victory", "You have won", "Ok");
                await Navigation.PopAsync();
            }
        }

        private async void OnWrongConfiguration(object sender, ConfigEventArgs args)
        {
            const float degreeStep = 15.0f;
            const int delayMicrosecond = 15;
            int FRow = args.FRow;
            int FCol = args.FCol;
            int SRow = args.SRow;
            int SCol = args.SCol;
            int soundId = rnd.Next(defeats.Length);
            await defeats[soundId].PlaySoundAsync();

            tiles[FRow][FCol].IsRotating = true;
            tiles[SRow][SCol].IsRotating = true;
            await Task.Delay(1000);
            while (tiles[FRow][FCol].Deg <= 90 - degreeStep)
            {
                await Task.Delay(delayMicrosecond);
                await Device.InvokeOnMainThreadAsync(() => tiles[FRow][FCol].Deg = tiles[FRow][FCol].Deg + degreeStep);
                await Device.InvokeOnMainThreadAsync(() => tiles[SRow][SCol].Deg = tiles[SRow][SCol].Deg + degreeStep);
            }
            while (tiles[FRow][FCol].Deg >= 0 + degreeStep)
            {
                await Task.Delay(delayMicrosecond);
                await Device.InvokeOnMainThreadAsync(() => tiles[FRow][FCol].Deg = tiles[FRow][FCol].Deg - degreeStep);
                await Device.InvokeOnMainThreadAsync(() => tiles[SRow][SCol].Deg = tiles[SRow][SCol].Deg - degreeStep);
            }
            await Device.InvokeOnMainThreadAsync(() => tiles[FRow][FCol].Deg = 0);
            await Device.InvokeOnMainThreadAsync(() => tiles[SRow][SCol].Deg = 0);
            tiles[FRow][FCol].IsRotating = false;
            tiles[SRow][SCol].IsRotating = false;

            ////
            _Busy.Release();
            bool cond1 = tiles[FRow][FCol].WasTapped;
            bool cond2 = tiles[SRow][SCol].WasTapped;
            if (cond1 || cond2)
            {
                ErrorCounter++;
                if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                {
                    await Navigation.PushAsync(new AdvPage());
                }
                else
                {
                    BrowserPage browser = new BrowserPage();
                    await Navigation.PushAsync(browser);
                    browser.Navigate(App.Rest.advUrl);
                }
            }

            tiles[FRow][FCol].WasTapped = true;
            tiles[SRow][SCol].WasTapped = true;

            tiles[FRow][FCol].Flipped = false;
            tiles[SRow][SCol].Flipped = false;
        }
    }
}
