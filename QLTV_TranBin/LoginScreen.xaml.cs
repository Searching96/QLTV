﻿using Microsoft.EntityFrameworkCore;
using QLTV_TranBin.Models;
using QLTV_TranBin.CQuang;
using QLTV_TranBin.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QLTV_TranBin
{
    /// <summary>
    /// Interaction logic for LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Window
    {
        private readonly QLTV2Context _context;

        public LoginScreen()
        {
            InitializeComponent();
            _context = new QLTV2Context();
            CheckSession();
            LoginMain.Content = new SignInScreen();
        }
        private void CheckSession()
        {
            string sessionToken = Settings.Default.SessionToken;
            if (!string.IsNullOrEmpty(sessionToken))
            {
                var activeSession = _context.ACTIVE_SESSION
                    .FirstOrDefault(s => s.SessionToken == sessionToken && s.ExpiryTime > DateTime.Now);
                if (activeSession != null)
                {
                    int currentRoleID = Settings.Default.CurrentUserPhanQuyen;
                    if (currentRoleID < 4)
                    {

                        AUInterface ms = new AUInterface();
                        ms.Show();
                    }
                    else
                    {
                        WUserWindow us = new WUserWindow();
                        us.Show();
                    }
                    this.Close();
                }
            }
        }
    }
}