﻿using Android.App;
using Android.Views;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using System;
using System.Collections.Generic;
using Contacts.Models;
using Android.Content.PM;
using System.Text.RegularExpressions;
using System.Linq;

namespace Contacts
{
    //Label = "Contacts",
    [Activity( MainLauncher = true, Icon = "@drawable/icon",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        public static List<Contact> ContactList = new List<Contact>();

        public static List<Contact> ContactListNew = new List<Contact>();

        ListView _listView;
        Dialog _dialog;
        bool _IsContactNew;
        Contact _contactSelected;
        public static ListAdapter ListAdapter;

        #region Contructor

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            //Toolbar Set-Up
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            //List Set-Up
            LoadList();
            _listView = FindViewById<ListView>(Resource.Id.List);

            ListAdapter = new ListAdapter(this, ContactList);
            _listView.Adapter = ListAdapter;
            _listView.ItemClick += OnListItemClick;
            Button searchButton = (Button)FindViewById(Resource.Id.buttonsearch);
            searchButton.Click += searchButton_Click;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.home, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_add:
                    _IsContactNew = true;
                    CreateDialog();
                    _dialog.Show();
                    break;
                case Resource.Id.menu_contacts:
                    AddContactFromPhone();
                    break;
                case Resource.Id.menu_qr:
                    var scanner = new ZXing.Mobile.MobileBarcodeScanner();
                    RunOnUiThread(async () =>
                    {
                        var result = await scanner.Scan();
                        if (!string.IsNullOrEmpty(result?.Text))
                        {
                            SetUpAndShowAlert(result.Text);
                        }
                    });
                    break;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Events

        void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var listView = sender as ListView;
            _contactSelected = ContactList[e.Position];
            _IsContactNew = false;

            SetUpAndShowAlert();

        }
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            
           
            Toast.MakeText(this, "delete success!", ToastLength.Short).Show();

            ContactList.Remove(ContactList.FirstOrDefault(c => c.Id == _contactSelected.Id));
            ListAdapter.NotifyDataSetChanged();
            _dialog.Dismiss();

         
          //  Finish();
        }
        private void DoneButton_Click(object sender, EventArgs e)
        {
            EditText name = (EditText)_dialog.FindViewById(Resource.Id.firstNameTxt);
            EditText lastName = (EditText)_dialog.FindViewById(Resource.Id.lastNameTxt);
            EditText phone = (EditText)_dialog.FindViewById(Resource.Id.phoneNumberTxt);
            EditText email = (EditText)_dialog.FindViewById(Resource.Id.emailTxt);

            int isValid = ValidateData(name.Text, lastName.Text, phone.Text, email.Text);

            if (isValid != 0)
            {
                switch (isValid)
                {
                    case 1:
                        Toast.MakeText(this, "Information Missing!", ToastLength.Short).Show();
                        break;
                    case 2:
                        Toast.MakeText(this, "Invalid Email!", ToastLength.Short).Show();
                        break;
                    case 3:
                        Toast.MakeText(this, "Invalid Phone!", ToastLength.Short).Show();
                        break;
                    default:
                        break;
                }
                return;
            }

            if (_IsContactNew)
            {
                //Add new Contact
                ContactList.Add(new Contact() { Name = name.Text, LastName = lastName.Text, Phone = phone.Text, Email = email.Text });
                ListAdapter.NotifyDataSetChanged();
            }
            else
            {
                //Edit Contact
                var contactToBeUpdated = ContactList.FirstOrDefault(c => c.Id == _contactSelected.Id);
                if (contactToBeUpdated == null)
                {
                    Toast.MakeText(this, "Invalid Contact!", ToastLength.Short).Show();
                    return;
                }

                contactToBeUpdated.Name = name.Text;
                contactToBeUpdated.LastName = lastName.Text;
                contactToBeUpdated.Phone = phone.Text;
                contactToBeUpdated.Email = email.Text;

            }

            _dialog.Dismiss();
        }

        #endregion

        #region Helpers

        private void AddContactFromPhone()
        {
            StartActivity(typeof(ContactList));
        }

        void CreateDialog()
        {
            _dialog = new Dialog(this);
            _dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
            _dialog.SetContentView(Resource.Layout.custom_dialog_layout);
            Button dismissButton = (Button)_dialog.FindViewById(Resource.Id.dialog_button);
            dismissButton.Click += DoneButton_Click;
            Button deleteButton = (Button) _dialog.FindViewById(Resource.Id.button1);
            deleteButton.Click += DeleteButton_Click;

          

        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            // Toast.MakeText(this, "delete success!", ToastLength.Short).Show();
            // _contactSelected
            ContactListNew.AddRange(ContactList);
            ContactListNew= ContactListNew.Distinct().ToList();
            EditText name = (EditText)FindViewById(Resource.Id.searchTxt);
            if (!string.IsNullOrEmpty(name.Text))
            {
                ContactList = ContactListNew.FindAll(c => c.Name.Contains(name.Text));
                //ListAdapter.NotifyDataSetChanged();
                // _dialog.Dismiss();
                ListAdapter = new ListAdapter(this, ContactList);
                _listView.Adapter = ListAdapter;
               // _listView.ItemClick += OnListItemClick;
                //Button searchButton = (Button)FindViewById(Resource.Id.buttonsearch);
                //searchButton.Click += searchButton_Click;




            }

           
        }


        public static int ValidateData(string name, string lastName, string phone, string email)
        {
            // || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(email)
            if (string.IsNullOrEmpty(name))
            {
                return 1;
            } else if (!IsEmailValid(email) && !string.IsNullOrEmpty(email))
            {
                return 2;
            }else if (IsPhoneValid(phone) && !string.IsNullOrEmpty(phone))
            {
                return 3;
            }
            else
            {
                return 0;
            }
        }

        public static bool IsPhoneValid(string phone)
        {
            return Regex.Match(phone, @"^(\+[0-9]{9})$").Success;
        }

        public static bool IsEmailValid(string emailAddress)
        {
            var regex = new Regex(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$");
            var match = regex.Match(emailAddress);
            return match.Success;
        }

        void LoadList()
        {
            ContactList.Add(new Contact() { Id = 1, Name = "Abel", LastName = "Hu", Phone = "(582) 399-8411", Email = "abhu@gmail.com" });
            ContactList.Add(new Contact() { Id = 2, Name = "Alan", LastName = "Wang", Phone = "(721) 906-1017", Email = "alanw14@gmail.com" });
            ContactList.Add(new Contact() { Id = 3, Name = "Christopher", LastName = "Liu", Phone = "(780) 903-4122", Email = "chrislui_1@gmail.com" });
            ContactList.Add(new Contact() { Id = 4, Name = "David", LastName = "Smith", Phone = "(197) 448-5930", Email = "dasmith@gmail.com" });
            ContactList.Add(new Contact() { Id = 5, Name = "Hank", LastName = "Scorpio", Phone = "(733) 584-7015", Email = "hankscs@gmail.com" });
            ContactList.Add(new Contact() { Id = 6, Name = "Anna", LastName = "Frank", Phone = "(555) 986-3924", Email = "anniefrankm@gmail.com" });

        }

        void SetUpAndShowAlert(string contact = null)
        {
            CreateDialog();

            TextView initials = (TextView)_dialog.FindViewById(Resource.Id.initialsDialog);
            EditText name = (EditText)_dialog.FindViewById(Resource.Id.firstNameTxt);
            EditText lastName = (EditText)_dialog.FindViewById(Resource.Id.lastNameTxt);
            EditText phone = (EditText)_dialog.FindViewById(Resource.Id.phoneNumberTxt);
            EditText email = (EditText)_dialog.FindViewById(Resource.Id.emailTxt);

            if (_contactSelected == null)
            {

                try
                {
                    var separatedContact = contact.Split(' ');
                    switch (separatedContact.Length)
                    {
                        case 1:
                            name.Text = separatedContact[0];
                            break;
                        case 2:
                            name.Text = separatedContact[0];
                            lastName.Text = separatedContact[1];
                            break;
                        case 3:
                            name.Text = separatedContact[0];
                            lastName.Text = separatedContact[1];
                            phone.Text = separatedContact[2];
                            break;
                        case 4:
                            name.Text = separatedContact[0];
                            lastName.Text = separatedContact[1];
                            phone.Text = separatedContact[2];
                            email.Text = separatedContact[3];
                            break;
                        default:
                            Toast.MakeText(this, "Invalid QR Code!", ToastLength.Short).Show();
                            return;
                    }

                    _IsContactNew = true;

                }
                catch
                {
                    Toast.MakeText(this, "Invalid QR Code!", ToastLength.Short).Show();
                }
            }
            else
            {
                initials.Text = _contactSelected.Initials;
                name.Text = _contactSelected.Name;
                lastName.Text = _contactSelected.LastName;
                phone.Text = _contactSelected.Phone;
                email.Text = _contactSelected.Email;
            }

            _dialog.Show();
        }

        #endregion

    }
}

