﻿using Android.App;
using Android.Widget;
using Android.OS;
using CallOfService.Mobile.Core.DI;
using CallOfService.Mobile.Droid.Core.DI;
using Acr.UserDialogs;
using Android.Content.PM;

namespace CallOfService.Mobile.Droid
{
	[Activity (Label = "CallOfService.Mobile.Droid", 
	           Icon = "@mipmap/icon",
	           Theme = "@style/MyTheme",
	           MainLauncher = true,
			   ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate (bundle);
			UserDialogs.Init(this);
			global::Xamarin.Forms.Forms.Init(this, bundle);
			global::Xamarin.FormsMaps.Init(this,bundle);
			DependencyResolver.Initialize(new AndroidModule(), new FormsModule());
			LoadApplication(new App());
		}
	}
}

