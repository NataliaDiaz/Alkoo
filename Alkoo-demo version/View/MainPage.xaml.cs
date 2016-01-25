using System;
using System.IO;
using System.Text;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using BluetoothConnection;
using Windows.Networking.Proximity;
using System.Diagnostics; //Debug


namespace Alkoo.View
{
    public partial class MainPage : PhoneApplicationPage
    {


        public static Model.User user;

        private ConnectionManager bt_manager; //Bluetooth
        private AlcoMeter meter;


        //Estimated blood ethanol concentration (EBAC) formula in 
        //http://en.wikipedia.org/wiki/Blood_alcohol_content
        float MRwomen = 0.017f; // Metabolism constant  (0.017 for women and 0.015 for men) 
        float MRmen = 0.015f;
        float DP = -1f; // Drinking period in hours
        float EBAC = 0.018f;//0.018f; // Estimated blood ethanol concentration. E.g. 0.018 is 1 drink for a male of 82 kg 
        float BWmen = 0.58f; // Body water constant (0.58 for men and 0.49 for women)
        float BWwomen = 0.49f;
        float Wt = -1f; // Body weight
        float SD = -1f; // SD is the number of standard drinks containing 10 grams of ethanol
        // 1.2 is a factor in order to convert the amount in grams to Swedish standards set by 
        // The Swedish National Institute of Public Health
        // 0.806 is a constant for body water in the blood (mean 80.6%)
        

        public MainPage()
        {
            InitializeComponent();
            string returnString;
            user = PhoneApplicationService.Current.State["param"] as Model.User;
            Connection_manager manager = new Connection_manager();
            manager.fetch_user_profile_from_taltioni(user);
            //Not working at the moment
            //manager.fetch_user_weigth_from_taltioni(user);
            bt_manager = new ConnectionManager();
            bt_manager.MessageReceived += meterReadingReceived;
            meter = new AlcoMeter();
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bt_manager.Initialize();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            bt_manager.Terminate();
        }

        public static void Update_user_profile(string profile) 
        {

            User_profile j = JsonConvert.DeserializeObject<User_profile>(profile);

            
            user.First_name = (string)j.FirstName;
            user.Last_name = (string)j.LastName;
            PhoneApplicationService.Current.State["param"] = user;
            

            /*
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show((string)j.FirstName);
            });
            */
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ConnectToMeter();
            //bt_manager.SendByte((byte)255); //Reset meter

            Dispatcher.BeginInvoke(delegate()
            {
                AnalyzeButton.IsEnabled = false;
            });


        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {

        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {

        }

        /* Connect to BT Alcometer */
        public async void ConnectToMeter()
        {
            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
            var pairedDevices = await PeerFinder.FindAllPeersAsync();

            if (pairedDevices.Count == 0)
            {
                Debug.WriteLine("No paired devices were found.");
            }
            else
            {
                foreach (var pairedDevice in pairedDevices)
                {
                    if (pairedDevice.DisplayName == "Alkoo")
                    {
                        bt_manager.Connect(pairedDevice.HostName);
                        //AnalyzeButton.Content = "Connected";
                        //DeviceName.IsReadOnly = true;
                        //ConnectAppToDeviceButton.IsEnabled = false;
                        continue;
                    }
                }
            }
        }

        void meterReadingReceived(int message)
        {
            Debug.WriteLine("Message received:" + message);
            meter.reading = message;
            Dispatcher.BeginInvoke(delegate()
            {
                AlcoholValue.Text = meter.reading2BAC().ToString();
            });

            EBAC = meter.reading2BAC()/100.0F;
            // ANALYZE BUTTON

            Dispatcher.BeginInvoke(delegate()
            {

            string[] splitH_M = (HourMinute.Text).Split(new Char[] { ':' });

            DateTime drinkingStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, Convert.ToInt16(splitH_M[0]), Convert.ToInt16(splitH_M[1]), 0);// DateTime.Today;
            
            Debug.WriteLine("Drinking Start Date is: " + drinkingStartDate + "and EBAC is " + EBAC);
            //DP = (float)((DateTime.Now).Subtract(drinkingStartDate)).Duration().TotalHours;
            DP = (float)((DateTime.Now).Subtract(drinkingStartDate)).TotalHours;
            //TimeSpan DP2 = DateTime.Now.Subtract(drinkingStartDate);

            TimeSpan ts = (drinkingStartDate - DateTime.Now);
            Debug.WriteLine("Drinking period until now: " + DP + "H. DATE TODAY:" + DateTime.Now);

            string weight = "";
            if (NavigationContext.QueryString.TryGetValue("weight", out weight))
                Wt = Convert.ToInt16(weight);
            Debug.WriteLine("Weight is " + weight);
            string sex = "";

            if (NavigationContext.QueryString.TryGetValue("sex", out sex))
            {
                if (sex.Equals("female"))
                    SD = (((MRwomen * DP) + EBAC) * BWwomen * Wt) / (0.806f * 1.2f);
                else
                    if (sex.Equals("male"))
                        SD = (((MRmen * DP) + EBAC) * BWmen * Wt) / (0.806f * 1.2f);
                    else
                        Debug.WriteLine("Sex undisclosed");
            }

            Debug.WriteLine("Estimated Drink units are: " + SD + " for sex :" + sex + " and weight: " + weight);
           

            // VISUALIZE RESULTS
            ShareButton.Visibility = Visibility.Visible;  //Opposite is Collapsed.
            YouHad.Visibility = Visibility.Visible;
            Diagnosis.Visibility = Visibility.Visible;
            Units.Visibility = Visibility.Visible;
            YourAlcohol.Visibility = Visibility.Visible;
            AlcoholValue.Visibility = Visibility.Visible;
            UnitsNr.Visibility = Visibility.Visible;

            //SET RESULT VALUES
            //AlcoholValue.Text = Convert.ToString(EBAC);
            UnitsNr.Text = Convert.ToString(SD);
            //DoEvents() will force all the message in the message queue to be processed

            });
        }

    }

    public class Connection_manager 
    {
        private void GetResponseCallback(IAsyncResult rez)
        {
            HttpWebRequest hwr = rez.AsyncState as HttpWebRequest;
            HttpWebResponse response = hwr.EndGetResponse(rez) as HttpWebResponse;
            string a = (new StreamReader(response.GetResponseStream(), Encoding.UTF8)).ReadToEnd();


            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                MainPage.Update_user_profile(a);
                //MessageBox.Show(a);
            });
            
        }

        public void fetch_user_profile_from_taltioni(Model.User user)
        {
            string app_id = "wud9w25epyezyqoq6dxbwq137q5e743r";
            string access_token = user.Access_token;


            HttpWebRequest hwr = WebRequest.Create(new Uri("https://rest.taltioni.fi/" + "api/UserProfile")) as HttpWebRequest;
            hwr.Headers[System.Net.HttpRequestHeader.Authorization] = "Bearer " + access_token;
            hwr.Headers["X-ApplicationID"] = app_id;

            hwr.BeginGetResponse(GetResponseCallback, hwr);
        }

        private void GetWeightCallback(IAsyncResult rez)
        {
            HttpWebRequest hwr = rez.AsyncState as HttpWebRequest;
            HttpWebResponse response = hwr.EndGetResponse(rez) as HttpWebResponse;
            string a = (new StreamReader(response.GetResponseStream(), Encoding.UTF8)).ReadToEnd();


            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                //MainPage.Update_user_weigth(a);
                MessageBox.Show(a);
            });

        }

        public void fetch_user_weigth_from_taltioni(Model.User user)
        {
            string app_id = "wud9w25epyezyqoq6dxbwq137q5e743r";
            string access_token = user.Access_token;


            HttpWebRequest hwr = WebRequest.Create(new Uri("https://rest.taltioni.fi/" + "api/Observation/{Weigth}/GetLatest")) as HttpWebRequest;
            hwr.Headers[System.Net.HttpRequestHeader.Authorization] = "Bearer " + access_token;
            hwr.Headers["X-ApplicationID"] = app_id;

            hwr.BeginGetResponse(GetWeightCallback, hwr);
        }
    }

    public class User_profile
    {
        // Constructor, do nothing
        public User_profile() { }

        // Properties
        public AuditInfo AuditInfo { get; set; }
        public string State { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string Culture { get; set; }

    }
    public class AuditInfo
    {
        public string Created { get; set;}
        public string Lastupdated {get; set;} 
    }
}