using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenBound.Common;
using OpenBound.GameComponents.Audio;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Extension;
using Openbound_Network_Object_Library.FileOutput;
using Openbound_Network_Object_Library.Models;
using Openbound_Network_Object_Library.TCP.ServiceProvider;
using Openbound_Network_Object_Library.ValidationModel;

namespace OpenBound.Launcher.Connection
{
    public class LauncherRequestManager
    {
        private GameClientSettingsInformation _gcsi;
        public Thread LauncherThread;
        public Thread RequestThread;
        public string LoginResult = "";
        public string RegistrationResult = "";

        public LauncherRequestManager()
        {
            SetGameClientInformation();
        }

        public string GetSavedNickname()
        {
            return _gcsi.SavedNickname;
        }

        private void SetGameClientInformation()
        {
            ConfigFileManager.CreateConfigFile(RequesterApplication.Launcher);
            ConfigFileManager.LoadConfigFile(RequesterApplication.Launcher);
            _gcsi = ConfigFileManager.ReadClientInformation();
            Parameter.Initialize(_gcsi);
            AudioHandler.Initialize(_gcsi);
        }

        public void PrepareLoginThread(string nickname, string password)
        {
            RequestThread = new Thread(() => Login(nickname, password));
            RequestThread.Start();
        }

        public void PrepareRegistration(Account account)
        {
            RequestThread = new Thread(() => DoRegistration(account));
            RequestThread.Start();
        }

        public void DoRegistration(Account account)
        {
            try
            {
                bool waiting = false;
                ClientServiceProvider csp = new ClientServiceProvider(
                    NetworkObjectParameters.LoginServerInformation.ServerLocalAddress,
                    NetworkObjectParameters.LoginServerInformation.ServerPort,
                    NetworkObjectParameters.LoginServerBufferSize,
                    NetworkObjectParameters.LoginServerBufferSize,
                    (serviceProvider, message) =>
                    {
                        RegistrationResult = ObjectWrapper.DeserializeRequest<string>(message[1]);
                        waiting = true;
                    });

                csp.StartOperation();
                csp.RequestList.Enqueue(NetworkObjectParameters.LoginServerAccountCreationRequest, account);
                while (!waiting) Thread.Sleep(100);

                csp.StopOperation();
            }
            catch (Exception ex)
            {
                RegistrationResult = ex.Message;
            }
        }

        public void Login(string nickname, string password)
        {
            try
            {
                //Preparing player variable
                Player player = new Player()
                {
                    Nickname = nickname,
                    Password = password
                };

                bool waiting = false;

                ClientServiceProvider csp = new ClientServiceProvider(
                    NetworkObjectParameters.LoginServerInformation.ServerLocalAddress,
                    NetworkObjectParameters.LoginServerInformation.ServerPort,
                    NetworkObjectParameters.LoginServerBufferSize,
                    NetworkObjectParameters.LoginServerBufferSize,
                    (serviceProvider, message) =>
                    {
                        player = ObjectWrapper.DeserializeRequest<Player>(message[1]);
                        waiting = true;
                    });
                csp.StartOperation();
                csp.RequestList.Enqueue(NetworkObjectParameters.LoginServerLoginAttemptRequest, player);

                while (!waiting) Thread.Sleep(100);

                csp.StopOperation();

                if (player == null || player.ID == 0)
                {
                    lock (LoginResult) 
                        LoginResult = "Player not found.";
                }
                else
                {
                    GameInformation.Instance.PlayerInformation = player;
                    lock (LoginResult)
                        LoginResult = "success";
                }
            }
            catch (Exception ex)
            {
                lock (LoginResult)
                    LoginResult = ex.Message;
            }

        }
    }
}
