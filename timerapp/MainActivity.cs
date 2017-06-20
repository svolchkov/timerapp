using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Views;
using System.Timers;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
//using Java.Lang;
using Android.Content;
using Java.Lang;

namespace timerapp
{
    [Activity(Label = "timerapp", MainLauncher = true, Icon = "@drawable/Timerpic")]
    public class MainActivity : Activity
    {
        Button btnStart;
        Button btnReset;
        EditText hours;
        EditText minutes;
        EditText seconds;
        EditText desc;
        LinearLayout referenceLayout;
        LinearLayout timerLayout;
        Timer aTimer;
        MediaPlayer _player;
        MediaPlayer _player2;
        MediaPlayer _player3;
        MediaPlayer _player4;
        long timeWhenPaused;
        int timerSeconds;
        bool timerStopping;
        //AlarmManager am = (AlarmManager)context.GetSystemService(Context.AlarmService);

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);
            btnStart = FindViewById<Button>(Resource.Id.btnStart);
            btnReset = FindViewById<Button>(Resource.Id.btnReset);
            hours = FindViewById<EditText>(Resource.Id.etHours);
            minutes = FindViewById<EditText>(Resource.Id.etMinutes);
            seconds = FindViewById<EditText>(Resource.Id.etSeconds);
            desc = FindViewById<EditText>(Resource.Id.etDescription);
            referenceLayout = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            timerLayout = FindViewById<LinearLayout>(Resource.Id.linearLayout2);
            AlarmManager am;
            btnReset.Click += resetTimer;
            btnStart.Click += runTimer;
            seconds.Click += editTimeField;
            minutes.Click += editTimeField;
            hours.Click += editTimeField;
            //minutes.EditorAction += adjustMinutes;
            //seconds.EditorAction += adjustSeconds;
            minutes.FocusChange += adjustMinutes;
            seconds.FocusChange += adjustSeconds;
            hours.FocusChange += adjustHours;
            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;
            timeWhenPaused = 0;
            timerSeconds = 0;
            timerStopping = false;
            _player = MediaPlayer.Create(this, Resource.Raw.shoot);
            _player2 = MediaPlayer.Create(this, Resource.Raw.gong);
            _player3 = MediaPlayer.Create(this, Resource.Raw.doorbell);
            _player4 = MediaPlayer.Create(this, Resource.Raw.stag);
            _player3.Start();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean("timerState", aTimer.Enabled);
            if (aTimer.Enabled)
            {
                timeWhenPaused = DateTime.Now.Ticks;
                aTimer.Enabled = false;
                timerStopping = true;
            }
            outState.PutLong("timeNow", timeWhenPaused);
            outState.PutInt("secondsNow", timerSeconds);
            base.OnSaveInstanceState(outState);
            _player3.Start();
        }

        protected override void OnRestoreInstanceState(Bundle savedState)
        {
            base.OnRestoreInstanceState(savedState);
            bool timerState = savedState.GetBoolean("timerState");
            if (timerState)
            {
                aTimer.Enabled = true;
                btnStart.Text = "Stop";
                timeWhenPaused = savedState.GetLong("timeNow");
                timerSeconds = savedState.GetInt("secondsNow");
            }
            _player4.Start();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (timerStopping)
            {
                var ringTime = JavaSystem.CurrentTimeMillis() +
                (long)TimeSpan.FromSeconds(timerSeconds).TotalMilliseconds;
                var intent = new Intent("xamarincookbook.Alarm");
                intent.PutExtra("message", desc.Text);
                var alarm = PendingIntent.GetBroadcast(this, 0, intent, 0);
                var manager = AlarmManager.FromContext(this);
                //manager.Set(AlarmType.RtcWakeup, ringTime, alarm);
                if ((int)Build.VERSION.SdkInt >= 21) //my device enters this case
                {
                    AlarmManager.AlarmClockInfo info = new AlarmManager.AlarmClockInfo(ringTime, alarm);
                    manager.SetAlarmClock(info, alarm);
                }
                else
                {
                    manager.SetExact(AlarmType.RtcWakeup, ringTime, alarm);
                }
                _player.Start();
                timerStopping = false;
            }

            //}
           
        }

        protected override void OnResume()
        {
            base.OnResume();
            _player2.Start();
            if (timeWhenPaused != 0)
            {
                //DateTime myDate = new DateTime(timeWhenPaused);
                //String test = myDate.ToString("MMM dd yyyy hh:mm:ss tt");
                //Toast.MakeText(this, test, ToastLength.Long).Show();
                //timeWhenPaused = 0;
                double oldTime = System.Math.Round(timeWhenPaused / (double) TimeSpan.TicksPerSecond,0);
                double newTime = System.Math.Round(DateTime.Now.Ticks / (double)TimeSpan.TicksPerSecond,0);
                timerSeconds -= (int)(newTime - oldTime);
                timerSeconds = System.Math.Max(0, timerSeconds);
                timeWhenPaused = 0;
                if (timerSeconds > 0)
                {
                    aTimer.Enabled = true;
                }else
                {
                    RunOnUiThread(() => hours.Text = 0.ToString("00"));
                    RunOnUiThread(() => minutes.Text = 0.ToString("00"));
                    RunOnUiThread(() => seconds.Text = 0.ToString("00"));
                    RunOnUiThread(() => timerLayout.SetBackgroundColor(Color.Green));
                }
                var intent = new Intent("xamarincookbook.Alarm");
                var alarm = PendingIntent.GetBroadcast(this, 0, intent, 0);
                var manager = AlarmManager.FromContext(this);
                manager.Cancel(alarm);
            }
            else
            {
                //Toast.MakeText(this, "Time is 0", ToastLength.Long).Show();
            }
        }
        



        private void resetTimer(object sender, EventArgs e)
        {
            hours.Text = "00";
            minutes.Text = "00";
            seconds.Text = "00";
            timerSeconds = 0;
            hours.Focusable = true;
            hours.FocusableInTouchMode = true;
            minutes.Focusable = true;
            minutes.FocusableInTouchMode = true;
            seconds.Focusable = true;
            seconds.FocusableInTouchMode = true;
            var buttonBackground = referenceLayout.Background;
            Color backgroundColor = Color.Black;
            if (buttonBackground is ColorDrawable)
            {
                backgroundColor = (buttonBackground as ColorDrawable).Color;
                //You now have a background color.
            }
            timerLayout.SetBackgroundColor(backgroundColor);
            btnStart.Text = "Start";
        }

        private void runTimer(object sender, EventArgs e)
        {
            //timer.Text = "async method started";
            //await Task.Delay(1000); // example purpose only
            //timer.Text = "1 second passed";
            //await Task.Delay(2000);
            //timer.Text = "2 more seconds passed";
            //await Task.Delay(1000);
            //timer.Text = "async method completed";
            if (aTimer.Enabled == false)
            {
                int s, m, h;
                if (int.TryParse(seconds.Text, out s) 
                    && int.TryParse(minutes.Text, out m)
                    && int.TryParse(hours.Text, out h))
                {
                    timerSeconds = h * 3600 + m * 60 + s;
                }
                else
                {
                    Toast.MakeText(this, "Invalid time", ToastLength.Long);
                    return;
                }

                hours.Focusable = false;
                minutes.Focusable = false;
                seconds.Focusable = false;
                aTimer.Enabled = true;
                btnStart.Text = "Stop";

            }else
            {
                hours.Focusable = true;
                minutes.Focusable = true;
                seconds.Focusable = true;
                aTimer.Enabled = false;
                btnStart.Text = "Start";
            }



        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            timerSeconds--;
            if (timerSeconds <= 3)
            {
                _player.Start();
            }
            int s = 0, m = 0, h = 0;
            if (timerSeconds > 0)
            {                
                s = timerSeconds % 60;
                m = timerSeconds / 60 % 60;
                h = timerSeconds / 3600;
            }
            else
            {
                RunOnUiThread(() => timerLayout.SetBackgroundColor(Color.Green));
                aTimer.Enabled = false;
            }
            RunOnUiThread(() => hours.Text = h.ToString("00"));
            RunOnUiThread(() => minutes.Text = m.ToString("00"));
            RunOnUiThread(() => seconds.Text = s.ToString("00"));
        }

        private void adjustMinutes(object sender, View.FocusChangeEventArgs e)
        {
            //if (e.ActionId == ImeAction.Done || e.ActionId == ImeAction.Next)
            //{
            if (!(e.HasFocus))
            {
                if (minutes.Text == "") minutes.Text = "00";
                int m; 
                if (int.TryParse(minutes.Text, out m)) {
                    //if (m > 59)
                    //{
                        minutes.Text = (m % 60).ToString("00");
                        int h = int.Parse(hours.Text);
                        hours.Text = (h + m / 60).ToString("00");
                    //}
                }
            }
 
            //}
        }

        private void adjustSeconds(object sender, View.FocusChangeEventArgs e)
        {
            //if (e.ActionId == ImeAction.Done || e.ActionId == ImeAction.Next)
            //{
            if (!(e.HasFocus))
            {
                if (seconds.Text == "") seconds.Text = "00";
                int s;
                if (int.TryParse(seconds.Text, out s))
                {
                    //if (s > 59)
                    //{
                        seconds.Text = (s % 60).ToString("00");
                        int m = int.Parse(minutes.Text);
                        minutes.Text = (m + s / 60).ToString("00");
                    //}
                }
                //}
            }
        }

        private void adjustHours(object sender, View.FocusChangeEventArgs e)
        {
            int h;
            if (int.TryParse(hours.Text, out h))
            {
                hours.Text = h.ToString("00");
            }
        }

        private void editTimeField(object sender, EventArgs e)
        {
            EditText edit = (EditText)sender;
            if (edit.Focusable)
            {
                edit.SetSelection(edit.Text.Length);
            }
        }
    }
}

