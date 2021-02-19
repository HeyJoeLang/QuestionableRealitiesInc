using UnityEngine;
using System.Collections;
using System.IO;
using System;

namespace LunarCatsStudio.SuperCombiner
{
    public class Logger
    {
        string _logs = " ";
        static Logger _instance;
        static bool _display = true;

        /// <summary>
        /// logs level
        /// </summary>
        public enum LogLevel
        {
            LOG_DEBUG,
            LOG_WARNING,
            LOG_ERROR
        }

        /// <summary>
        /// get instance of the logger singleton
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                }
                return _instance;
            }
        }


        /// <summary>
        /// General switch for display or not new logs add in logger store
        /// </summary>
        public static bool Display
        {
            get
            {
                return _display;
            }

            set
            {
                _display = value;
            }
        }

        /// <summary>
        /// add new log in the logger store
        /// </summary>
        /// <param name="tag"> log tag</param>
        /// <param name="log">log message</param>
        /// <param name="level">log level</param>
        /// <param name="display">display or not this new log in editor console</param>
        public void AddLog(string tag, string log, LogLevel level = LogLevel.LOG_DEBUG, bool display = true)
        {
            string log_type = " ";
            string new_log = "[" + tag + "] " + log;
            switch (level)
            {
                case LogLevel.LOG_WARNING:
                    log_type = "WARNING: ";
                    if (display && _display) Debug.LogWarning(new_log);
                    break;


                case LogLevel.LOG_ERROR:
                    log_type = "ERROR: ";
                    if (display && _display) Debug.LogError(new_log);
                    break;


                default:
                    log_type = "DEBUG: ";
                    if (display && _display) Debug.Log(new_log);
                    break;
            }
            _logs = _logs + log_type + new_log + "\n";
        }

        /// <summary>
        /// clear logs store
        /// </summary>
        public void ClearLogs()
        {
            _logs = " ";
        }

        /// <summary>
        /// get logs store content
        /// </summary>
        /// <returns></returns>
        public string GetLogs()
        {
            return _logs;
        }
    }
}
