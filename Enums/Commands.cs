//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Text;

//namespace Daisi.SDK.Enums
//{
//    /// <summary>
//    /// These are a list of commands that can be sent.
//    /// </summary>
//    /// <param name="Name">The name of the command</param>
//    /// <param name="Parameters"></param>
//    public record Commands(string Name, string[] RequiredParameters)
//    {        
//        public static Commands SetAllSettingsRequest = new Commands(nameof(SetAllSettingsRequest), []);
//        public static Commands GetAllSettingsRequest = new Commands(nameof(GetAllSettingsRequest), []);
//        public static Commands HeartbeatRequest = new Commands(nameof(HeartbeatRequest), []);
//        public static Commands ConnectRequest = new(nameof(ConnectRequest), []);
//        public static Commands ConnectResponse = new(nameof(ConnectResponse), []);
//        public static Commands SendRequest = new(nameof(SendRequest), []);
//        public static Commands SendResponse = new(nameof(SendResponse), []);

//        /// <summary>
//        /// Commands that can be sent to the Host.
//        /// </summary>
//        public static Dictionary<string, Commands> HostCommands = new Dictionary<string, Commands>() {
//            { ConnectRequest.Name, ConnectRequest }, // Request to create a new chat connection
//            { SendRequest.Name, SendRequest },
//            { GetAllSettingsRequest.Name, GetAllSettingsRequest  },
//            { SetAllSettingsRequest.Name, SetAllSettingsRequest  },
//        };

//        /// <summary>
//        /// Commands that can be sent to the Orc.
//        /// </summary>
//        public static Dictionary<string, Commands> OrcCommands = new Dictionary<string, Commands>() {
//            { HeartbeatRequest.Name, HeartbeatRequest }, // Request to log Host heartbeat
//            { ConnectResponse.Name, ConnectResponse }, // Response to a connection request
//            { SendResponse.Name, SendResponse } //Response to a send request
//        };

//    }

//    public record CommandParameters(string Name)
//    {
//        public static CommandParameters ClientKey = new CommandParameters(nameof(ClientKey));
//        public static CommandParameters Host = new CommandParameters(nameof(Host));
//        public static CommandParameters Json = new CommandParameters(nameof(Json));
//    }
//}
