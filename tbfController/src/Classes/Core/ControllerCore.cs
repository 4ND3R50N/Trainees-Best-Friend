﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleNetworkServer;
using Support;
using WCDatabaseEngine;
using System.Net;
using System.Net.Sockets;


namespace tbfController.Classes.Core
{
    class ControllerCore
    {

        //Variablen
        networkServer TcpServer;
        DBEngine DatabaseEngine;
        logWriter Logger;
        List<networkServer.networkClientInterface> ActiveConnections;
        
        private string sAesKey;
        private char   cProtocolDelimiter;
        private char   cDataDelimiter;

        //Konstruktor
        public ControllerCore(short _iPort, char _cProtocolDelimiter, char _cDataDelimiter, string _sAesKey, string _sDatabaseDriver,
            string _sDBHostIp, short _sDBPort, string _sDBUser, string _sDBPass, string _sDBDefaultDB, string _sLogPath)
        {
            //Logging initialisations
            Logger = new logWriter(_sLogPath);
            Logger.writeInLog(true, "Logging class initialized!");
            //Database Initialisations
            if(_sDatabaseDriver == "mysql")
            {
                DatabaseEngine = new DBMysqlDataManager(_sDBHostIp,_sDBUser,_sDBPass,_sDBPort,_sDBDefaultDB);

            }else if(_sDatabaseDriver == "mssql")
            {
                DatabaseEngine = new DBMssqlDataManager(_sDBHostIp, _sDBUser, _sDBPass, _sDBPort, _sDBDefaultDB);
            }
            //Database test
            if (DatabaseEngine.testDBConnection())
            {
                Logger.writeInLog(true, "Database test successfull!");
            }else
            {
                Logger.writeInLog(true, "ERROR: Database test was not successfull!");
                return;
            }

            //Network Initialisations
            ActiveConnections = new List<networkServer.networkClientInterface>();
            sAesKey = _sAesKey;
            this.cProtocolDelimiter = _cProtocolDelimiter;
            this.cDataDelimiter = _cDataDelimiter;
            TcpServer = new networkServer(NetworkProtocol, _sAesKey, IPAddress.Any, _iPort, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Logger.writeInLog(true, "TCP Server ready for start!");


            //TESTCASE
            networkServer.networkClientInterface dummy = new networkServer.networkClientInterface();
            //Auth
            //networkProtocol("#104;Anderson2;Lars;Pickelin;miau1234;l.pickelin@web.de", ref dummy);
            //networkProtocol("#102;Anderson2;miau1x234", ref dummy);
            //Content
            //Get all rooms
            //networkProtocol("#201", ref dummy);
            //Add new room
            //  networkProtocol("#203;1;Avelinas Test raum;Hallo Welt;1;http://www.AvelinaLerntArrays.net", ref dummy);
            //Get all workouts of room id 2
            //  NetworkProtocol("#205;2", ref dummy);
            //Get Levels of workout with id 1
            //  NetworkProtocol("#207;1", ref dummy);
            //Get all excercises of workout 1
            //  NetworkProtocol("#209;1", ref dummy);
        }

        public void Start()
        {
            if(TcpServer.startListening())
            {
                Logger.writeInLog(true, "Server has been started successfully!");
            }
            else
            {
                Logger.writeInLog(true, "ERROR: The server was not able to start!");
            }
           
        }
        
        private void NetworkProtocol(string message, ref networkServer.networkClientInterface relatedClient)
        {
            string sProtocolShortcut = GetProtocolShortcut(message);
            // Put data in array
            List<string> lDataList = new List<string>();
            lDataList = GetProtocolData(GetProtocolMessage(message));

            switch (sProtocolShortcut)
            {
                case "#001":
                    Tel_001_testPackage(ref relatedClient); break;
                case "#003":
                    Tel_003_testPackage(lDataList, ref relatedClient); break;
                case "#102":
                    Tel_102_loginUser(lDataList, ref relatedClient); break;
                case "#104":
                    Tel_104_registerUser(lDataList, ref relatedClient); break;
                case "#201":
                    Tel_201_requestRoomOverview(ref relatedClient); break;
                case "#203":
                    Tel_203_requestRoomAdd(lDataList, ref relatedClient); break;
                case "#205":
                    Tel_205_requestWorkoutOverview(lDataList, ref relatedClient); break;
                case "#207":
                    Tel_207_requestLevelOverview(lDataList, ref relatedClient); break;
                case "#209":
                    Tel_209_requestFullExerciseData(lDataList, ref relatedClient); break;
                default:
                    Logger.writeInLog(true, "Unknown package protocol/data received: " + message);
                    break;
            }
        }

        #region telegram functions
        //Testpackages
        private void Tel_001_testPackage(ref networkServer.networkClientInterface relatedClient)
        {
            Logger.writeInLog(true, "Message #001 (TESTPACKET_NORMAL) received from a client!");
            TcpServer.sendMessage("#002;Greetings from Controller :)", relatedClient);
            Logger.writeInLog(true, "Answered #002 with the greetings message!");
        }

        private void Tel_003_testPackage(List<string> lDataList, ref networkServer.networkClientInterface relatedClient)
        {
            Logger.writeInLog(true, "Message #003 (TESTPACKET_LARGE) received from a client!");
            string sDataPackage = "#004;";
            int iForCounter = 40;
            for (int i = 0; i < iForCounter; i++)
            {
                sDataPackage = sDataPackage + "Package number: " + (i + 1) + ". This is a large datapackage to test the network tcp/ip receive function. You should get " + iForCounter + " of these packages! Good luck, programmer!\n";
            }
            TcpServer.sendMessage(sDataPackage, relatedClient);
            Logger.writeInLog(true, "Answered #004 with the large datapacket!");
        }
        
        //Signup
        private void Tel_104_registerUser(List<string> lDataList, ref networkServer.networkClientInterface relatedClient)
        {
            try
            {
                //log
                Logger.writeInLog(true, "Message #104 (SIGNUP) received from a client!");
                //register user
                int iSignUpStatusCode = DatabaseEngine.signUpRegisterUser(lDataList[0], lDataList[1], lDataList[2], lDataList[3], lDataList[4], Convert.ToInt16(lDataList[5]));
                //send message to client
                TcpServer.sendMessage("#105" + cProtocolDelimiter + iSignUpStatusCode, relatedClient);
                Logger.writeInLog(true, "Answered #105 with SignUpCode " + iSignUpStatusCode + "!");
            }
            catch (Exception e)
            {
                Logger.writeInLog(true, "ERROR: Something went wrong with telegram (SIGNUP)! Message: " + e.ToString());
                return;
            }
            
        }
        //Login
        private void Tel_102_loginUser(List<string> lDataList, ref networkServer.networkClientInterface relatedClient)
        {
            try
            {
                //log
                Logger.writeInLog(true, "Message #102 (LOGIN) received from a client!");
                //Try to login user
                int iUserID = 0;
                int iLoginStatusCode = DatabaseEngine.loginUser(lDataList[0], lDataList[1], ref iUserID);
                //Send message to client
                if (iLoginStatusCode != 1)
                {
                    TcpServer.sendMessage("#103" + cProtocolDelimiter + iLoginStatusCode, relatedClient);
                }
                else
                {
                    TcpServer.sendMessage("#103" + cProtocolDelimiter + iLoginStatusCode + cProtocolDelimiter + iUserID, relatedClient);
                }
                Logger.writeInLog(true, "Answered #103 with LoginCode " + iLoginStatusCode + ". The user id is " + iUserID + "!");
            }
            catch (Exception e)
            {
                Logger.writeInLog(true, "ERROR: Something went wrong with telegram (LOGIN)! Message: " + e.ToString());
                return;
            }
            
        }
        //Content
        private void Tel_201_requestRoomOverview(ref networkServer.networkClientInterface relatedClient)
        {
            try
            {
                //log
                Logger.writeInLog(true, "Message #201 (REQ_ROOMOVERVIEWDATA) received from a client!");
                //Get room data
                List<List<string>> llRoomData = new List<List<string>>();
                llRoomData = DatabaseEngine.getRoomOverViewData();
                //Build protocol
                string sProtocol = "#202" + cProtocolDelimiter.ToString();
                sProtocol += llRoomData.Count + cProtocolDelimiter.ToString();
                for (int i = 0; i < llRoomData.Count; i++)
                {
                    for (int d = 0; d < llRoomData[i].Count; d++)
                    {
                        sProtocol += llRoomData[i][d] + cDataDelimiter.ToString();
                    }
                    //Remove the last cDataDelimiter
                    sProtocol = sProtocol.Remove(sProtocol.Length - 1);
                    sProtocol += cProtocolDelimiter.ToString();
                }
                //Remove last cProtocolDelimiter
                sProtocol = sProtocol.Remove(sProtocol.Length - 1);

                //Send message to client
                TcpServer.sendMessage(sProtocol, relatedClient);
                Logger.writeInLog(true, "Answered #202 with all room data. " + llRoomData.Count + " room entries sent!");

            }
            catch (Exception e)
            {
                Logger.writeInLog(true, "ERROR: Something went wrong with telegram (REQ_ROOMOVERVIEWDATA)! Message: " + e.ToString());
                return;
            }
        }

        private void Tel_203_requestRoomAdd(List<string> lDataList, ref networkServer.networkClientInterface relatedClient)
        {
            try
            {
                //log
                Logger.writeInLog(true, "Message #203 (REQ_ROOMADD) received from a client!");
                //Try to add room + add trainer to room
                int iRoomAddStatus = DatabaseEngine.addNewRoom(Convert.ToInt32(lDataList[0]), 
                                                               lDataList[1], lDataList[2], 
                                                               Convert.ToInt16(lDataList[3]), 
                                                               lDataList[4]);
                //Send message to client
                TcpServer.sendMessage("#204" + cProtocolDelimiter + iRoomAddStatus, relatedClient);
                Logger.writeInLog(true, "Answered #205 with status code "+ iRoomAddStatus + "!");
            }
            catch (Exception e)
            {
                Logger.writeInLog(true, "ERROR: Something went wrong with telegram (REQ_ROOMADD)! Message: " + e.ToString());
                return;
            }
        }

        private void Tel_205_requestWorkoutOverview(List<string> lDataList, ref networkServer.networkClientInterface relatedClient)
        {
            try
            {
                //log
                Logger.writeInLog(true, "Message #205 (REQ_WORKOUTOVERVIEWDATA) received from a client!");
                //Get room data
                List<List<string>> llWorkoutData = new List<List<string>>();
                llWorkoutData = DatabaseEngine.getWorkoutOverViewData(Convert.ToInt32(lDataList[0]));
                //Build protocol
                string sProtocol = "#206" + cProtocolDelimiter.ToString();
                sProtocol += llWorkoutData.Count + cProtocolDelimiter.ToString();
                for (int i = 0; i < llWorkoutData.Count; i++)
                {
                    for (int d = 0; d < llWorkoutData[i].Count; d++)
                    {
                        sProtocol += llWorkoutData[i][d] + cDataDelimiter.ToString();
                    }
                    //Remove the last cDataDelimiter
                    sProtocol = sProtocol.Remove(sProtocol.Length - 1);

                    sProtocol += cProtocolDelimiter.ToString();
                }
                //Remove last cProtocolDelimiter
                sProtocol = sProtocol.Remove(sProtocol.Length - 1);

                //Send message to client
                TcpServer.sendMessage(sProtocol, relatedClient);
                Logger.writeInLog(true, "Answered #206 with all workout overview data. " + llWorkoutData.Count + " workout entries sent!");

            }
            catch (Exception e)
            {
                Logger.writeInLog(true, "ERROR: Something went wrong with telegram (REQ_ROOMOVERVIEWDATA)! Message: " + e.ToString());
                return;
            }
        }

        private void Tel_207_requestLevelOverview(List<string> lDataList, ref networkServer.networkClientInterface relatedClient)
        {
            try
            {
                //log
                Logger.writeInLog(true, "Message #207 (REQ_LEVELOVERVIEW) received from a client!");
                //Get room data
                List<List<string>> llLevel = new List<List<string>>();
                llLevel = DatabaseEngine.getLevelOverviewData(Convert.ToInt32(lDataList[0]));
                //Build protocol
                string sProtocol = "#208" + cProtocolDelimiter.ToString();
                //sProtocol += llWorkoutData.Count + cProtocolDelimiter.ToString(); <- Keine Anzahl angegeben
                for (int i = 0; i < llLevel.Count; i++)
                {

                    for (int d = 0; d < llLevel[i].Count; d++)
                    {
                        sProtocol += llLevel[i][d] + cDataDelimiter.ToString();
                    }
                    sProtocol += cProtocolDelimiter.ToString();                   
                    //Remove the last cDataDelimiter
                }
                //Remove last cProtocolDelimiter
                sProtocol = sProtocol.Remove(sProtocol.Length - 1);

                //Send message to client
                TcpServer.sendMessage(sProtocol, relatedClient);
                Logger.writeInLog(true, "Answered #208 with all levels for workout with ID: "+ lDataList[0]  + ". " + llLevel.Count + " levels sent!");
            }
            catch (Exception e)
            {
                Logger.writeInLog(true, "ERROR: Something went wrong with telegram (REQ_LEVELOVERVIEW)! Message: " + e.ToString());
                return;
            }
        }

        private void Tel_209_requestFullExerciseData(List<string> lDataList, ref networkServer.networkClientInterface relatedClient)
        {
            try
            {
                //log
                Logger.writeInLog(true, "Message #209 (REQ_FULLEXERCISEDATA) received from a client!");
                //Get room data
                List<List<string>> llExercises = new List<List<string>>();
                llExercises = DatabaseEngine.getFullExerciseData(Convert.ToInt32(lDataList[0]));
                //Build protocol
                string sProtocol = "#210" + cProtocolDelimiter.ToString();
                sProtocol += llExercises.Count + cProtocolDelimiter.ToString();
                for (int i = 0; i < llExercises.Count; i++)
                {
                    for (int d = 0; d < llExercises[i].Count; d++)
                    {
                        sProtocol += llExercises[i][d] + cDataDelimiter.ToString();
                    }
                    sProtocol += cProtocolDelimiter.ToString();
                    //Remove the last cDataDelimiter
                }
                //Remove last cProtocolDelimiter
                sProtocol = sProtocol.Remove(sProtocol.Length - 1);

                //Send message to client
                TcpServer.sendMessage(sProtocol, relatedClient);
                Logger.writeInLog(true, "Answered #210 with all levels for workout with ID: " + lDataList[0] + ". " + llExercises.Count + " levels sent!");
            }
            catch (Exception e)
            {
                Logger.writeInLog(true, "ERROR: Something went wrong with telegram (REQ_LEVELOVERVIEW)! Message: " + e.ToString());
                return;
            }
        }

        #endregion


        #region Support functions
        private List<string> GetProtocolData(string message)
        {
            return message.Split(cProtocolDelimiter).ToList();
        }

        private string GetProtocolShortcut(string message)
        {
            return message.Split(cProtocolDelimiter)[0];
        }
        private string GetProtocolMessage(string message)
        {
            try
            {
                return message.Substring(GetProtocolShortcut(message).Length + 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                return "-";
            }
            
        }
        #endregion
        
    }
}
