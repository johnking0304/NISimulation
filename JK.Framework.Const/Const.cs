using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JK.Framework.Const
{

    public enum Result
    {
        OK = 0,
        FAIL = 1
    }


    public enum ChannelErrorCode
    {
        None =-1,
        Ok = 0,
        InitError = 1,
        ReadError = 2,
        WriteError = 3,
        
           
    }

    public class Resource
    {
        private const string CONFIG_FILENAME = "config.ini";
        private const string MAIN_DATA_FOLDER = "data\\";
        private const string MAIN_LOGS_FOLDER = "logs\\";
        private const string PROFILE_CONFIG_FILENAME = "profile.cfg";
        private const string CONFIG_DATA_FILE = "manager_config_data.cfg";
        private const string ABOUT_FILENAME = "About.exe";
        private const string EXCEPTION_DIRECTORY = "exception\\";
        



        public const string DEFAULT_BASE_URL = "http://127.0.0.1:8000/";



        public const string REFRESH_NIBOX_LIST_URL = "simulation/nibox/list/";




        public static string BoxClientsDataFile
        {
            get
            {
                return AppPath + CONFIG_DATA_FILE;
            }
        }
        public static string AppPath
        {
            get
            {
                return System.AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public static string ProfileFileName
        {
            get
            {
                return "\\" + PROFILE_CONFIG_FILENAME;
            }
        }


        public static string ExceptionDirectory
        {
            get
            {
                return AppPath + EXCEPTION_DIRECTORY;
            }
        }

        public static string AboutFileName
        {
            get
            {
                return AppPath + ABOUT_FILENAME;
            }
        }


        public static string ConfigFileName
        {
            get
            {
                return AppPath + CONFIG_FILENAME;
            }
        }

        public static string MainLogsPath
        {
            get
            {
                return AppPath + MAIN_LOGS_FOLDER;
            }
        }



        public static string MainDataPath
        {
            get
            {
                return AppPath + MAIN_DATA_FOLDER;
            }
        }


    }
}
