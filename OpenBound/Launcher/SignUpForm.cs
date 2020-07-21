using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenBound.Launcher.Connection;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Extension;
using Openbound_Network_Object_Library.Helper;
using Openbound_Network_Object_Library.Models;
using Openbound_Network_Object_Library.TCP.ServiceProvider;
using Openbound_Network_Object_Library.ValidationModel;

namespace OpenBound.Launcher
{
    public partial class SignUpForm : Form
    {
        private LauncherRequestManager _launcherRequestManager;
        private bool _isRegistrationPossible = true;
        public SignUpForm(LauncherRequestManager launcherRequestManager)
        {
            InitializeComponent();
            _launcherRequestManager = launcherRequestManager;
#if DEBUG
            btnRegisterDebug.Visible = true;
            btnRegisterDebug.Enabled = true;
#endif
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
             Account newAccount = new Account
            {
                Email = txtEmail.Text,
                Nickname = txtNickname.Text,
                CharacterGender = rdbMale.Checked ? Gender.Male : Gender.Female,
                Password = txtPassword.Text,
                PasswordConfirmation = txtPasswordConfirmation.Text
            };

            if (newAccount.Validate())
            {
                RegisterAccount(newAccount);
            }
            else
            {
                MessageBox.Show(newAccount.ValidationErrorsToString(), "Account creation error");
            }
        }

        private void RegisterAccount(Account newAccount)
        {
            if (!_isRegistrationPossible) return;
            _launcherRequestManager.PrepareRegistration(newAccount);
            _launcherRequestManager.LauncherThread = new Thread(() => 
                CheckRegistrationResult(_launcherRequestManager.RequestThread));
            _launcherRequestManager.LauncherThread.Start();
            _isRegistrationPossible = false;
        }

        public void CheckRegistrationResult(Thread thread)
        {
            int elapsedTimeInMilliseconds = 0;

            do
            {
                lock (_launcherRequestManager.RegistrationResult)
                {
                    if (_launcherRequestManager.RegistrationResult == "success")
                    {
                        MessageBox.Show("Account successfully created", "Account creation");
                        lock (this)
                        {
                            ClearForm();
                            FormsHelper.SetControlPropertyThreadSafe(this, "Visible", false);
                        }
                        break;
                    }
                    else if (_launcherRequestManager.RegistrationResult != "")
                    {
                        MessageBox.Show(_launcherRequestManager.RegistrationResult);
                        break;
                    }
                }

                if (thread.IsAlive && elapsedTimeInMilliseconds >= 15000)
                    thread.Abort();

                Thread.Sleep(100);
                elapsedTimeInMilliseconds += 100;

            } while (true);

            _launcherRequestManager.RegistrationResult = "";
            _isRegistrationPossible = true;
        }

        private void ClearForm()
        {
            rdbMale.Checked = true;
            foreach (TextBox txt in gpbAccount.Controls.OfType<TextBox>())
                FormsHelper.SetControlPropertyThreadSafe(txt, "Text", "");
        }

        private void Textbox_TextChanged(object sender, EventArgs e)
        {
            ttpValidation.Show(((TextBox)sender).Tag.ToString(), gpbAccount, 
                new Point(((TextBox)sender).Location.X + ((TextBox)sender).Width, ((TextBox)sender).Location.Y));
        }

        private void Textbox_Leave(object sender, EventArgs e) => ttpValidation.Hide((TextBox)sender);
        private void BtnClose_Click(object sender, EventArgs e) => Hide();
        private void BtnRegisterDebug_Click(object sender, EventArgs e) => RegisterAccount(new Account()
        {
            Nickname = "Sopa",
            Email = "sopa@hotmail.com",
            Password = "123456",
            PasswordConfirmation = "123456",
            CharacterGender = Gender.Female
        });


    }
}
