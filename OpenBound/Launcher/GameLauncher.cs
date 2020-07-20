/* 
 * Copyright (C) 2020, Carlos H.M.S. <carlos_judo@hotmail.com>
 * This file is part of OpenBound.
 * OpenBound is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or(at your option) any later version.
 * 
 * OpenBound is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with OpenBound. If not, see http://www.gnu.org/licenses/.
 */

using OpenBound.Common;
using OpenBound.GameComponents.Audio;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.FileOutput;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Extension;
using Openbound_Network_Object_Library.TCP.ServiceProvider;
using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using OpenBound.Launcher.Connection;
using Openbound_Network_Object_Library.Models;

namespace OpenBound.Launcher
{
    public partial class GameLauncher : Form
    {
        private SignUpForm _signUpForm;
      
        private LauncherRequestManager _launcherRequestManager;
        private bool _isLoginPossible = true;
        public GameLauncher()
        {
            InitializeComponent();
            _launcherRequestManager = new LauncherRequestManager();
            _signUpForm = new SignUpForm(_launcherRequestManager);
            txtNickname.Text = _launcherRequestManager.GetSavedNickname();
        }
        public void CheckLoginResult(Thread thread)
        {
            int elapsedTimeInMilliseconds = 0;

            do
            {
                lock (_launcherRequestManager.LoginResult)
                {
                    if (_launcherRequestManager.LoginResult == "success")
                    {
                        DialogResult = DialogResult.OK;
                        BeginInvoke(new Action(() => Close()));
                        break;
                    }
                    else if (_launcherRequestManager.LoginResult != "")
                    {
                        MessageBox.Show(_launcherRequestManager.LoginResult);
                        break;
                    }
                }

                if (thread.IsAlive && elapsedTimeInMilliseconds >= 15000)
                    thread.Abort();

                Thread.Sleep(100);
                elapsedTimeInMilliseconds += 100;

            } while (true);

            _launcherRequestManager.LoginResult = "";
            _isLoginPossible = true;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (!_isLoginPossible) return;
            _launcherRequestManager.PrepareLoginThread(txtNickname.Text, txtPassword.Text);
            _launcherRequestManager.LauncherThread = new Thread(() => CheckLoginResult(_launcherRequestManager.RequestThread));
            _launcherRequestManager.LauncherThread.IsBackground = true;
            _launcherRequestManager.LauncherThread.Start();
            _isLoginPossible = false;
        }

        private void BtnSignup_Click(object sender, EventArgs e)
        {
            _signUpForm.ShowDialog();
        }

        private void LoginTextbox_TextChanged(object sender, EventArgs e)
        {
            btnLogin.Enabled = txtNickname.Text.Length > 2 && txtPassword.Text.Length > 2;
        }

    }
}
