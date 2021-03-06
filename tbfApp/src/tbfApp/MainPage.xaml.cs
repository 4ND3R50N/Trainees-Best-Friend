﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace tbfApp
{
    public partial class MainPage : MasterDetailPage
    {
        //MasterPage masterPage;
        public MainPage()
        {
            masterPage = new MasterPage()
            {
                //Title = "Menü", //Wird nicht angezeigt
                BackgroundColor = Color.FromHex(App.GetMenueColor()), //#009acd
            };
            Master = masterPage;
            Detail = new NavigationPage(new RoomPage()
            {
                Title = "Räume",
            })
            {
                BarBackgroundColor = Color.FromHex(App.GetMenueColor()), //#009acd
                BarTextColor = Color.White,
            };

            masterPage.ListView.ItemSelected += OnItemSelected;
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPageItem;
            if (item != null)
            {
                var page = (Page) Activator.CreateInstance(item.TargetType);
                Detail = new NavigationPage(page)
                {
                    BarBackgroundColor = Color.FromHex(App.GetMenueColor()),
                    BarTextColor = Color.White,
                };
                //page.Title = item.TargetType.Name; //Titel anpassen, funktionieren
                masterPage.ListView.SelectedItem = null;
                IsPresented = false;
            }
        }
    }
}
