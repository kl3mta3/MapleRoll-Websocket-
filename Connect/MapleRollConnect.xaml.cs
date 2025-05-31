using MapleRoll2.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using MapleRoll2;
using MapleRoll2.Net.IO;
using System.Net.Sockets;
using System.Windows.Shell;
using System.Windows.Controls;
using System.Windows.Media;
using MapleRoll2.Roll;
using System.Windows.Input;
using System.IO;
using System.IO.Packaging;
using System.Windows.Shapes;
using System.Media;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography;
using MapleRoll2.Greed;
using System.Drawing;
using System.Windows.Interop;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Configuration;

namespace MapleRoll2.Connect
{

    public partial class MapleRollConnect : Window
    {

        public static List<User> groupMembers = new List<User>();
        public static User thisUser = new User();
        public ConnectionError connectionError = new ConnectionError();
        public static Dictionary<User, int> currentRoll = new Dictionary<User, int>();
        
       // public static Server _server { get; set; }
       public static WebSocketClient _clientSocket {  get; set; }
        public static MainWindow mainWindow { get; set; }
        public static RollWindow rollWindow { get; set; }
        public static NeedGreedWindow needGreedWindow { get; set; }
       // PacketReader packetReader { get; set; }
        public static string userName { get; set; }
        public static string groupId { get; set; }
        public static string uId { get; set; }
        public bool isConnectedToServer = false;
        public static int userSoundProfile = 1;
        //roll data
        public static int winningRoll = 0;
        public static int tiedRoll = 0;
        public static User winningRollUser = new User();
        public static int userRoll = 0;
        public static bool hasRolled = false;
        public static bool rollStarted = false;
        public static List<User> tiedRollUsers = new List<User>();

        //Stats data
        public static decimal totalUserRollCount = 0;
        public static decimal userTotalRollSum = 0;
        public static decimal totalPerfectRollCount = 0;
        public static decimal totalOneRolls = 0;
        public static decimal totalWinningRolls = 0;
        private static string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string statsSavePath = System.IO.Path.Combine(systemPath, @"MapleRoll");
        private static string fileName = "stats.txt";
        private static string filePath = System.IO.Path.Combine(statsSavePath, $"{fileName}");

        //Vote to end roll info
        public static int votesForEndingRoll = 0;
      
        public static bool voteToEndRollStarted=false;
        public static bool userHasVotedToEndRoll = false;
        public static bool rollToEndVoteDone = false;
        public static List<string> endRollVotes = new List<string>();

        //need > greed

        public static Dictionary<User, int> needRolls = new Dictionary<User, int>();
        public static Dictionary<User, int> greedRolls = new Dictionary<User, int>();
        public static User winningNeedUser= new User();
        public static User winningGreedUser = new User();
        public static User winningNeedGreedUser = new User();
        public static bool hasNeedGreedRolled = false;
        public static bool needGreedRollStarted = false;
        public static bool isNeedRoll = false;
        public static bool isGreedRoll = false;
        public static bool someoneRolledNeed = false;
        public static bool userRolledNeed = false;
        public static bool userRolledGreed = false;
        public static int winningNeedRoll = 0;
        public static int winningGreedRoll = 0;
        public static int winningNeedGreedRoll = 0;
        public static int tiedGreedRoll = 0;
        public static int tiedNeedRoll = 0;
        public static int userNeedGreedRoll = 0;
        public static bool signOut = false;
        public static bool rollToEndNeedGreedVoteDone = true;
        public static List<string> endNeedGreedRollVotes= new List<string>();
        public static int votesForEndingNeedGreedRoll = 0;
        public static bool userHasVotedToEndNeedGreedRoll = false;
        public static bool needgreedpass=false;
        public static bool rollpass=false;
        public static string ServerUrl =  ConfigurationManager.AppSettings["ServerUrl"];
        public MapleRollConnect()
        {
            InitializeComponent();
            rollWindow = new RollWindow();

        }
        public static async Task Disconnect()
        {
            await _clientSocket.DisconnectFromServer();
            groupMembers.Clear();
            if (signOut)
            {

             MainWindow.connectWindow.Show();
            Application.Current.Dispatcher.Invoke((() => { mainWindow.Hide(); }));
            signOut=false;
            }
        }
        public static void LoadUserStats()
        {

            Console.WriteLine("Checking for save file");
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);


                Console.WriteLine("Save Found Loading Data");



                decimal _totalUserRollCount = decimal.Parse(lines[1]);
                decimal stat1 = DecryptStat(_totalUserRollCount);
                totalUserRollCount = stat1 ;

                decimal _userTotalRollSum = decimal.Parse(lines[2]);
                decimal stat2 = DecryptStat(_userTotalRollSum);
                userTotalRollSum = stat2;
                Console.WriteLine($"userTotalRollSum = {userTotalRollSum}");

                decimal _totalWinningRolls = decimal.Parse(lines[3]);
                decimal stat3 = DecryptStat(_totalWinningRolls);
                totalWinningRolls = stat3;
                Console.WriteLine($"totalWinningRollsm = {totalWinningRolls}");

                decimal _totalOneRolls = decimal.Parse(lines[4]);
                decimal stat4 = DecryptStat(_totalOneRolls);
                totalOneRolls = stat4;
                Console.WriteLine($"totalOneRolls = {totalOneRolls}");

                decimal _totalPerfectRollCount = decimal.Parse(lines[5]);
                decimal stat5 = DecryptStat(_totalPerfectRollCount);
                totalPerfectRollCount = stat5;
                Console.WriteLine($"totalPerfectRollCount = {totalPerfectRollCount}");

                mainWindow.UpdateUserStatDisplay();
                Console.WriteLine("Loaded Data");
            }
            else
            {

                Console.WriteLine("No Save file Found.");
            }
        }
        public static void SaveUserStats()
        {
            Console.WriteLine("Saving Roll Data");
            if (File.Exists(filePath))
            {
                Console.WriteLine(" Old save found, deleting it.");
                File.Delete(filePath);
            }
            else
            {
                if (!Directory.Exists(statsSavePath))
                {
                    Directory.CreateDirectory(statsSavePath);

                }

            }
            //FileStream fs = File.CreateText(filePath);
            using (StreamWriter sw = new StreamWriter(filePath))
            {

                    sw.WriteLine(DateTime.Now.ToString());
                if (totalUserRollCount>0)
                {

                    decimal stat1 = EncryptStat(totalUserRollCount);
                    sw.WriteLine($"{stat1}");
                }
                else
                {
                    sw.WriteLine($"{totalUserRollCount}");

                }

            if(userTotalRollSum>0)
            {
                    decimal stat2 = EncryptStat(userTotalRollSum);
                    sw.WriteLine($"{stat2}");

             }
            else 
             { 
                
                sw.WriteLine($"{userTotalRollSum}");
            }

                if (totalWinningRolls>0)
                {
                    decimal stat3 = EncryptStat(totalWinningRolls);
                    sw.WriteLine($"{stat3}");

                }
                else 
                { 
              
                sw.WriteLine($"{totalWinningRolls}");
                }

                if (totalOneRolls>0)
                {
                    decimal stat4 = EncryptStat(totalOneRolls);
                    sw.WriteLine($"{stat4}");
                }
                else
                {
                    sw.WriteLine($"{totalOneRolls}");
                }

                if (totalPerfectRollCount>0)
                {
                    decimal stat5 = EncryptStat(totalPerfectRollCount);
                    sw.WriteLine($"{stat5}");
                }
                else
                {
                   
                    sw.WriteLine($"{totalPerfectRollCount}");
                }
                    sw.WriteLine($"~END~");

                }
            mainWindow.UpdateUserStatDisplay();
            Console.WriteLine("Data Saved.");
        }
        public static decimal EncryptStat(decimal stat)
        {
            if (stat>0)
            {
                decimal number = ((stat * 69) / 2);
                return number;
            }
            else
            {
                return 0;
            }
        }
        public static decimal DecryptStat(decimal stat)
        {
            if (stat > 0)
            {
                decimal number = ((stat * 2) / 69);
                return number;
            }
            else
            {
                return 0;
            }

        }
        public async void btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txb_UserName.Text) && !string.IsNullOrEmpty(txb_GroupId.Text) && txb_GroupId.Text.Length==6)
            {

                try
                {
                    // Set up WebSocket Client (public IP: 76.143.47.68)
                    var serverUrl = $"ws://{ServerUrl}/"; 
                     _clientSocket = new WebSocketClient(serverUrl);
                    Console.WriteLine($"Attempting to connect to WebSocket Server at: {serverUrl}");
                    // Set up event handlers

                    _clientSocket.OnConnected += async () =>
                    {
                        mainWindow = new MainWindow();
                       
                        userName = txb_UserName.Text;

                        groupId = txb_GroupId.Text;
                        Dispatcher.Invoke(() =>
                        {
                            isConnectedToServer = true;
                            this.Dispatcher.Invoke(() =>
                            {
                                mainWindow.Show();
                                this.Hide();
                                Dispatcher.Invoke(() =>
                                {
                                   mainWindow.SendMessageToConsole($"Welcome {userName}. May the RNG be with you...", 1);
                                });
                            });
                            Console.WriteLine($"Connected as {txb_UserName.Text}");
                        });

                        // Send connection data
                        _ = _clientSocket.SendMessageAsync(new
                        {
                            Type = "connection",
                            GroupID = groupId,
                            Username = txb_UserName.Text
                        });
                    };

                    _clientSocket.OnMessageReceived += (message) =>
                    {
                        var parsedMessage = JsonSerializer.Deserialize<Dictionary<string, string>>(message);
                        if (parsedMessage != null && parsedMessage.ContainsKey("Type"))
                        {
                            var type = parsedMessage["Type"];
                            HandleServerMessage(type, parsedMessage);
                        }
                    };

                    _clientSocket.OnDisconnected += () =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            isConnectedToServer = false;
                            MessageBox.Show("Disconnected from server.");


                        });
                    };

                    // Connect to the server
                    await _clientSocket.ConnectAsync();
                 
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }

                   
            }
            else if (!string.IsNullOrEmpty(txb_UserName.Text) && !string.IsNullOrEmpty(txb_GroupId.Text) && txb_GroupId.Text.Length != 6)
            {

                MessageBox.Show($"{txb_GroupId.Text}is not a Valid Group ID. A group ID should be 6 digits in length and belongs to an existing group. \n If you do not have a valid Group ID start a new Group.");

            }
            else if (string.IsNullOrEmpty(txb_UserName.Text) && string.IsNullOrEmpty(txb_GroupId.Text))
            {

                MessageBox.Show("Please Enter a Username and a valid 6 diget Group ID, or start a new group.");
            }
            else if (string.IsNullOrEmpty(txb_UserName.Text) && !string.IsNullOrEmpty(txb_GroupId.Text)&& txb_GroupId.Text.Length==6)
            {

                MessageBox.Show("Please enter a UserName");

            }
            else if (!string.IsNullOrEmpty(txb_UserName.Text) && string.IsNullOrEmpty(txb_GroupId.Text))
            {

                MessageBox.Show("Please enter a vaild 6 digit group ID to join a group. \n Otherwise create a new group.");

            }



        }

        private async void btn_ConnectNew_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txb_UserName.Text) && string.IsNullOrEmpty(txb_GroupId.Text))
            {
                //_server = new Server();
                mainWindow = new MainWindow();
                userName = txb_UserName.Text;
                groupId = "";

                //add connection to webserver here without known group **CREATE NEW
                try
                {      
                    // Set up WebSocket Client
                    var serverUrl = "ws://10.0.0.219:42069/";
                    _clientSocket = new WebSocketClient(serverUrl);

                    // Set up event handlers
                    _clientSocket.OnConnected += async () =>
                    {


                        mainWindow = new MainWindow();
                        MainWindow.connectWindow = this;
                        //_clientSocket._mainWindow = mainWindow;
                        userName = txb_UserName.Text;
                        groupId = "";
                        Dispatcher.Invoke(() =>
                        {
                            isConnectedToServer = true;
                            this.Dispatcher.Invoke(() =>
                            {
                                mainWindow.Show();
                                this.Hide();
                            });
                            Console.WriteLine($"Connected as {txb_UserName.Text}");
                        });

                        // Send connection data
                        _ = _clientSocket.SendMessageAsync(new
                        {
                            Type = "connection",
                            GroupID = groupId,
                            Username = txb_UserName.Text,
                            UID=uId
                        });
                    };

                    _clientSocket.OnMessageReceived += (message) =>
                    {
                        var parsedMessage = JsonSerializer.Deserialize<Dictionary<string, string>>(message);
                        if (parsedMessage != null && parsedMessage.ContainsKey("Type"))
                        {
                            var type = parsedMessage["Type"];
                            HandleServerMessage(type, parsedMessage);
                        }
                    };

                    _clientSocket.OnDisconnected += () =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            isConnectedToServer = false;
                            MessageBox.Show("Disconnected from server.");
                        });
                    };

                 
                    // Connect to the server
                    await _clientSocket.ConnectAsync();
               


                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }

            }
            else if(string.IsNullOrEmpty(txb_UserName.Text) && string.IsNullOrEmpty(txb_GroupId.Text))
            {

                    MessageBox.Show("Please Enter a Username");
            }
            else if(!string.IsNullOrEmpty(txb_UserName.Text) && !string.IsNullOrEmpty(txb_GroupId.Text))
            {

                MessageBox.Show("If you wish to create a new group do not enter a group ID, if you wish to join a group hit JOIN group instead.");

            }
            else if (string.IsNullOrEmpty(txb_UserName.Text) && !string.IsNullOrEmpty(txb_GroupId.Text))
            {

                MessageBox.Show("Please enter a UserName. If you wish to create a new group do not enter a group ID, if you wish to join a group hit JOIN group instead.");

            }
        }

        //new handlers 
        private void HandleServerMessage(string type, Dictionary<string, string> messageData)
        {
            switch (type)
            {
                case "connection":
                    HandleConnectionMessage(messageData);
                    break;

                case "UserConnection":
                    HandleUserConnectionMessage(messageData);
                    break;

                case "badgroupid":
                    HandleUserBadGroupIDMessage(messageData);
                    break;

                case "message":
                    HandleGeneralMessage(messageData);
                    break;

                case "roll":
                    HandleRollMessage(messageData);
                    break;

                case "need":
                    HandleNeedRollMessage(messageData);
                    break;

                case "greed":
                    HandleGreedRollMessage(messageData);
                    break;

                case "flip":
                    HandleFlipMessage(messageData);
                    break;

                case "rps":
                    HandleRpsMessage(messageData);
                    break;

                case "endroll":
                    HandleEndRollVoteMessage(messageData);
                    break;

                case "endneed":
                    HandleEndNeedGreedVoteMessage(messageData);
                    break;

                case "private":
                    HandlePrivateMessage(messageData);
                    break;

                case "system_message":
                    HandleSystemMessage(messageData);
                    break;

                case "kicked":
                    HandleKickMessage(messageData);
                    break;

                case "group_migration":
                    HandleGroupMigration(messageData);
                    break;

                case "disconnection":
                    HandleDisconnectionMessage(messageData);
                    break;

                default:
                    Console.WriteLine($"Unhandled message type: {type}");
                    break;
            }
        }
        private void HandleConnectionMessage(Dictionary<string, string> messageData)
        {
            var username = messageData["Username"];
            var uID = messageData["UID"];
            Console.WriteLine($"Connection event recieved user{uID}");
            
            UserConnected(username, uID);
        }

        private void HandleDisconnectionMessage(Dictionary<string, string> messageData)
        {

            
            var uID = messageData["UID"];
            Console.WriteLine($"Disconnection event recieved user{uID}");
            UserDisconnected(uID);

        }

        private void HandleUserConnectionMessage(Dictionary<string, string> messageData)
        {
            var username = messageData["Username"];
            var UID = messageData["UID"];

            var groupID = messageData["GroupID"];
            Console.WriteLine($"User Connection Message recieved user's{UID}");
            if (groupId == null || groupId == "")
            {
                groupId = groupID;
                Console.WriteLine($"GROUP ID RECIEVED and updated to {groupId}");
            }
            uId = UID;
            userName = username;
            groupId= groupID;
            mainWindow.UpdateConnectionInfo(uId, username, groupId);

            UserConnected(username, uId);
        }

        private void HandleUserBadGroupIDMessage(Dictionary<string, string> messageData)
        {
            Dispatcher.Invoke(() =>
            {
                mainWindow.Hide();
                this.Show();
                userName = "";
                txb_UserName.Text = "";

                groupId = "";
                txb_GroupId.Text = "";
                MessageBox.Show(
                     "That Group doesn't exist. Please enter a valid group ID or start a new Group.",
                     "Error",
                     MessageBoxButton.OK,
                     MessageBoxImage.Error
                );


            });
   


        }

        public User GetUserByUID(string uid)
        {
           User user = new User();
            user=null;

            foreach (User _user in groupMembers) 
            {
                if (_user.UID == uid)
                {

                    Console.WriteLine($"GetuserbyUID: User Found");
                    user = _user;
                    return user;
                }
            
            }
            Console.WriteLine($"GetuserbyUID: User not Found");
            return user;
        }
        private void HandleGeneralMessage(Dictionary<string, string> messageData)
        {
            var groupID = messageData["GroupID"];
            var message = messageData["Message"];
            var uid = messageData["UID"];
            var username = GetUserByUID(uid).Username;
           
            Console.WriteLine($"Message in group {groupID}: {message}");

            Dispatcher.Invoke(() =>
            {
                mainWindow.SendMessageToConsole($"[{username}]: {message}", 5);
            });
        }
        private void HandleRollMessage(Dictionary<string, string> messageData)
        {
            var groupID = messageData["GroupID"];
            var uid = messageData["UID"];
            string username = GetUsernamebyUID(uid);
            var roll = int.Parse(messageData["Roll"]);
            Console.WriteLine($"Roll Recieved. User {username} rolled a {roll}.");

                ProcessRoll(uid, roll);
            //Dispatcher.Invoke(() =>
            //{
            //    mainWindow.SendMessageToConsole($"[Roll]:{username} rolled a {roll}.", 6);
            //});
        }
        private void HandleNeedRollMessage(Dictionary<string, string> messageData)
        {
            var groupID = messageData["GroupID"];
            var uid = messageData["UID"];
            Console.WriteLine($"Need Roll Incomign UID: {messageData["UID"]} .");
            var roll = int.Parse(messageData["Roll"]);
            string Username = GetUsernamebyUID(uid);
            Console.WriteLine($"Need roll: User {Username} rolled {roll} in group {groupID}.");

            Dispatcher.Invoke(() =>
            {
                ProcessNeedGreedRoll(1,groupID,uid,roll); 
                mainWindow.SendMessageToConsole($"[Need Roll]: User {uid} rolled {roll}.", 15);
            });
        }
        private void HandleGreedRollMessage(Dictionary<string, string> messageData)
        {
            var groupID = messageData["GroupID"];
            var uid = messageData["UID"];
            var roll = int.Parse(messageData["Roll"]);
            Console.WriteLine($"Greed roll: User {uid} rolled {roll} in group {groupID}.");

            Dispatcher.Invoke(() =>
            {
                ProcessNeedGreedRoll(2, groupID, uid, roll);  
                mainWindow.SendMessageToConsole($"[Greed Roll]: User {uid} rolled {roll}.", 15);
            });
        }
        private void HandleFlipMessage(Dictionary<string, string> messageData)
        {
            var groupID = messageData["GroupID"];
            var message = messageData["Message"];
            var uid= messageData["UID"];
            Console.WriteLine($"Flip in group {groupID}: {message}");

            Dispatcher.Invoke(() =>
            {
                mainWindow.SendMessageToConsole($"[Flip]: {message}", 16);
            });
        }
        private void HandleRpsMessage(Dictionary<string, string> messageData)
        {
            var groupID = messageData["GroupID"];
            var message = messageData["Message"];
            //var user = messageData["Username"];
            //Console.WriteLine($"RPS : {user} threw {message}");

            Dispatcher.Invoke(() =>
            {
                mainWindow.SendMessageToConsole($"[RPS]:{message}", 17);
            });
        }
        private void HandleEndRollVoteMessage(Dictionary<string, string> messageData)
        {
            var groupID = messageData["GroupID"];
            var uid = messageData["UID"];
            Console.WriteLine($"Vote to end roll in group {groupID} by user {uid}.");

            Dispatcher.Invoke(() =>
            {
                VoteToEndRollRecieved(uid, groupID);
                mainWindow.SendMessageToConsole($"[End Roll Vote]: User {uid} voted to end the roll.", 12);
            });
        }
        private void HandleEndNeedGreedVoteMessage(Dictionary<string, string> messageData)
        {
            var groupID = messageData["GroupID"];
            var uid = messageData["UID"];
            Console.WriteLine($"Vote to end Need/Greed roll in group {groupID} by user {uid}.");

            Dispatcher.Invoke(() =>
            {
                VoteToEndNeedGreedRollRecieved(uid,groupID);
                mainWindow.SendMessageToConsole($"[End Need/Greed Roll Vote]: User {uid} voted to end the roll.", 18);
            });
        }
        private void HandlePrivateMessage(Dictionary<string, string> messageData)
        {
            var senderUID = messageData["SenderUID"];
            var receiverUID = messageData["ReceiverUID"];
            var message = messageData["Message"];
            string sendingUsername = GetUsernamebyUID(senderUID);
            Console.WriteLine($"Private message from {sendingUsername} to {receiverUID}: {message}");

            Dispatcher.Invoke(() =>
            {
                mainWindow.SendMessageToConsole($"[Whisper]:{sendingUsername} says: {message}", 25);
            });
        }
        private void HandleSystemMessage(Dictionary<string, string> messageData)
        {
            var message = messageData["Message"];
            Console.WriteLine($"System message: {message}");

            Dispatcher.Invoke(() =>
            {
                mainWindow.SendMessageToConsole($"[System]: {message}", 20);
            });
        }
        private async Task HandleKickMessage(Dictionary<string, string> messageData)
        {
            var message = messageData["Message"];
            Console.WriteLine($"Kicked: {message}");

            Dispatcher.Invoke(() =>
            {
                MessageBox.Show("You were kicked from the server.");
            });
                 await Disconnect();
        }
        private void HandleGroupMigration(Dictionary<string, string> messageData)
        {
            var newGroupID = messageData["NewGroupID"];
            Console.WriteLine($"Group migrated to {newGroupID}");

            Dispatcher.Invoke(() =>
            {
                groupId = newGroupID;
                mainWindow.SendMessageToConsole($"[Group Migration]: Migrated to new group {newGroupID}.", 1);
                mainWindow.UpdateConnectionInfo(uId, userName, groupId);
            });
        }
      
        public static async Task ProcessRoll(string uid, int _roll)
        {

            var roll = _roll;
            Console.WriteLine($"Start user by UID search{uid}");
            User user = GetUserbyUID(uid);
            //string username = user.Username;
            Console.WriteLine($"Starting to process ROll");
            if (!rollStarted)
            {
                Console.WriteLine($"New Roll Started");
                rollStarted = true;

                if (MainWindow.playSoundEnabled && groupMembers.Count>1)
                {
                    SoundPlayer player = new SoundPlayer(Properties.Resources.roll_default);

                    player.Load();
                    player.Play();
                }
                
            }
            if (!hasRolled)
            {
                Console.WriteLine($"User has not rolled updating user roll data");

                rollWindow.UpdateUserRollData(" ", 2);
            }

            Console.WriteLine($"if winning roll 0 update roll and winning user.");
            if (winningRoll !=0 && winningRoll == roll)
            {
                tiedRoll = roll;
                winningRollUser = new User();
                tiedRollUsers.Add(user);
                Console.WriteLine($"Updating winning Roll data #1");
                rollWindow.UpdateWinningRollData(winningRoll.ToString(), $"{winningRollUser.Username} is winning.", 1);

            }

            Console.WriteLine($"Check if roll is greater than current winner.");
            if (winningRoll < roll&& winningRoll != roll)
            {
                winningRoll = roll;
                winningRollUser = user;
                tiedRollUsers.Add(user);
           
                rollWindow.UpdateWinningRollData(winningRoll.ToString(), $"{winningRollUser.Username} is winning.", 1);
                Console.WriteLine($"Updating winning Roll data #2");
            }


            Console.WriteLine($"Checks if the roll was for This user. ");
            if (uid == uId)
            {
                //clients roll update roll window.
                userRoll = roll;

                if(roll==0)
                {
                    rollpass = true;
                }
                if (groupMembers.Count >= 2)
                {
                    if (userRoll == 100)
                    {
                        totalPerfectRollCount++;
                       
                    }
                    else if (userRoll == 1)
                    {
                        totalOneRolls++;
                        if (MainWindow.playSoundEnabled)
                        {
                            SoundBoard("1");
                        }

                    }
                    totalUserRollCount++;
                    userTotalRollSum += userRoll;
                }

                Console.WriteLine($"Start Updating User roll data to show we rolled.");
                rollWindow.UpdateUserRollData(userRoll.ToString(), 1);

            }

            currentRoll.Add(user, roll);

            //Processing roll soundboand and winner . 

            //if someone rolls 100.
            if(roll==100)
            {

                if(uid!=uId)
                {
                    // they win
                    if (MainWindow.playSoundEnabled)
                    {
                        SoundBoard("they100");
                    }
                    mainWindow.SendMessageToConsole($"[Roll]: {user.Username} is a Wizzard! They rolled a {roll.ToString()}.",1);
                }
                else
                {
                    if (MainWindow.playSoundEnabled)
                    {
                        SoundBoard("you100");
                    }
                    mainWindow.SendMessageToConsole($"[Roll]: You're a Wizzard! You rolled a {roll.ToString()}.", 1);
                }


            }
            else
            {

                mainWindow.SendMessageToConsole($"[Roll]: {user.Username} rolls a {roll.ToString()}.",6);
            }
          
            if (currentRoll.Count >= groupMembers.Count)
            {
                if(tiedRoll!=0)
                {
                    rollWindow.UpdateUserRollData(userRoll.ToString(), 2);

                    rollWindow.UpdateWinningRollData(winningRoll.ToString(), $"You have tied!!!", 2);

                }
               else if (winningRollUser.UID == uId && tiedRoll == 0)
                {
                    if (groupMembers.Count >= 2)
                    {
                        totalWinningRolls++;
                    }
                    rollWindow.UpdateUserRollData(userRoll.ToString(), 2);

                    rollWindow.UpdateWinningRollData(winningRoll.ToString(), $"You win!!!", 2);
                    
                    if(userName=="STICKY")
                    {
                        if (MainWindow.playSoundEnabled && userRoll != 1 && winningRoll != 100 && winningRoll != 69)
                        {
                            SoundPlayer player = new SoundPlayer(Properties.Resources.wow_c_101soundboards);
                          
                            player.Load();
                            player.Play();
                        }
                    }
                    else
                    {
                        if (MainWindow.playSoundEnabled && userRoll != 1 && winningRoll != 100 && winningRoll != 69)
                        {
                            SoundBoard("win");
                        }
                    }
                }
                else 
                {
                    rollWindow.UpdateUserRollData(userRoll.ToString(), 3);
                    rollWindow.UpdateWinningRollData(winningRoll.ToString(), $"{winningRollUser.Username} wins!!!", 1);
                    if (MainWindow.playSoundEnabled && userRoll != 1 && winningRoll != 100 && winningRoll != 69)
                    {
                        SoundBoard("lose");
                    }

                }

                 mainWindow.SendMessageToConsole($"[Roll]: {winningRollUser.Username} wins with a roll of {winningRoll.ToString()}.",7);
                if(userRoll==(winningRoll-5) || userRoll == (winningRoll - 4) || userRoll == (winningRoll - 3) || userRoll == (winningRoll - 2) || userRoll == (winningRoll - 1) && groupMembers.Count>=2)
                {
                    if (MainWindow.playSoundEnabled)
                    {
                        //play emotional damage
                        SoundPlayer player = new SoundPlayer(Properties.Resources.emotional_damage);
                        
                        player.Load();
                        player.Play();

                        //SoundBoard("emo");
                    }

                }
                winningRollUser = new User();
                currentRoll.Clear();
                tiedRollUsers.Clear();
                winningRoll = 0;
                tiedRoll = 0;
                hasRolled = false;
                rollStarted = false;
                Application.Current.Dispatcher.Invoke(() => rollWindow.btn_VoteDone_White.Visibility = Visibility.Visible);
                Application.Current.Dispatcher.Invoke(() => rollWindow.btn_VoteDone_Red.Visibility = Visibility.Hidden);
                if (!rollpass)
                {
                    SaveUserStats();
                }
            }
        }
        public void ResetRollonDisconnect(string username)
        {
            if (rollStarted)
            {
                CancelRoll();
                mainWindow.SendMessageToConsole($"[Roll] Roll canceled {username} Disconnected.", 1);
                rollWindow.UpdateWinningRollData(winningRoll.ToString(), $"Roll Canceled!!!", 1);
            }

        }
        public void ResetRollonUserJoin(string username)
        {
            if (rollStarted)
            {
                 CancelRoll();
                mainWindow.SendMessageToConsole($"[Roll] Roll canceled {username} Joined.",1);
                rollWindow.UpdateWinningRollData(winningRoll.ToString(), $"Roll Canceled!!!", 1);
            }

            }
        public void ResetNeedGreedRollonUserJoin(string username)
        {
            if (needGreedRollStarted)
            {
                CancelNeedGreedRoll();
                mainWindow.SendMessageToConsole($"[N>G]: Roll canceled {username} Joined.",1);
                rollWindow.UpdateWinningRollData("", $"Roll Canceled!!!", 1);
            }

        }
        public void ResetNeedGreedRollonUserDisconnect(string username)
        {
            if (needGreedRollStarted)
            {
                CancelNeedGreedRoll();
                mainWindow.SendMessageToConsole($"[N>G]: Roll canceled {username} Disconnected.",1);
                rollWindow.UpdateWinningRollData("", $"Roll Canceled!!!", 1);
            }

        }
        public void CancelRoll()
        {
            if (rollStarted)
            {
                winningRollUser = new User();
                currentRoll.Clear();
                winningRoll = 0;
                tiedRoll = 0;
                tiedRollUsers.Clear();
                hasRolled = false;
                rollStarted = false;
               Application.Current.Dispatcher.Invoke(() => rollWindow.btn_VoteDone_White.Visibility = Visibility.Visible);
                Application.Current.Dispatcher.Invoke(() => rollWindow.btn_VoteDone_Red.Visibility = Visibility.Hidden);
            
                //vote to cancel things needing reset
                rollToEndVoteDone = true;
                endRollVotes.Clear();
                votesForEndingRoll = 0;
                userHasVotedToEndRoll = false;
            }
        }
        public static async Task SendRollRequestToServer()
        {
            if (_clientSocket != null )
            {
                var message = new
                {
                    Type = "roll",
                    GroupID = groupId,
                    UID = uId
                };
                await _clientSocket.SendMessageAsync(message);
                Console.WriteLine("Roll Request sent to server.");
            }
        }

        public static async Task SendRollPassRequestToServer()
        {
            if (_clientSocket != null)
            {
                var message = new
                {
                    Type = "rollpass",
                    GroupID = groupId,
                    UID = uId
                };
                await _clientSocket.SendMessageAsync(message);
                Console.WriteLine("RollPass Request sent to server.");
            }
        }
        public static User GetUserbyUID(string uid)
        {
            User user = new User();
             Console.WriteLine($"Lookign for user with UID {uid}.");
            for (int i = 0; i < groupMembers.Count; i++)
            {
                if (groupMembers[i].UID == uid)
                {
                    user = groupMembers[i];
                    Console.WriteLine($"User Found");
                    return user;
                }
            }

            return user;
        }
        public static string GetUsernamebyUID(string uid)
        {
            User user = new User();
            Console.WriteLine($"Lookign for user with UID {uid}.");
            for (int i = 0; i < groupMembers.Count; i++)
            {
                if (groupMembers[i].UID == uid)
                {
                    user = groupMembers[i];
                    string name = groupMembers[i].Username;
                    Console.WriteLine($"User Found");
                    return name;
                }
            }

            return null;
        }
        private void UserDisconnected(string _uid)
        {

            //Get Dc'd users UID and remove the user
            var uid = _uid;
            var user = groupMembers.Where(x => x.UID.ToString() == uid).FirstOrDefault();

            this.Dispatcher.Invoke(() => groupMembers.Remove(user));
            Console.WriteLine($"The Group now has {groupMembers.Count} members.");


            if (MainWindow.playSoundEnabled)
            {
                SoundPlayer player = new SoundPlayer(Properties.Resources.door_shuts);
                //SoundPlayer player = new SoundPlayer(@"Mapleroll\Connect\door_shuts.wav");
               
                player.Load();
                player.Play();
            }

            mainWindow.BuildMembersListView();
            ResetRollonDisconnect(user.Username);
            ResetNeedGreedRollonUserDisconnect(user.Username);
            mainWindow.SendMessageToConsole($"{user.Username} has left the group.",1);
        }

   

        public void UserConnected(string _username, string _uid)
        {
            //Console.WriteLine("User Connected Invoke Recieved");
            var uid = _uid;
            Console.WriteLine($"User Connected UID: {uid}");

            var username = _username;
            Console.WriteLine($"User Connected Username: {username}");
            if (!UserExistsInGroup(uid))
            {
                var user = new User();
                // Console.WriteLine("New User Created");
                user.UID = uid;
                user.Username = username;



                groupMembers.Add(user);
                Console.WriteLine($"User added to group members: {username} total members :{groupMembers.Count}");

                this.Dispatcher.Invoke(() =>
                {
                    {

                        ListViewItem item = new ListViewItem();
                        //ContextMenu menu = (ContextMenu)this.FindResource("ItemContextMenu");

                        item.FontWeight = FontWeights.Bold;

                        item.Visibility = Visibility.Visible;
                        item.Width = 90;

                        item.Name = user.Username;
                        item.Content = user.Username;
                       
                        item.VerticalContentAlignment = VerticalAlignment.Center;
                        item.HorizontalContentAlignment = HorizontalAlignment.Center;
                        item.Background = Brushes.Transparent;
                        item.BorderBrush = Brushes.White;
                        item.BorderThickness = new Thickness(.5, .5, .5, .5);
                        item.Foreground = new SolidColorBrush(Colors.White);
                        item.FontSize = 9;
                        item.IsTabStop = false;
                        item.Uid = user.UID;
                        item.Focusable = true;
                        item.ContextMenu = mainWindow.lst_GroupMembers.Resources["MembersContextMenu"] as ContextMenu;
                        //item.MouseMove += mainWindow.Window_MouseMove;
                        item.MouseRightButtonUp += mainWindow.ContextMenuOpening;
                        Console.WriteLine($"Textbox parrent  {item.Parent}");

                        mainWindow.lst_GroupMembers.Items.Add(item);
                    }
                });


                if (user.Username != userName)
                {
                    mainWindow.SendMessageToConsole($"{user.Username} joined the group.", 1);
                    if (MainWindow.playSoundEnabled)
                    {
                        SoundPlayer player = new SoundPlayer(Properties.Resources.knocking_on_door);

                        player.Load();
                        player.Play();
                    }
                }
 


                    ResetRollonUserJoin(user.Username);
                ResetNeedGreedRollonUserJoin(user.Username);

            }
        }
        public static async Task SendVoteToEndRollRequestToServer()
        {
            if (_clientSocket != null)
            {
                var message = new
                {
                    Type = "endroll",
                    GroupId = groupId,
                    UID = uId
                };
                await _clientSocket.SendMessageAsync(message);
                Console.WriteLine("Vote to end Roll Request sent to server.");
            }
        }
        public void VoteToEndRollRecieved(string _uid, string groupID)
        {
                //Console.WriteLine("User Connected Invoke Recieved");
                var uid = _uid;
                Console.WriteLine($"User Voted To end Vote UID: {uid}");
                var _groupID = groupID;
                Console.WriteLine($"User Voted GroupID: {_groupID}");

            if (rollStarted)
            {
                Console.WriteLine($"Checking if user has voted to cancel.");
                if (!UserVotedToEndRollAlready(uid))
                {
                    endRollVotes.Add(uid);
                    votesForEndingRoll++;
                    Console.WriteLine($"Adding Vote to cancel.");
                }

                Console.WriteLine($"Calculating percent to cancel.");
                Console.WriteLine($"Current group size {groupMembers.Count}.");
                double percent = .6;
                Console.WriteLine($"Percent needed to cancel vote {percent}.");
                double grouppercent = groupMembers.Count * percent;
                Console.WriteLine($"Votes needed based on Group size: {grouppercent}.");
                if (groupMembers.Count <= 2)
                {
                    int voteSucceedPoint = (int)Math.Ceiling(grouppercent);
                    Console.WriteLine($"Votes needed rounded up: {voteSucceedPoint}.");
                    Console.WriteLine($"Current Votes To cancel are {votesForEndingRoll}  votes needed are {voteSucceedPoint}.");
                    if (votesForEndingRoll >= voteSucceedPoint)
                    {
                        Console.WriteLine($"Vote to cancel passed.");
                        CancelRoll();
                        rollWindow.UpdateWinningRollData("--", $"Roll Canceled!", 1);
                        rollWindow.UpdateUserRollData("--", 1);
                        mainWindow.SendMessageToConsole($"[Roll] The vote to cancel the Roll has PASSED!", 1);
                        rollToEndVoteDone = true;
                        endRollVotes.Clear();
                        votesForEndingRoll = 0;
                        userHasVotedToEndRoll = false;
                    }
                }
                else
                {
                    int voteSucceedPoint = (int)Math.Floor(grouppercent);
                    Console.WriteLine($"Votes needed rounded up: {voteSucceedPoint}.");
                    Console.WriteLine($"Current Votes To cancel are {votesForEndingRoll}  votes needed are {voteSucceedPoint}.");
                    if (votesForEndingRoll >= voteSucceedPoint)
                    {
                        Console.WriteLine($"Vote to cancel passed.");
                        CancelRoll();
                        rollWindow.UpdateWinningRollData("--", $"Roll Canceled!", 1);
                        rollWindow.UpdateUserRollData("--", 1);
                        mainWindow.SendMessageToConsole($"[Roll] The vote to cancel the Roll has PASSED!", 1);
                        rollToEndVoteDone = true;
                        endRollVotes.Clear();
                        votesForEndingRoll = 0;
                        userHasVotedToEndRoll = false;
                    }

                }

            }
            
         }
        public bool UserVotedToEndRollAlready(string uid)
        {

            foreach (var item in endRollVotes)
            {
                if(item==uid)
                {

                    Console.WriteLine($"User Voted to cancel already.");
                    return true;
                }


            }

            Console.WriteLine($"User has not voted to cancel already.");
            return false;
        }
        public static async Task SendVoteToEndNeedGreedRollRequestToServer()
        {
            if (_clientSocket != null)
            {
                var message = new
                {
                    type = "endneed",
                    groupId = groupId,
                    userId = uId
                };
                await _clientSocket.SendMessageAsync(message);
                Console.WriteLine("Vote to end Need Greed Roll Request sent to server.");
            }
        }
        public void VoteToEndNeedGreedRollRecieved(string _uid, string groupID)
        {
            //Console.WriteLine("User Connected Invoke Recieved");
            var uid = _uid;
            Console.WriteLine($"User Voted To end N>G Vote UID: {uid}");
            var _groupID = groupID;
            Console.WriteLine($"User Voted N>G GroupID: {_groupID}");
            if (needGreedRollStarted)
            {
                Console.WriteLine($"Checking if user has voted to cancel.");
                if (!UserVotedToEndNeedGreedRollAlready(uid))
                {
                    endNeedGreedRollVotes.Add(uid);
                    votesForEndingNeedGreedRoll++;
                    Console.WriteLine($"Adding Vote to cancel.");
                }

                double percent = .6;
                decimal grouppercent = MapleRollConnect.groupMembers.Count * (int)percent;
                int voteSucceedPoint = (int)Math.Floor(grouppercent);
                Console.WriteLine($"Current Votes To cancel are {votesForEndingNeedGreedRoll}  votes needed are {voteSucceedPoint}.");
                if (votesForEndingNeedGreedRoll >= voteSucceedPoint)
                {
                    Console.WriteLine($"Vote to cancel passed.");
                    CancelRoll();
                    needGreedWindow.UpdateWinningRollData("--", $"Roll Canceled!.", 1, "Need");
                    needGreedWindow.UpdateWinningRollData("--", $"Roll Canceled!.", 1, "Greed");
                    needGreedWindow.UpdateUserRollData("--", 1);
                    mainWindow.SendMessageToConsole($"[N>G]: The vote to cancel the Roll has PASSED!", 1);

                    rollToEndNeedGreedVoteDone = true;
                    endNeedGreedRollVotes.Clear();
                    votesForEndingNeedGreedRoll = 0;
                    userHasVotedToEndNeedGreedRoll = false;
                }

            }
        }
        public bool UserVotedToEndNeedGreedRollAlready(string uid)
        {

            foreach (var item in endNeedGreedRollVotes)
            {
                if (item == uid)
                {

                    Console.WriteLine($"User Voted to cancel already.");
                    return true;
                }


            }

            Console.WriteLine($"User has not voted to cancel already.");
            return false;
        }
        public bool UserExistsInGroup(string uid)
        {
            for (int i = 0; i < groupMembers.Count; i++)
            {
                if (groupMembers[i].UID==uid)
                {

                    return true;
                }
            }
            return false;
        }
        public void ConnectionSuccessful()
        {

            LoadUserStats();
            Console.WriteLine("Connection Successful");

                MainWindow.connectWindow = this;
                rollWindow = MainWindow.rollWindow;
                needGreedWindow=MainWindow.needGreedWindow;
                string temp = userName.ToUpper();
                this.Dispatcher.Invoke((() => {mainWindow.Show();}));;
                mainWindow.SendMessageToConsole($"May RNGsus be with you {temp}!",1);
                mainWindow.UpdateConnectionInfo( uId, temp, groupId);
               
                this.Dispatcher.Invoke((() =>{this.Hide();}));
            


        }
        public void MigrateUser()
        {

            mainWindow.UpdateConnectionInfo(uId, userName.ToUpper(), groupId);


        }
        public void ConnectionError()
        {

            connectionError.Show();

        }
        public static void MessageReceived(string msg, int color)
        {
            mainWindow.SendMessageToConsole(msg, color);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        public static void SoundBoard(string sound)
        {
            switch (sound)
            {

                case "win":
                    switch (userSoundProfile)
                    {
                        //Defaut 
                        case 1:
                            SoundPlayer player1 = new SoundPlayer(Properties.Resources.oh_yeah_Default);
                            player1.Load();
                            player1.Play();
                            break;

                            //mario
                        case 2:
                            SoundPlayer player2 = new SoundPlayer(Properties.Resources.coin_mario);
                            player2.Load();
                            player2.Play();
                            break;

                        // Bender
                        case 3:
                            SoundPlayer player3 = new SoundPlayer(Properties.Resources.yeah_bender);
                            player3.Load();
                            player3.Play();
                            break;

                        // Family Guy
                        case 4:
                            SoundPlayer player4 = new SoundPlayer(Properties.Resources.ha_familyguy);
                            player4.Load();
                            player4.Play();
                            break;

                        // Zelda
                        case 5:
                            SoundPlayer player5 = new SoundPlayer(Properties.Resources.item_Zelda);
                            player5.Load();
                            player5.Play();
                            break;

                        // Southpark
                        case 6:
                            SoundPlayer player6 = new SoundPlayer(Properties.Resources.holy_moly_southpark);
                            player6.Load();
                            player6.Play();
                            break;



                        default:
                            SoundPlayer _player = new SoundPlayer(Properties.Resources.mario_yahoo);
                            _player.Load();
                            _player.Play();

                            break;
                    }
                    break;
               
                case "lose":
                    switch (userSoundProfile)
                    {                         //Defaut 
                        case 1:
                            SoundPlayer player1 = new SoundPlayer(Properties.Resources.oh_man_deafult);
                            player1.Load();
                            player1.Play();
                            break;

                        //mario
                        case 2:
                            SoundPlayer player2 = new SoundPlayer(Properties.Resources.bro_mario);
                            player2.Load();
                            player2.Play();
                            break;

                        // Bender
                        case 3:
                            SoundPlayer player3 = new SoundPlayer(Properties.Resources.pass_Bender);
                            player3.Load();
                            player3.Play();
                            break;

                        // Family Guy
                        case 4:
                            SoundPlayer player4 = new SoundPlayer(Properties.Resources.ha_familyguy);
                            player4.Load();
                            player4.Play();
                            break;

                        // Zelda
                        case 5:
                            SoundPlayer player5 = new SoundPlayer(Properties.Resources.low_hp_Zelda);
                            player5.Load();
                            player5.Play();
                            break;

                        // Southpark
                        case 6:
                            SoundPlayer player6 = new SoundPlayer(Properties.Resources.and_its_gone_Southpark);
                            player6.Load();
                            player6.Play();
                            break;



                        default:
                            SoundPlayer _player = new SoundPlayer(Properties.Resources.mario_yahoo);
                            _player.Load();
                            _player.Play();

                            break;
                    }


                    break;

                case "you100":
                    switch (userSoundProfile)
                    {
                        //Defaut 
                        case 1:
                            SoundPlayer player1 = new SoundPlayer(Properties.Resources.yer_a_wizard_harry);
                            player1.Load();
                            player1.Play();
                            break;

                        //mario
                        case 2:
                            SoundPlayer player2 = new SoundPlayer(Properties.Resources.star_theme_mario);
                            player2.Load();
                            player2.Play();
                            break;

                        // Bender
                        case 3:
                            SoundPlayer player3 = new SoundPlayer(Properties.Resources.oh_my_god_Bender);
                            player3.Load();
                            player3.Play();
                            break;

                        // Family Guy
                        case 4:
                            SoundPlayer player4 = new SoundPlayer(Properties.Resources.jackpot_familyguy);
                            player4.Load();
                            player4.Play();
                            break;

                        // Zelda
                        case 5:
                            SoundPlayer player5 = new SoundPlayer(Properties.Resources.chest1_zelda);
                            player5.Load();
                            player5.Play();
                            break;

                        // Southpark
                        case 6:
                            SoundPlayer player6 = new SoundPlayer(Properties.Resources.yeah_yes_south);
                            player6.Load();
                            player6.Play();
                            break;



                        default:
                            SoundPlayer _player = new SoundPlayer(Properties.Resources.yer_a_wizard_harry);
                            _player.Load();
                            _player.Play();

                            break;
                    }


                    break;

                case "1":
                    switch (userSoundProfile)
                    {                        //Defaut 
                        case 1:
                            SoundPlayer player1 = new SoundPlayer(Properties.Resources.dissapointment);
                            player1.Load();
                            player1.Play();
                            break;

                        //mario
                        case 2:
                            SoundPlayer player2 = new SoundPlayer(Properties.Resources.oh_uh_mario);
                            player2.Load();
                            player2.Play();
                            break;

                        // Bender
                        case 3:
                            SoundPlayer player3 = new SoundPlayer(Properties.Resources.ah_crap_Bender);
                            player3.Load();
                            player3.Play();
                            break;

                        // Family Guy
                        case 4:
                            SoundPlayer player4 = new SoundPlayer(Properties.Resources.shut_up_meg_familyguy);
                            player4.Load();
                            player4.Play();
                            break;

                        // Zelda
                        case 5:
                            SoundPlayer player5 = new SoundPlayer(Properties.Resources.link_fall_zelda);
                            player5.Load();
                            player5.Play();
                            break;

                        // Southpark
                        case 6:
                            SoundPlayer player6 = new SoundPlayer(Properties.Resources.geez_thats_terrible_south_park);
                            player6.Load();
                            player6.Play();
                            break;



                        default:
                            SoundPlayer _player = new SoundPlayer(Properties.Resources.dissapointment);
                            _player.Load();
                            _player.Play();

                            break;
                    }


                    break;

                case "69":
                    switch (userSoundProfile)
                    {
                        //Defaut 
                        case 1:
                            SoundPlayer player1 = new SoundPlayer(Properties.Resources.niiice);
                            player1.Load();
                            player1.Play();
                            break;

                        //mario
                        case 2:
                            SoundPlayer player2 = new SoundPlayer(Properties.Resources.mama_mia_mario);
                            player2.Load();
                            player2.Play();
                            break;

                        // Bender
                        case 3:
                            SoundPlayer player3 = new SoundPlayer(Properties.Resources.whoa_mama_Bender);
                            player3.Load();
                            player3.Play();
                            break;

                        // Family Guy
                        case 4:
                            SoundPlayer player4 = new SoundPlayer(Properties.Resources.bow_chicka_wow_familyguy);
                            player4.Load();
                            player4.Play();
                            break;

                        // Zelda
                        case 5:
                            SoundPlayer player5 = new SoundPlayer(Properties.Resources.bounce_Zelda);
                            player5.Load();
                            player5.Play();
                            break;

                        // Southpark
                        case 6:
                            SoundPlayer player6 = new SoundPlayer(Properties.Resources.nice_Southpark);
                            player6.Load();
                            player6.Play();
                            break;


                        default:
                            SoundPlayer _player = new SoundPlayer(Properties.Resources.niiice);
                            _player.Load();
                            _player.Play();

                            break;
                    }

                    break;

                case "they100":
                    switch (userSoundProfile)
                    {
                        //Defaut 
                        case 1:
                            SoundPlayer player1 = new SoundPlayer(Properties.Resources.harry_potter_theme_hq);
                            player1.Load();
                            player1.Play();
                            break;

                        //mario
                        case 2:
                            SoundPlayer player2 = new SoundPlayer(Properties.Resources.star_theme_mario);
                            player2.Load();
                            player2.Play();
                            break;

                        // Bender
                        case 3:
                            SoundPlayer player3 = new SoundPlayer(Properties.Resources.futurama_theme_bender);
                            player3.Load();
                            player3.Play();
                            break;

                        // Family Guy
                        case 4:
                            SoundPlayer player4 = new SoundPlayer(Properties.Resources.congratulations_Familyguy);
                            player4.Load();
                            player4.Play();
                            break;

                        // Zelda
                        case 5:
                            SoundPlayer player5 = new SoundPlayer(Properties.Resources.prayer_zelda);
                            player5.Load();
                            player5.Play();
                            break;

                        // Southpark
                        case 6:
                            SoundPlayer player6 = new SoundPlayer(Properties.Resources.intro_Southpark);
                            player6.Load();
                            player6.Play();
                            break;

                        default:
                            SoundPlayer _player = new SoundPlayer(Properties.Resources.harry_potter_theme_hq);
                            _player.Load();
                            _player.Play();

                            break;
                    }

                    break;

                case "Emo":
                    switch (userSoundProfile)
                    {
                        //Defaut 
                        case 1:
                            SoundPlayer player1 = new SoundPlayer(Properties.Resources.emotional_damage);
                            player1.Load();
                            player1.Play();
                            break;

                        //mario
                        case 2:
                            SoundPlayer player2 = new SoundPlayer(Properties.Resources.wahhhhh_mario);
                            player2.Load();
                            player2.Play();
                            break;

                        // Bender
                        case 3:
                            SoundPlayer player3 = new SoundPlayer(Properties.Resources.you_bastard_bender);
                            player3.Load();
                            player3.Play();
                            break;

                        // Family Guy
                        case 4:
                            SoundPlayer player4 = new SoundPlayer(Properties.Resources.almost_familyguy);
                            player4.Load();
                            player4.Play();
                            break;

                        // Zelda
                        case 5:
                            SoundPlayer player5 = new SoundPlayer(Properties.Resources.link_death_zelda);
                            player5.Load();
                            player5.Play();
                            break;

                        // Southpark
                        case 6:
                            SoundPlayer player6 = new SoundPlayer(Properties.Resources.omg_no_way_Southpark);
                            player6.Load();
                            player6.Play();
                            break;

                        default:
                            SoundPlayer _player = new SoundPlayer(Properties.Resources.harry_potter_theme_hq);
                            _player.Load();
                            _player.Play();

                            break;
                    }

                    break;

                case "join":
                   
                            SoundPlayer player7 = new SoundPlayer(Properties.Resources.knocking_on_door);
                            player7.Load();
                            player7.Play();
                          

                    break;

                case "leave":
                    
                   
                      SoundPlayer player8 = new SoundPlayer(Properties.Resources.door_shuts);
                      player8.Load();
                      player8.Play();
                       
                    break;

                case "roll":


                    SoundPlayer player9 = new SoundPlayer(Properties.Resources.roll_default);
                    player9.Load();
                    player9.Play();

                    break;
            }
        }
        public static void CancelNeedGreedRoll()
        {
            if(needGreedRollStarted)
            {

                winningNeedGreedUser = new User();
                winningGreedUser = new User();
                winningNeedUser = new User();
                winningNeedRoll = 0;
                winningGreedRoll = 0;
                winningNeedGreedRoll = 0;
                tiedGreedRoll = 0;
                tiedNeedRoll = 0;
                userNeedGreedRoll = 0;


                needRolls.Clear();
                greedRolls.Clear();

                isNeedRoll = false;
                isGreedRoll = false;
                someoneRolledNeed = false;
                hasNeedGreedRolled = false;
                needGreedRollStarted = false;

                Application.Current.Dispatcher.Invoke(() => needGreedWindow.btn_VoteDone_White.Visibility = Visibility.Visible);
                Application.Current.Dispatcher.Invoke(() => needGreedWindow.btn_VoteDone_Red.Visibility = Visibility.Hidden);

            }

            
        }
        public static void ProcessNeedGreedRoll(int __rollType, string _groupID, string _uid, int __roll)
        {
            
            var groupId=_groupID;
            var uid = _uid;
            int roll = __roll;
            int rollType = __rollType;

            User user = GetUserbyUID(uid);
            string username = user.Username;
            if (!needGreedRollStarted)
            {
                needGreedWindow.UpdateUserRollData("".ToString(), 1);
                needGreedRollStarted = true;
            }
            if (rollType==1)
            {
                isNeedRoll=true;
                isGreedRoll = false;
                if(!someoneRolledNeed)
                {
                    winningNeedGreedRoll = 0;
                    winningNeedGreedUser= new User();
                    winningGreedRoll = 0;
                    winningGreedUser = new User();
                }
                someoneRolledNeed = true;
                Application.Current.Dispatcher.Invoke((() => needGreedWindow.lbl_WinningNeedTitle.Visibility= Visibility.Visible ));
                Application.Current.Dispatcher.Invoke((() => needGreedWindow.lbl_WinningGreedTitle.Visibility = Visibility.Hidden));
                
            }
            else if (rollType==2)
            {

                isGreedRoll=true;
                isNeedRoll = false;
                if(!someoneRolledNeed)
                {
                Application.Current.Dispatcher.Invoke((() => needGreedWindow.lbl_WinningNeedTitle.Visibility = Visibility.Hidden));
                Application.Current.Dispatcher.Invoke((() => needGreedWindow.lbl_WinningGreedTitle.Visibility = Visibility.Visible));
                }
            }

            if (uid == uId)
            {
                userNeedGreedRoll = roll;
                if(roll==0)
                {

                    needgreedpass = true;
                }
                if (groupMembers.Count >= 2)
                {
                    if (roll == 100)
                    {
                        totalPerfectRollCount++; 
                    }
                    else if (roll == 1)
                    {
                        totalOneRolls++;
                        if (MainWindow.playSoundEnabled)
                        {
                            SoundBoard("1");
                        }
                    }

                    totalUserRollCount++;
                    userTotalRollSum += roll;
                }
                if (roll == 69)
                {

                    if (MainWindow.playSoundEnabled)
                    {
                        SoundBoard("69");
                    }
                }

                needGreedWindow.UpdateUserRollData(roll.ToString(), 1);
            }

            //need roll
            if (isNeedRoll)
            {
                needRolls.Add(user, roll);

                if (winningNeedGreedRoll != 0 && winningNeedGreedRoll == roll)
                {
                    tiedNeedRoll = roll;

                    winningNeedUser = new User();
                    winningNeedGreedUser = new User();
                    needGreedWindow.UpdateWinningRollData(tiedNeedRoll.ToString(), $"There is a tie.", 1, "Need");
                }

                if (roll>winningNeedGreedRoll && winningNeedGreedRoll !=roll)
                {
                    winningNeedGreedRoll = roll;
                    winningNeedGreedUser = user;

                   needGreedWindow.UpdateWinningRollData(winningNeedGreedRoll.ToString(), $"{winningNeedGreedUser.Username} is winning.", 1, "Need");
                }
            }

            //greed roll
            if (isGreedRoll )
            {
                greedRolls.Add(user, roll);

                if (winningNeedGreedRoll != 0 && winningNeedGreedRoll == roll && !someoneRolledNeed)
                {
                    tiedGreedRoll = roll;
                    winningNeedGreedUser = new User();
                   

                    needGreedWindow.UpdateWinningRollData(winningNeedGreedRoll.ToString(), $"{winningNeedGreedUser.Username} is Tied.", 1, "Greed");
                }
                    


                if (roll > winningNeedGreedRoll && winningNeedGreedRoll != roll && !someoneRolledNeed)
                {
                    winningNeedGreedRoll = roll;
                    
                    winningNeedGreedUser = user;

                    if (!someoneRolledNeed)
                    {
                        needGreedWindow.UpdateWinningRollData(winningNeedGreedRoll.ToString(), $"{winningNeedGreedUser.Username} is winning.", 1, "Greed");
                    }
                    
                }
            }

            if (roll == 100)
            {
                if (uid != uId)
                {
                    // they win
                    if (MainWindow.playSoundEnabled)
                    {
                        SoundBoard("they100");
                    }
                    mainWindow.SendMessageToConsole($"[N>G]: {user.Username} is a Wizzard! They rolled a {roll.ToString()}.",1);
                }
                else
                {
                    if (MainWindow.playSoundEnabled)
                    {
                        SoundBoard("you100");
                    }
                    mainWindow.SendMessageToConsole($"[N>G]: You're a Wizzard! You rolled a {roll.ToString()}.",1);
                }
            }
            else
            {
                if (isGreedRoll)
                {
                    mainWindow.SendMessageToConsole($"[N>G]: {user.Username} Greed rolls a {roll.ToString()}.",15);
                }
                else if(isNeedRoll)
                {
                    mainWindow.SendMessageToConsole($"[N>G]: {user.Username} Need rolls a {roll.ToString()}.",15);
                }
            }

            //all rolls in calculate winner
            if (needRolls.Count + greedRolls.Count >= groupMembers.Count)
            {
                //if tied
                if (tiedNeedRoll != 0 || tiedGreedRoll != 0)
                {
                    needGreedWindow.UpdateUserRollData(userNeedGreedRoll.ToString(), 2);
                    if (isGreedRoll && !someoneRolledNeed)
                    {
                        needGreedWindow.UpdateWinningRollData(winningNeedGreedRoll.ToString(), $"You have tied!!!", 2, "Greed");

                    }
                    else if (isNeedRoll)
                    {
                        needGreedWindow.UpdateWinningRollData(winningNeedGreedRoll.ToString(), $"There is a  tied!!!", 2, "Need");

                    }

                }
                //if we win and Tied is Zero (No Tie)
                else if (winningNeedGreedUser.UID == uId && tiedNeedRoll == 0 && tiedGreedRoll == 0)
                {
                    if (groupMembers.Count >= 2)
                    {
                        totalWinningRolls++;
                    }
                    //emo damage check
                    if (userNeedGreedRoll == (winningNeedGreedRoll - 5) || userNeedGreedRoll == (winningNeedGreedRoll - 4) || userNeedGreedRoll == (winningNeedGreedRoll - 3) || userNeedGreedRoll == (winningNeedGreedRoll - 2) || userNeedGreedRoll == (winningNeedGreedRoll - 1) && groupMembers.Count >= 2)
                    {
                        if (MainWindow.playSoundEnabled)
                        {
                            SoundBoard("emo");
                        }

                    }
                    needGreedWindow.UpdateUserRollData(userNeedGreedRoll.ToString(), 2);

                    if (!someoneRolledNeed)
                    {
                        needGreedWindow.UpdateWinningRollData(winningNeedGreedRoll.ToString(), $"You win!!!", 2, "Greed");
                        mainWindow.SendMessageToConsole($"[N>G]: {winningNeedGreedUser.Username} wins with a Greed roll of {winningNeedGreedRoll.ToString()}.", 7);
                    }
                    else if (someoneRolledNeed)
                    {

                        needGreedWindow.UpdateWinningRollData(winningNeedGreedRoll.ToString(), $"You win!!!", 2, "Need");
                        mainWindow.SendMessageToConsole($"[N>G]: {winningNeedGreedUser.Username} wins with a Need roll of {winningNeedGreedRoll.ToString()}.", 7);
                    }

                    //if the user is sticky play special sound 
                    if (userName == "STICKY")
                    {
                        if (MainWindow.playSoundEnabled && winningNeedGreedRoll != 1 && winningNeedGreedRoll != 100)
                        {
                            SoundPlayer player = new SoundPlayer(Properties.Resources.wow_c_101soundboards);

                            player.Load();
                            player.Play();
                        }
                    }
                    //else  play use win sound.
                    else
                    {
                        if (MainWindow.playSoundEnabled && winningNeedGreedRoll != 1 && winningNeedGreedRoll != 100)
                        {
                            SoundBoard("win");
                        }
                    }
                }
                //we lose
                else
                {


                    if (!someoneRolledNeed)
                    {
                        needGreedWindow.UpdateUserRollData(userNeedGreedRoll.ToString(), 3);
                        needGreedWindow.UpdateWinningRollData(winningNeedGreedRoll.ToString(), $"{winningNeedGreedUser.Username} wins!!!", 1, "Greed");
                        mainWindow.SendMessageToConsole($"[N>G]: {winningNeedGreedUser.Username} wins with a Greed roll of {winningNeedGreedRoll.ToString()}.",7);
                    }
                    else if (someoneRolledNeed)
                    {

                        needGreedWindow.UpdateUserRollData(userNeedGreedRoll.ToString(), 3);
                        needGreedWindow.UpdateWinningRollData(winningNeedGreedRoll.ToString(), $"{winningNeedGreedUser.Username} wins!!!", 1, "Need");
                        mainWindow.SendMessageToConsole($"[N>G]: {winningNeedGreedUser.Username} wins with a Need roll of {winningNeedGreedRoll.ToString()}.",7);


                    }

                    SoundBoard("lose");
                }

                ////emo damage check
                //if (userNeedGreedRoll == (winningNeedGreedRoll - 5) || userNeedGreedRoll == (winningNeedGreedRoll - 4) || userNeedGreedRoll == (winningNeedGreedRoll - 3) || userNeedGreedRoll == (winningNeedGreedRoll - 2) || userNeedGreedRoll == (winningNeedGreedRoll - 1) && groupMembers.Count >= 2)
                //{
                //    if (MainWindow.playSoundEnabled)
                //    {
                //        SoundBoard("emo");
                //    }

                //}

                //reset all
                winningNeedGreedUser = new User();
                winningGreedUser = new User();
                winningNeedUser = new User();
                winningNeedRoll = 0;
                winningGreedRoll = 0;
                winningNeedGreedRoll = 0;
                tiedGreedRoll = 0;
                tiedNeedRoll = 0;
                userNeedGreedRoll = 0;


                needRolls.Clear();
                greedRolls.Clear();

                isNeedRoll = false;
                isGreedRoll = false;
                someoneRolledNeed = false;
                hasNeedGreedRolled = false;
                needGreedRollStarted = false;
                if (!needgreedpass)
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() => SaveUserStats());
                    }
                    catch
                    {
                        Console.WriteLine("Error saving stats ");
                    }
                }
                needgreedpass = false;
            }
        }

        public static async Task SendVoteToKickToServer(string username)
        {
            if (_clientSocket != null)
            {
                var message = new
                {
                    Type = "kick",
                    GroupID = groupId,
                    Username= userName,
                    TargetUsername = username
                };
                await _clientSocket.SendMessageAsync(message);
                Console.WriteLine($"Vote to kick {username} sent to server.");
            }
        }
        public static async Task SendGreedRollToServer()
        {
            if (_clientSocket != null)
            {
                var message = new
                {
                    Type = "greed",
                    GroupID = groupId,
                    UID = uId
                };
                await _clientSocket.SendMessageAsync(message);
                Console.WriteLine("Greed Roll Request sent to server.");
            }
        }
        public static async Task SendMessageToServerGroup(string msg)
        {
            if (_clientSocket != null)
            {
                var message = new
                {
                    Type = "message",
                    GroupID = groupId,
                    UserName = userName,
                    UID = uId,
                    Message = msg
                };
                await _clientSocket.SendMessageAsync(message);
                Console.WriteLine($"[{DateTime.Now}]: Message Sent to server Group: {msg}.");
            }
        }
        public static async Task SendNeedRollToServer()
        {
            if (_clientSocket != null)
            {
                var message = new
                {
                    Type = "need",
                    GroupID = groupId,
                    UID = uId
                };
                await _clientSocket.SendMessageAsync(message);
                Console.WriteLine($"Need Roll Request sent to server with UID {uId}.");
            }
        }

        public static async Task SendNeedGreedPassRollToServer()
        {
            if (_clientSocket != null)
            {
                var message = new
                {
                    Type = "needgreedpass",
                    GroupID = groupId,
                    UID = uId
                };
                await _clientSocket.SendMessageAsync(message);
                Console.WriteLine($"Need/Greed Pass Roll Request sent to server with UID {uId}.");
            }
        }

        public static async Task SendPrivateMessageToServerGroup( string receiverUid, string msg)
        {
            if (_clientSocket != null)
            {
                string recievingUsername = GetUsernamebyUID(receiverUid);
                var message = new
                {
                    Type = "private",
                    GroupID = groupId,
                    SenderUID = uId,
                    ReceiverUID = receiverUid,
                    Message = msg
                };
                await _clientSocket.SendMessageAsync(message);
                Console.WriteLine($"[{DateTime.Now}]: Private Message Sent to {recievingUsername} in group {groupId}: {msg}.");
            }
        }
        public static async Task SendCoinFlipToServerGroup( string msg)
        {
            if (_clientSocket != null)
            {
                var message = new
                {
                    Type = "flip",
                    GroupID = groupId,
                    UID = uId,
                    Message = msg
                };
                await _clientSocket.SendMessageAsync(message);
                Console.WriteLine($"[{DateTime.Now}]: Coin Flip Sent to server Group {groupId}: {msg}.");
            }
        }
        public static async Task SendRPSToServerGroup( string msg)
        {
            if (_clientSocket != null)
            {
                var message = new
                {
                    Type = "rps",
                    GroupID = groupId,
                    Message = msg
                };
                await _clientSocket.SendMessageAsync(message);
                Console.WriteLine($"[{DateTime.Now}]: RPS Sent to server Group {groupId}: {msg}.");
            }
        }
        public static void SystemMessageRecieved(string msg)
        {
            
            mainWindow.SendMessageToConsole(msg, 20);
        }

        private void txb_UserName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            btn_ConnectNew_Click(sender, e);
        }

        private void txb_GroupId_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            btn_Connect_Click(sender, e);
        }
    }
}
