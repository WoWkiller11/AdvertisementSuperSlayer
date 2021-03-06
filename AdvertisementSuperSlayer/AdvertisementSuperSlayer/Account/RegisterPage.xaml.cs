﻿using SkiaSharp;
using System;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Http;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Text;
using AdvertisementSuperSlayer.DbModels;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace AdvertisementSuperSlayer.Account
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {
        private const int STROKE_WIDTH = 50;
        private float offset;
        private float gradientCycleLength;
        private bool isAnimating;


        private bool usernameReady = false;
        private bool passwordReady = false;
        private bool emailReady = false;
        private bool cpasswordReady = false;


        private SKRect pathBounds;
        private SKPath infinityPath;
        private SKColor[] colors;
        private Stopwatch stopwatch;

        public RegisterPage()
        {
            InitializeComponent();
            BindingContext = this;

            colors = new SKColor[3];
            colors[0] = SKColor.Parse("FFC3A0");
            colors[1] = SKColor.Parse("FFAFBD");
            colors[2] = SKColor.Parse("FFC3A0");


            infinityPath = new SKPath();
            infinityPath.MoveTo(0, 0);
            infinityPath.LineTo(1000f, 0);
            infinityPath.LineTo(1000f, 1000f);
            infinityPath.LineTo(0, 1000f);
            infinityPath.LineTo(0, 0);

            sbmButton.IsEnabled = false;

            // Calculate path information 
            pathBounds = infinityPath.Bounds;
            gradientCycleLength = pathBounds.Width +
                pathBounds.Height * pathBounds.Height / pathBounds.Width;
        }

        

        /*************************************************************************************************************/
        /* LOGIC START*/
        private async void Button_Clicked(object sender, EventArgs e)
        {
            User usr = new User
            {
                Username = username.Text,
                Password = password.Text
            };

            bool reg = await App.Rest.Register(usr, email.Text);
            if (!reg)
            {
                await DisplayAlert("Register", "Invalid details supplied", "");
                return;
            }

            await Navigation.PushAsync(new Games.GameSelectMenue());
        }

        private void cpassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cpassword.Text == password.Text)
            {
                cpasswordReady = true;
            }
            else
            {
                cpasswordReady = false;
            }
            EnableButton();
        }

        private void password_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (password.Text.Length == 0)
            {
                passwordReady = false;
            }
            else
            {
                passwordReady = true;
            }
            EnableButton();
        }

        private void email_TextChanged(object sender, TextChangedEventArgs e)
        {
            Match m = Regex.Match(email.Text, @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$");
            
            
            if (m.Success)
            {
                emailReady = true;
            }
            else
            {
                emailReady = false;
            }
            EnableButton();
        }

        private void username_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (username.Text.Length == 0)
            {
                usernameReady = false;
            }
            else
            {
                usernameReady = true;
            }
            EnableButton();
        }

        private void EnableButton()
        {
            if (emailReady && usernameReady && passwordReady && cpasswordReady)
            {
                sbmButton.IsEnabled = true;
            }
            else
            {
                sbmButton.IsEnabled = false;
            }
        }

        public ICommand LoginNavigateCommand => new Command(async () =>
        {
            if (Navigation.NavigationStack.Count > 1 && Navigation.NavigationStack.ElementAt(Navigation.NavigationStack.Count - 2) is LoginPage)
            {
                await Navigation.PopAsync();
            }
            else
            {
                await Navigation.PushAsync(new LoginPage());
            }
        });
        /* LOGIC END */
        /*************************************************************************************************************/






        /*************************************************************************************************************/
        /* FOR BACKGROUND ANIMATION START*/
        protected override void OnAppearing()
        {
            base.OnAppearing();
            isAnimating = true;
            stopwatch = new Stopwatch();
            stopwatch.Start();
            Device.StartTimer(TimeSpan.FromMilliseconds(35), OnTimerTick);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            isAnimating = false;
            stopwatch.Stop();
        }

        private void StackLayout_SizeChanged(object sender, EventArgs e)
        {
            StackLayout st = sender as StackLayout;
            double side = (Width - 370) / 2.0d;
            st.Margin = new Thickness(side, 0, side, 15);
        }

        private void canvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            //Set transforms to shift path to center and scale to canvas size
            canvas.Translate(info.Width / 2, info.Height / 2);
            canvas.Scale(0.95f *
                Math.Min(info.Width / (pathBounds.Width + STROKE_WIDTH),
                         info.Height / (pathBounds.Height + STROKE_WIDTH)));



            using (SKPaint paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = STROKE_WIDTH;
                paint.Shader = SKShader.CreateLinearGradient(
                                    new SKPoint(pathBounds.Left, pathBounds.Top),
                                    new SKPoint(pathBounds.Right, pathBounds.Bottom),
                                    colors,
                                    null,
                                    SKShaderTileMode.Repeat,
                                    SKMatrix.MakeTranslation(offset, 0));

                canvas.DrawPaint(paint);
            }
        }

        private bool OnTimerTick()
        {
            const int duration = 2;     // seconds
            double progress = stopwatch.Elapsed.TotalSeconds % duration / duration;
            offset = (float)(gradientCycleLength * progress);
            canvasView.InvalidateSurface();

            return isAnimating;
        }
        /* FOR BACKGROUND ANIMATION END*/
        /*************************************************************************************************************/
    }
}