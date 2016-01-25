using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Newtonsoft.Json;
using System.Text;
using System.Windows.Threading;
using System.Diagnostics;

namespace Alkoo.View
{
    public partial class UserProfile : PhoneApplicationPage
    {
        public static Model.User user;
        public UserProfile()
        {
            InitializeComponent();
            user = PhoneApplicationService.Current.State["param"] as Model.User;
            FirstName.Text = user.First_name;
            LastName.Text = user.Last_name;
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton(object sender, RoutedEventArgs e)
        {


            // SAVE BUTTON (PROFILE INFO)
            //Update user data into Taltioni or Save user data if it doesnt exist
            //int height = Convert.ToInt16(Height.Text);
            if (Weight.Text.Length > 0)
            {
                int weight = Convert.ToInt16(Weight.Text);
                Debug.WriteLine("Weight is " + weight);
            }
            string sex = "unknown";
            if (Male.IsChecked == true)
                //ale.Checked == true)//if (Male.IsPressed)
                sex = "male";
            else if (Female.IsChecked == true)
                sex = "female";
            Debug.WriteLine("Sex is " + sex);

            //Send parameters weight, drinkingStartDate and sex to next page view
            //NavigationService.Navigate(new Uri("/Page2.xaml?weight=" + Weight.Text, UriKind.Relative));
            //NavigationService.Navigate(new Uri("/Page2.xaml?sex=" + sex, UriKind.Relative));
            NavigationService.Navigate(new Uri("/View/MainPage.xaml?weight=" + Weight.Text + "&sex=" + sex, UriKind.Relative));

        }

        private void Height_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

    }

    public class Push_manager
    {
        public Push_manager() { }
        private void GetResponseCallback(IAsyncResult rez)
        {
            HttpWebRequest request = rez.AsyncState as HttpWebRequest;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(rez);
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    Debug.WriteLine(reader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                {
                    Debug.WriteLine(reader.ReadToEnd());
                }

                //HttpWebRequest hwr = rez.AsyncState as HttpWebRequest;
                //HttpWebRequest hwr = (HttpWebRequest)rez.AsyncState;
                //HttpWebResponse response = hwr.EndGetResponse(rez) as HttpWebResponse;
                //string a = (new StreamReader(response.GetResponseStream(), Encoding.UTF8)).ReadToEnd();

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MainPage.Update_user_profile(a);
                    //MessageBox.Show(a);
                });

            }
        }
        public void GetRequestStreamCallback(IAsyncResult asyncResult)
        {
            HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;

            Stream postStream = request.EndGetRequestStream(asyncResult);

            StringBuilder postData = new StringBuilder();
            //postData.Append(HttpUtility.UrlEncode(String.Format("{\"TypeId\": \"Alkoo\", \"EffectiveDateTime\": \"2013-08-12T11:30:38.83Z\",\"ObservationItems\": [{\"TypeId\": \"IrregularHeartbeatDetected\",\"BooleanValue\": \"True\",}]}")));
            postData.Append("{\"TypeId\": \"Alkoo\", \"EffectiveDateTime\": \"2013-09-12T11:30:38.83Z\",\"ObservationItems\": [{\"TypeId\": \"IrregularHeartbeatDetected\",\"BooleanValue\": \"True\"}],\"AuditInfo\":{\"SourceSystem\":\"My EHR\",\"IdInSourceSytems\": \"432534890HFGKLS\"}}");
            
            //postData.Append({"TypeId":"BloodPressure", "EffectiveDateTime":"2013-09-12T11:30:38.83Z", "Unit":"cm", "Value":"180"});

            //string json = JsonConvert.SerializeObject(l.ToDictionary(postData);

            //MemoryStream stream = new MemoryStream();
            //DataContractJsonSerializer sr = new DataContractJsonSerializer(postData.GetType());
            //sr.WriteObject(stream, postData);
            //stream.Position = 0;
            //StreamReader reader = new StreamReader(stream);
            //string jsonResult = reader.ReadToEnd();

            Debug.WriteLine(postData);

            //Make the string UTF-8
            byte[] byteArray = Encoding.UTF8.GetBytes(postData.ToString());
            postStream.Write(byteArray, 0, postData.Length);
            //byte[] byteArray = Encoding.UTF8.GetBytes(jsonResult.ToString());
            //postStream.Write(byteArray, 0, jsonResult.Length);
            postStream.Close();

            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);

        }

        public void Push_user_weight_to_taltioni(Model.User user)
        {

            string app_id = "wud9w25epyezyqoq6dxbwq137q5e743r";
            string access_token = user.Access_token;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("https://rest.taltioni.fi/" + "api/observation/Alkoo"));

            request.Headers[System.Net.HttpRequestHeader.Authorization] = "Bearer " + access_token;
            request.Headers["X-ApplicationID"] = app_id;

            request.ContentType = "application/json, text/json";
            request.Method = "POST";

            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
        }

    }
}