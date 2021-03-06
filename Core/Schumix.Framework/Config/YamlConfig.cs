﻿/*
 * This file is part of Schumix.
 * 
 * Copyright (C) 2010-2012 Megax <http://www.megaxx.info/>
 * Copyright (C) 2012 Jackneill
 * 
 * Schumix is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Schumix is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Schumix.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Schumix.Framework.Extensions;
using Schumix.Framework.Localization;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Schumix.Framework.Config
{
	public sealed class YamlConfig : DefaultConfig
	{
		private readonly LocalizationConsole sLConsole = Singleton<LocalizationConsole>.Instance;
		private readonly Utilities sUtilities = Singleton<Utilities>.Instance;
		private readonly Dictionary<YamlNode, YamlNode> NullYMap = null;

		public YamlConfig()
		{
		}

		public YamlConfig(string configdir, string configfile, bool colorbindmode)
		{
			var yaml = new YamlStream();
			yaml.Load(File.OpenText(sUtilities.DirectoryToSpecial(configdir, configfile)));

			var schumixmap = (yaml.Documents.Count > 0 && ((YamlMappingNode)yaml.Documents[0].RootNode).Children.ContainsKey("Schumix")) ? ((YamlMappingNode)((YamlMappingNode)yaml.Documents[0].RootNode).Children["Schumix".ToYamlNode()]).Children : NullYMap;
			LogMap((!schumixmap.IsNull() && schumixmap.ContainsKey("Log")) ? ((YamlMappingNode)schumixmap["Log".ToYamlNode()]).Children : NullYMap);
			Log.Initialize(LogConfig.FileName, colorbindmode);
			Log.Debug("YamlConfig", ">> {0}", configfile);

			Log.Notice("YamlConfig", sLConsole.Config("Text3"));
			ServerMap((!schumixmap.IsNull() && schumixmap.ContainsKey("Server")) ? ((YamlMappingNode)schumixmap["Server".ToYamlNode()]).Children : NullYMap);

			if((!schumixmap.IsNull() && schumixmap.ContainsKey("Irc")))
			{
				var list = new Dictionary<YamlNode, YamlNode>();

				foreach(var irc in schumixmap)
				{
					if(irc.Key.ToString().Contains("Irc"))
						list.Add(irc.Key, irc.Value);
				}

				IrcMap(list);
			}
			else
				IrcMap(NullYMap);

			MySqlMap((!schumixmap.IsNull() && schumixmap.ContainsKey("MySql")) ? ((YamlMappingNode)schumixmap["MySql".ToYamlNode()]).Children : NullYMap);
			SQLiteMap((!schumixmap.IsNull() && schumixmap.ContainsKey("SQLite")) ? ((YamlMappingNode)schumixmap["SQLite".ToYamlNode()]).Children : NullYMap);
			CrashMap((!schumixmap.IsNull() && schumixmap.ContainsKey("Crash")) ? ((YamlMappingNode)schumixmap["Crash".ToYamlNode()]).Children : NullYMap);
			LocalizationMap((!schumixmap.IsNull() && schumixmap.ContainsKey("Localization")) ? ((YamlMappingNode)schumixmap["Localization".ToYamlNode()]).Children : NullYMap);
			ShutdownMap((!schumixmap.IsNull() && schumixmap.ContainsKey("Shutdown")) ? ((YamlMappingNode)schumixmap["Shutdown".ToYamlNode()]).Children : NullYMap);
			CleanMap((!schumixmap.IsNull() && schumixmap.ContainsKey("Clean")) ? ((YamlMappingNode)schumixmap["Clean".ToYamlNode()]).Children : NullYMap);

			Log.Success("YamlConfig", sLConsole.Config("Text4"));
			Console.WriteLine();
		}

		~YamlConfig()
		{
		}

		public bool CreateConfig(string ConfigDirectory, string ConfigFile, bool ColorBindMode)
		{
			try
			{
				string filename = sUtilities.DirectoryToSpecial(ConfigDirectory, ConfigFile);

				if(File.Exists(filename))
					return true;
				else
				{
					new LogConfig(d_logfilename, d_logdatefilename, d_logmaxfilesize, 3, d_logdirectory, d_irclogdirectory, d_irclog);
					Log.Initialize(d_logfilename, ColorBindMode);
					Log.Error("YamlConfig", sLConsole.Config("Text5"));
					Log.Debug("YamlConfig", sLConsole.Config("Text6"));
					var yaml = new YamlStream();
					string filename2 = sUtilities.DirectoryToSpecial(ConfigDirectory, "_" + ConfigFile);

					if(File.Exists(filename2))
						yaml.Load(File.OpenText(filename2));

					try
					{
						var schumixmap = (yaml.Documents.Count > 0 && ((YamlMappingNode)yaml.Documents[0].RootNode).Children.ContainsKey("Schumix")) ? ((YamlMappingNode)((YamlMappingNode)yaml.Documents[0].RootNode).Children["Schumix".ToYamlNode()]).Children : NullYMap;
						var nodes = new YamlMappingNode();
						var nodes2 = new YamlMappingNode();
						nodes2.Add("Server",       CreateServerMap((!schumixmap.IsNull() && schumixmap.ContainsKey("Server")) ? ((YamlMappingNode)schumixmap["Server".ToYamlNode()]).Children : NullYMap));

						if((!schumixmap.IsNull() && schumixmap.ContainsKey("Irc")))
						{
							foreach(var irc in schumixmap)
							{
								if(irc.Key.ToString().Contains("Irc"))
									nodes2.Add(irc.Key, CreateIrcMap(((YamlMappingNode)irc.Value).Children));
							}
						}
						else
							nodes2.Add("Irc",      CreateIrcMap(NullYMap));

						nodes2.Add("Log",          CreateLogMap((!schumixmap.IsNull() && schumixmap.ContainsKey("Log")) ? ((YamlMappingNode)schumixmap["Log".ToYamlNode()]).Children : NullYMap));
						nodes2.Add("MySql",        CreateMySqlMap((!schumixmap.IsNull() && schumixmap.ContainsKey("MySql")) ? ((YamlMappingNode)schumixmap["MySql".ToYamlNode()]).Children : NullYMap));
						nodes2.Add("SQLite",       CreateSQLiteMap((!schumixmap.IsNull() && schumixmap.ContainsKey("SQLite")) ? ((YamlMappingNode)schumixmap["SQLite".ToYamlNode()]).Children : NullYMap));
						nodes2.Add("Crash",        CreateCrashMap((!schumixmap.IsNull() && schumixmap.ContainsKey("Crash")) ? ((YamlMappingNode)schumixmap["Crash".ToYamlNode()]).Children : NullYMap));
						nodes2.Add("Localization", CreateLocalizationMap((!schumixmap.IsNull() && schumixmap.ContainsKey("Localization")) ? ((YamlMappingNode)schumixmap["Localization".ToYamlNode()]).Children : NullYMap));
						nodes2.Add("Shutdown",     CreateShutdownMap((!schumixmap.IsNull() && schumixmap.ContainsKey("Shutdown")) ? ((YamlMappingNode)schumixmap["Shutdown".ToYamlNode()]).Children : NullYMap));
						nodes2.Add("Clean",        CreateCleanMap((!schumixmap.IsNull() && schumixmap.ContainsKey("Clean")) ? ((YamlMappingNode)schumixmap["Clean".ToYamlNode()]).Children : NullYMap));
						nodes.Add("Schumix", nodes2);

						sUtilities.CreateFile(filename);
						var file = new StreamWriter(filename, true) { AutoFlush = true };
						file.Write(nodes.Children.ToString("Schumix"));
						file.Close();

						if(File.Exists(filename2))
							File.Delete(filename2);

						Log.Success("YamlConfig", sLConsole.Config("Text7"));
					}
					catch(Exception e)
					{
						Log.Error("YamlConfig", sLConsole.Config("Text13"), e.Message);
						errors = true;
					}
				}
			}
			catch(DirectoryNotFoundException)
			{
				CreateConfig(ConfigDirectory, ConfigFile, ColorBindMode);
			}

			return false;
		}

		private void LogMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			string LogFileName = (!nodes.IsNull() && nodes.ContainsKey("FileName")) ? nodes["FileName".ToYamlNode()].ToString() : d_logfilename;
			bool LogDateFileName = (!nodes.IsNull() && nodes.ContainsKey("DateFileName")) ? Convert.ToBoolean(nodes["DateFileName".ToYamlNode()].ToString()) : d_logdatefilename;
			int LogMaxFileSize = (!nodes.IsNull() && nodes.ContainsKey("MaxFileSize")) ? Convert.ToInt32(nodes["MaxFileSize".ToYamlNode()].ToString()) : d_logmaxfilesize;
			int LogLevel = (!nodes.IsNull() && nodes.ContainsKey("LogLevel")) ? Convert.ToInt32(nodes["LogLevel".ToYamlNode()].ToString()) : d_loglevel;
			string LogDirectory = (!nodes.IsNull() && nodes.ContainsKey("LogDirectory")) ? nodes["LogDirectory".ToYamlNode()].ToString() : d_logdirectory;
			string IrcLogDirectory = (!nodes.IsNull() && nodes.ContainsKey("IrcLogDirectory")) ? nodes["IrcLogDirectory".ToYamlNode()].ToString() : d_irclogdirectory;
			bool IrcLog = (!nodes.IsNull() && nodes.ContainsKey("IrcLog")) ? Convert.ToBoolean(nodes["IrcLog".ToYamlNode()].ToString()) : d_irclog;

			new LogConfig(LogFileName, LogDateFileName, LogMaxFileSize, LogLevel, sUtilities.GetSpecialDirectory(LogDirectory), sUtilities.GetSpecialDirectory(IrcLogDirectory), IrcLog);
		}

		private void ServerMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			bool ServerEnabled = (!nodes.IsNull() && nodes.ContainsKey("Enabled")) ? Convert.ToBoolean(nodes["Enabled".ToYamlNode()].ToString()) : d_serverenabled;
			string ServerHost = (!nodes.IsNull() && nodes.ContainsKey("Host")) ? nodes["Host".ToYamlNode()].ToString() : d_serverhost;
			int ServerPort = (!nodes.IsNull() && nodes.ContainsKey("Port")) ? Convert.ToInt32(nodes["Port".ToYamlNode()].ToString()) : d_serverport;
			string ServerPassword = (!nodes.IsNull() && nodes.ContainsKey("Password")) ? nodes["Password".ToYamlNode()].ToString() : d_serverpassword;

			new ServerConfig(ServerEnabled, ServerHost, ServerPort, ServerPassword);
		}

		private void IrcMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			int ServerId = 1;
			var IrcList = new Dictionary<string, IRCConfigBase>();

			if(nodes.IsNull())
			{
				string ServerName            = d_servername;
				string Server                = d_server;
				int Port                     = d_port;
				bool Ssl                     = d_ssl;
				string NickName              = d_nickname;
				string NickName2             = d_nickname2;
				string NickName3             = d_nickname3;
				string UserName              = d_username;
				string UserInfo              = d_userinfo;
				string MasterChannel         = d_masterchannel;
				string MasterChannelPassword = d_masterchannelpassword;
				string IgnoreChannels        = d_ignorechannels;
				string IgnoreNames           = d_ignorenames;
				bool UseNickServ             = d_usenickserv;
				string NickServPassword      = d_nickservpassword;
				bool UseHostServ             = d_usehostserv;
				bool HostServStatus          = d_hostservstatus;
				int MessageSending           = d_messagesending;
				string CommandPrefix         = d_commandprefix;
				string MessageType           = d_messagetype;

				IrcList.Add(ServerName.ToLower(), new IRCConfigBase(ServerId, Server, Port, Ssl, NickName, NickName2, NickName3, UserName, UserInfo, MasterChannel, MasterChannelPassword.Trim(), IgnoreChannels, IgnoreNames, UseNickServ, NickServPassword, UseHostServ, HostServStatus, MessageSending, CommandPrefix, MessageType));
			}
			else
			{
				foreach(var irc in nodes)
				{
					var node = ((YamlMappingNode)irc.Value).Children;
					string ServerName = (!node.IsNull() && node.ContainsKey("ServerName")) ? node["ServerName".ToYamlNode()].ToString() : d_servername;
					string Server = (!node.IsNull() && node.ContainsKey("Server")) ? node["Server".ToYamlNode()].ToString() : d_server;
					int Port = (!node.IsNull() && node.ContainsKey("Port")) ? Convert.ToInt32(node["Port".ToYamlNode()].ToString()) : d_port;
					bool Ssl = (!node.IsNull() && node.ContainsKey("Ssl")) ? Convert.ToBoolean(node["Ssl".ToYamlNode()].ToString()) : d_ssl;
					string NickName = (!node.IsNull() && node.ContainsKey("NickName")) ? node["NickName".ToYamlNode()].ToString() : d_nickname;
					string NickName2 = (!node.IsNull() && node.ContainsKey("NickName2")) ? node["NickName2".ToYamlNode()].ToString() : d_nickname2;
					string NickName3 = (!node.IsNull() && node.ContainsKey("NickName3")) ? node["NickName3".ToYamlNode()].ToString() : d_nickname3;
					string UserName = (!node.IsNull() && node.ContainsKey("UserName")) ? node["UserName".ToYamlNode()].ToString() : d_username;
					string UserInfo = (!node.IsNull() && node.ContainsKey("UserInfo")) ? node["UserInfo".ToYamlNode()].ToString() : d_userinfo;

					string MasterChannel = d_masterchannel;
					string MasterChannelPassword = d_masterchannelpassword;

					if(!node.IsNull() && node.ContainsKey("MasterChannel"))
					{
						var node2 = ((YamlMappingNode)node["MasterChannel".ToYamlNode()]).Children;
						MasterChannel = (!node2.IsNull() && node2.ContainsKey("Name")) ? node2["Name".ToYamlNode()].ToString() : d_masterchannel;
						MasterChannelPassword = (!node2.IsNull() && node2.ContainsKey("Password")) ? node2["Password".ToYamlNode()].ToString() : d_masterchannelpassword;
					}

					string IgnoreChannels = (!node.IsNull() && node.ContainsKey("IgnoreChannels")) ? node["IgnoreChannels".ToYamlNode()].ToString() : d_ignorechannels;
					string IgnoreNames = (!node.IsNull() && node.ContainsKey("IgnoreNames")) ? node["IgnoreNames".ToYamlNode()].ToString() : d_ignorenames;

					bool UseNickServ = d_usenickserv;
					string NickServPassword = d_nickservpassword;

					if(!node.IsNull() && node.ContainsKey("NickServ"))
					{
						var node2 = ((YamlMappingNode)node["NickServ".ToYamlNode()]).Children;
						UseNickServ = (!node2.IsNull() && node2.ContainsKey("Enabled")) ? Convert.ToBoolean(node2["Enabled".ToYamlNode()].ToString()) : d_usenickserv;
						NickServPassword = (!node2.IsNull() && node2.ContainsKey("Password")) ? node2["Password".ToYamlNode()].ToString() : d_nickservpassword;
					}

					bool UseHostServ = d_usehostserv;
					bool HostServStatus = d_hostservstatus;

					if(!node.IsNull() && node.ContainsKey("HostServ"))
					{
						var node2 = ((YamlMappingNode)node["HostServ".ToYamlNode()]).Children;
						UseHostServ = (!node2.IsNull() && node2.ContainsKey("Enabled")) ? Convert.ToBoolean(node2["Enabled".ToYamlNode()].ToString()) : d_usehostserv;
						HostServStatus = (!node2.IsNull() && node2.ContainsKey("Vhost")) ? Convert.ToBoolean(node2["Vhost".ToYamlNode()].ToString()) : d_hostservstatus;
					}

					int MessageSending = d_messagesending;

					if(!node.IsNull() && node.ContainsKey("Wait"))
					{
						var node2 = ((YamlMappingNode)node["Wait".ToYamlNode()]).Children;
						MessageSending = (!node2.IsNull() && node2.ContainsKey("MessageSending")) ? Convert.ToInt32(node2["MessageSending".ToYamlNode()].ToString()) : d_messagesending;
					}

					string CommandPrefix = d_commandprefix;

					if(!node.IsNull() && node.ContainsKey("Command"))
					{
						var node2 = ((YamlMappingNode)node["Command".ToYamlNode()]).Children;
						CommandPrefix = (!node2.IsNull() && node2.ContainsKey("Prefix")) ? node2["Prefix".ToYamlNode()].ToString() : d_commandprefix;
					}

					string MessageType = (!node.IsNull() && node.ContainsKey("MessageType")) ? node["MessageType".ToYamlNode()].ToString() : d_messagetype;

					if(MasterChannel.Length >= 2 && MasterChannel.Trim().Length > 1 && MasterChannel.Substring(0, 1) != "#")
						MasterChannel = "#" + MasterChannel;
					else if(MasterChannel.Length < 2 && MasterChannel.Trim().Length <= 1)
						MasterChannel = d_masterchannel;

					if(IrcList.ContainsKey(ServerName.ToLower()))
						Log.Error("YmlConfig", sLConsole.Config("Text12"), ServerName);
					else
					{
						IrcList.Add(ServerName.ToLower(), new IRCConfigBase(ServerId, Server, Port, Ssl, NickName, NickName2, NickName3, UserName, UserInfo, MasterChannel, MasterChannelPassword.Trim(), IgnoreChannels, IgnoreNames, UseNickServ, NickServPassword, UseHostServ, HostServStatus, MessageSending, CommandPrefix, MessageType));
						ServerId++;
					}
				}
			}

			new IRCConfig(IrcList);
		}

		private void MySqlMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			bool Enabled = (!nodes.IsNull() && nodes.ContainsKey("Enabled")) ? Convert.ToBoolean(nodes["Enabled".ToYamlNode()].ToString()) : d_mysqlenabled;
			string Host = (!nodes.IsNull() && nodes.ContainsKey("Host")) ? nodes["Host".ToYamlNode()].ToString() : d_mysqlhost;
			string User = (!nodes.IsNull() && nodes.ContainsKey("User")) ? nodes["User".ToYamlNode()].ToString() : d_mysqluser;
			string Password = (!nodes.IsNull() && nodes.ContainsKey("Password")) ? nodes["Password".ToYamlNode()].ToString() : d_mysqlpassword;
			string Database = (!nodes.IsNull() && nodes.ContainsKey("Database")) ? nodes["Database".ToYamlNode()].ToString() : d_mysqldatabase;
			string Charset = (!nodes.IsNull() && nodes.ContainsKey("Charset")) ? nodes["Charset".ToYamlNode()].ToString() : d_mysqlcharset;

			new MySqlConfig(Enabled, Host, User, Password, Database, Charset);
		}

		private void SQLiteMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			bool Enabled = (!nodes.IsNull() && nodes.ContainsKey("Enabled")) ? Convert.ToBoolean(nodes["Enabled".ToYamlNode()].ToString()) : d_sqliteenabled;
			string FileName = (!nodes.IsNull() && nodes.ContainsKey("FileName")) ? nodes["FileName".ToYamlNode()].ToString() : d_sqlitefilename;

			new SQLiteConfig(Enabled, sUtilities.GetSpecialDirectory(FileName));
		}

		private void CrashMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			string Directory = (!nodes.IsNull() && nodes.ContainsKey("Directory")) ? nodes["Directory".ToYamlNode()].ToString() : d_crashdirectory;

			new CrashConfig(sUtilities.GetSpecialDirectory(Directory));
		}

		private void LocalizationMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			string Locale = (!nodes.IsNull() && nodes.ContainsKey("Locale")) ? nodes["Locale".ToYamlNode()].ToString() : d_locale;

			new LocalizationConfig(Locale);
		}

		private void ShutdownMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			int MaxMemory = (!nodes.IsNull() && nodes.ContainsKey("MaxMemory")) ? Convert.ToInt32(nodes["MaxMemory".ToYamlNode()].ToString()) : d_shutdownmaxmemory;

			new ShutdownConfig(MaxMemory);
		}

		private void CleanMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			bool Config = (!nodes.IsNull() && nodes.ContainsKey("Config")) ? Convert.ToBoolean(nodes["Config".ToYamlNode()].ToString()) : d_cleanconfig;
			bool Database = (!nodes.IsNull() && nodes.ContainsKey("Database")) ? Convert.ToBoolean(nodes["Database".ToYamlNode()].ToString()) : d_cleandatabase;
			new CleanConfig(Config, Database);
		}

		private YamlMappingNode CreateServerMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			var map = new YamlMappingNode();
			map.Add("Enabled",  (!nodes.IsNull() && nodes.ContainsKey("Enabled")) ? nodes["Enabled".ToYamlNode()].ToString() : d_serverenabled.ToString());
			map.Add("Host",     (!nodes.IsNull() && nodes.ContainsKey("Host")) ? nodes["Host".ToYamlNode()].ToString() : d_serverhost);
			map.Add("Port",     (!nodes.IsNull() && nodes.ContainsKey("Port")) ? nodes["Port".ToYamlNode()].ToString() : d_serverport.ToString());
			map.Add("Password", (!nodes.IsNull() && nodes.ContainsKey("Password")) ? nodes["Password".ToYamlNode()].ToString() : d_serverpassword);
			return map;
		}

		private YamlMappingNode CreateLogMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			var map = new YamlMappingNode();
			map.Add("FileName",        (!nodes.IsNull() && nodes.ContainsKey("FileName")) ? nodes["FileName".ToYamlNode()].ToString() : d_logfilename);
			map.Add("DateFileName",    (!nodes.IsNull() && nodes.ContainsKey("DateFileName")) ? nodes["DateFileName".ToYamlNode()].ToString() : d_logdatefilename.ToString());
			map.Add("MaxFileSize",     (!nodes.IsNull() && nodes.ContainsKey("MaxFileSize")) ? nodes["MaxFileSize".ToYamlNode()].ToString() : d_logmaxfilesize.ToString());
			map.Add("LogLevel",        (!nodes.IsNull() && nodes.ContainsKey("LogLevel")) ? nodes["LogLevel".ToYamlNode()].ToString() : d_loglevel.ToString());
			map.Add("LogDirectory",    (!nodes.IsNull() && nodes.ContainsKey("LogDirectory")) ? nodes["LogDirectory".ToYamlNode()].ToString() : d_logdirectory);
			map.Add("IrcLogDirectory", (!nodes.IsNull() && nodes.ContainsKey("IrcLogDirectory")) ? nodes["IrcLogDirectory".ToYamlNode()].ToString() : d_irclogdirectory);
			map.Add("IrcLog",          (!nodes.IsNull() && nodes.ContainsKey("IrcLog")) ? nodes["IrcLog".ToYamlNode()].ToString() : d_irclog.ToString());
			return map;
		}

		private YamlMappingNode CreateIrcMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			var map = new YamlMappingNode();

			if(nodes.IsNull())
			{
				map.Add("ServerName",      d_servername);
				map.Add("Server",          d_server);
				map.Add("Port",            d_port.ToString());
				map.Add("Ssl",             d_ssl.ToString());
				map.Add("NickName",        d_nickname);
				map.Add("NickName2",       d_nickname2);
				map.Add("NickName3",       d_nickname3);
				map.Add("UserName",        d_username);
				map.Add("UserInfo",        d_userinfo);
				var map2 = new YamlMappingNode();
				map2.Add("Name",           "\"" + d_masterchannel + "\"");
				map2.Add("Password",       d_masterchannelpassword);
				map.Add("MasterChannel",   map2);
				map.Add("IgnoreChannels",  d_ignorechannels);
				map.Add("IgnoreNames",     d_ignorenames);
				map2 = new YamlMappingNode();
				map2.Add("Enabled",        d_usenickserv.ToString());
				map2.Add("Password",       d_nickservpassword);
				map.Add("NickServ",        map2);
				map2 = new YamlMappingNode();
				map2.Add("Enabled",        d_usehostserv.ToString());
				map2.Add("Vhost",          d_hostservstatus.ToString());
				map.Add("HostServ",        map2);
				map2 = new YamlMappingNode();
				map2.Add("MessageSending", d_messagesending.ToString());
				map.Add("Wait",            map2);
				map2 = new YamlMappingNode();
				map2.Add("Prefix",         "\"" + d_commandprefix + "\"");
				map.Add("Command",         map2);
				map.Add("MessageType",     d_messagetype);
			}
			else
			{
				var node = nodes;
				map.Add("ServerName", (!node.IsNull() && node.ContainsKey("ServerName")) ? node["ServerName".ToYamlNode()].ToString() : d_servername);
				map.Add("Server",     (!node.IsNull() && node.ContainsKey("Server")) ? node["Server".ToYamlNode()].ToString() : d_server);
				map.Add("Port",       (!node.IsNull() && node.ContainsKey("Port")) ? node["Port".ToYamlNode()].ToString() : d_port.ToString());
				map.Add("Ssl",        (!node.IsNull() && node.ContainsKey("Ssl")) ? node["Ssl".ToYamlNode()].ToString() : d_ssl.ToString());
				map.Add("NickName",   (!node.IsNull() && node.ContainsKey("NickName")) ? node["NickName".ToYamlNode()].ToString() : d_nickname);
				map.Add("NickName2",  (!node.IsNull() && node.ContainsKey("NickName2")) ? node["NickName2".ToYamlNode()].ToString() : d_nickname2);
				map.Add("NickName3",  (!node.IsNull() && node.ContainsKey("NickName3")) ? node["NickName3".ToYamlNode()].ToString() : d_nickname3);
				map.Add("UserName",   (!node.IsNull() && node.ContainsKey("UserName")) ? node["UserName".ToYamlNode()].ToString() : d_username);
				map.Add("UserInfo",   (!node.IsNull() && node.ContainsKey("UserInfo")) ? node["UserInfo".ToYamlNode()].ToString() : d_userinfo);

				var map2 = new YamlMappingNode();

				if(!node.IsNull() && node.ContainsKey("MasterChannel"))
				{
					var node2 = ((YamlMappingNode)node["MasterChannel".ToYamlNode()]).Children;
					map2.Add("Name",     "\"" + ((!node2.IsNull() && node2.ContainsKey("Name")) ? node2["Name".ToYamlNode()].ToString() : d_masterchannel) + "\"");
					map2.Add("Password", (!node2.IsNull() && node2.ContainsKey("Password")) ? node2["Password".ToYamlNode()].ToString() : d_masterchannelpassword);
				}
				else
				{
					map2.Add("Name",     "\"" + d_masterchannel + "\"");
					map2.Add("Password", d_masterchannelpassword);
				}

				map.Add("MasterChannel",  map2);
				map.Add("IgnoreChannels", (!node.IsNull() && node.ContainsKey("IgnoreChannels")) ? node["IgnoreChannels".ToYamlNode()].ToString() : d_ignorechannels);
				map.Add("IgnoreNames",    (!node.IsNull() && node.ContainsKey("IgnoreNames")) ? node["IgnoreNames".ToYamlNode()].ToString() : d_ignorenames);
				map2 = new YamlMappingNode();

				if(!node.IsNull() && node.ContainsKey("NickServ"))
				{
					var node2 = ((YamlMappingNode)node["NickServ".ToYamlNode()]).Children;
					map2.Add("Enabled",  (!node2.IsNull() && node2.ContainsKey("Enabled")) ? node2["Enabled".ToYamlNode()].ToString() : d_usenickserv.ToString());
					map2.Add("Password", (!node2.IsNull() && node2.ContainsKey("Password")) ? node2["Password".ToYamlNode()].ToString() : d_nickservpassword);
				}
				else
				{
					map2.Add("Enabled",  d_usenickserv.ToString());
					map2.Add("Password", d_nickservpassword);
				}

				map.Add("NickServ", map2);
				map2 = new YamlMappingNode();

				if(!node.IsNull() && node.ContainsKey("HostServ"))
				{
					var node2 = ((YamlMappingNode)node["HostServ".ToYamlNode()]).Children;
					map2.Add("Enabled", (!node2.IsNull() && node2.ContainsKey("Enabled")) ? node2["Enabled".ToYamlNode()].ToString() : d_usehostserv.ToString());
					map2.Add("Vhost",   (!node2.IsNull() && node2.ContainsKey("Vhost")) ? node2["Vhost".ToYamlNode()].ToString() : d_hostservstatus.ToString());
				}
				else
				{
					map2.Add("Enabled", d_usehostserv.ToString());
					map2.Add("Vhost",   d_hostservstatus.ToString());
				}

				map.Add("HostServ", map2);
				map2 = new YamlMappingNode();

				if(!node.IsNull() && node.ContainsKey("Wait"))
				{
					var node2 = ((YamlMappingNode)node["Wait".ToYamlNode()]).Children;
					map2.Add("MessageSending", (!node2.IsNull() && node2.ContainsKey("MessageSending")) ? node2["MessageSending".ToYamlNode()].ToString() : d_messagesending.ToString());
				}
				else
					map2.Add("MessageSending", d_messagesending.ToString());

				map.Add("Wait", map2);
				map2 = new YamlMappingNode();

				if(!node.IsNull() && node.ContainsKey("Command"))
				{
					var node2 = ((YamlMappingNode)node["Command".ToYamlNode()]).Children;
					map2.Add("Prefix", "\"" + ((!node2.IsNull() && node2.ContainsKey("Prefix")) ? node2["Prefix".ToYamlNode()].ToString() : d_commandprefix) + "\"");
				}
				else
					map2.Add("Prefix", "\"" + d_commandprefix + "\"");

				map.Add("Command",     map2);
				map.Add("MessageType", (!node.IsNull() && node.ContainsKey("MessageType")) ? node["MessageType".ToYamlNode()].ToString() : d_messagetype);
			}

			return map;
		}

		private YamlMappingNode CreateMySqlMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			var map = new YamlMappingNode();
			map.Add("Enabled",  (!nodes.IsNull() && nodes.ContainsKey("Enabled")) ? nodes["Enabled".ToYamlNode()].ToString() : d_mysqlenabled.ToString());
			map.Add("Host",     (!nodes.IsNull() && nodes.ContainsKey("Host")) ? nodes["Host".ToYamlNode()].ToString() : d_mysqlhost);
			map.Add("User",     (!nodes.IsNull() && nodes.ContainsKey("User")) ? nodes["User".ToYamlNode()].ToString() : d_mysqluser);
			map.Add("Password", (!nodes.IsNull() && nodes.ContainsKey("Password")) ? nodes["Password".ToYamlNode()].ToString() : d_mysqlpassword);
			map.Add("Database", (!nodes.IsNull() && nodes.ContainsKey("Database")) ? nodes["Database".ToYamlNode()].ToString() : d_mysqldatabase);
			map.Add("Charset",  (!nodes.IsNull() && nodes.ContainsKey("Charset")) ? nodes["Charset".ToYamlNode()].ToString() : d_mysqlcharset);
			return map;
		}

		private YamlMappingNode CreateSQLiteMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			var map = new YamlMappingNode();
			map.Add("Enabled",  (!nodes.IsNull() && nodes.ContainsKey("Enabled")) ? nodes["Enabled".ToYamlNode()].ToString() : d_sqliteenabled.ToString());
			map.Add("FileName", (!nodes.IsNull() && nodes.ContainsKey("FileName")) ? nodes["FileName".ToYamlNode()].ToString() : d_sqlitefilename);
			return map;
		}

		private YamlMappingNode CreateCrashMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			var map = new YamlMappingNode();
			map.Add("Directory", (!nodes.IsNull() && nodes.ContainsKey("Directory")) ? nodes["Directory".ToYamlNode()].ToString() : d_crashdirectory);
			return map;
		}

		private YamlMappingNode CreateLocalizationMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			var map = new YamlMappingNode();
			map.Add("Locale", (!nodes.IsNull() && nodes.ContainsKey("Locale")) ? nodes["Locale".ToYamlNode()].ToString() : d_locale);
			return map;
		}

		private YamlMappingNode CreateShutdownMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			var map = new YamlMappingNode();
			map.Add("MaxMemory", (!nodes.IsNull() && nodes.ContainsKey("MaxMemory")) ? nodes["MaxMemory".ToYamlNode()].ToString() : d_shutdownmaxmemory.ToString());
			return map;
		}

		private YamlMappingNode CreateCleanMap(IDictionary<YamlNode, YamlNode> nodes)
		{
			var map = new YamlMappingNode();
			map.Add("Config",   (!nodes.IsNull() && nodes.ContainsKey("Config")) ? nodes["Config".ToYamlNode()].ToString() : d_cleanconfig.ToString());
			map.Add("Database", (!nodes.IsNull() && nodes.ContainsKey("Database")) ? nodes["Database".ToYamlNode()].ToString() : d_cleandatabase.ToString());
			return map;
		}
	}
}