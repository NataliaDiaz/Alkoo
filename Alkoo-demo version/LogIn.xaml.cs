using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Alkoo.Resources;
using Alkoo.Model;

namespace Alkoo
{
    public partial class LogIn : PhoneApplicationPage
    {
        public Model.User Active_user;
        // Constructor
        public LogIn()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            //Login button

        }

        private void User1_Click(object sender, RoutedEventArgs e)
        {
            //User1 button
            Active_user = new User();
            Active_user.HeTu = "251093-689D";
            Active_user.Access_token = "z0wcdqt3abvyj4u7ilzz39veerq7z8gh";

            PhoneApplicationService.Current.State["param"] = Active_user;
        }

        private void User2_Click(object sender, RoutedEventArgs e)
        {
            //User2 button
            Active_user = new User();
            Active_user.HeTu = "020682-764X";
            Active_user.Access_token = "1cosbjrt49bv66axv7b5af2no14wdjb2";

            PhoneApplicationService.Current.State["param"] = Active_user;
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {

        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}