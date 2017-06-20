using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media;

namespace timerapp
{
    [BroadcastReceiver(Process = ":remote")]
    [IntentFilter(new[] { "xamarincookbook.Alarm" })]
    public class AlarmReceiver : BroadcastReceiver
    {
        MediaPlayer _player4;

        public override void OnReceive(Context context, Intent intent)
        {
            _player4 = MediaPlayer.Create(context, Resource.Raw.stag);
            _player4.Start();
            //Console.WriteLine("alarm fired");
            string message = "Countdown finished";
            if (!(intent.GetStringExtra("message") == "")) message = intent.GetStringExtra("message");
            Toast.MakeText(context, message, ToastLength.Short).Show();
        }
    }
}