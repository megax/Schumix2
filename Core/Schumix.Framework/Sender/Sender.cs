/*
 * This file is part of Schumix.
 * 
 * Copyright (C) 2010-2012 Megax <http://www.megaxx.info/>
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
using Schumix.Framework;
using Schumix.Framework.Config;
using Schumix.Framework.Extensions;
using Schumix.Framework.Localization;

namespace Schumix.Irc
{
	public sealed class Sender
	{
		private readonly LocalizationConsole sLConsole = Singleton<LocalizationConsole>.Instance;
		private readonly IrcBase sIrcBase = Singleton<IrcBase>.Instance;
		private readonly object WriteLock = new object();
		private readonly SendMessage sSendMessage;

		public Sender(string ServerName)
		{
			sSendMessage = sIrcBase.Networks[ServerName].sSendMessage;
		}

		public void NameInfo(string nick, string user, string userinfo)
		{
			lock(WriteLock)
			{
				Nick(nick);
				User(user, userinfo);
			}
		}

		public void Nick(string nick)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("NICK {0}", nick);
			}
		}

		public void User(string user, string userinfo)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("USER {0} 8 * :{1}", user, userinfo);
			}
		}

		public void Join(string channel)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("JOIN {0}", channel);
			}
		}

		public void Join(string channel, string pass)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("JOIN {0} {1}", channel, pass);
			}
		}

		public void Part(string channel)
		{
			lock(WriteLock)
			{
				Part(channel, sLConsole.Sender("Text"));
			}
		}

		public void Part(string channel, string message)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("PART {0} :{1}", channel, message);
			}
		}

		public void Kick(string channel, string name)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("KICK {0} {1}", channel, name);
			}
		}

		public void Kick(string channel, string name, string args)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("KICK {0} {1} :{2}", channel, name, args);
			}
		}

		public void Ban(string channel, string name)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("MODE {0} +b {1}", channel, name);
			}
		}

		public void Unban(string channel, string name)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("MODE {0} -b {1}", channel, name);
			}
		}

		public void Mode(string channel, string status)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("MODE {0} {1}", channel, status);
			}
		}

		public void Mode(string channel, string status, string name)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("MODE {0} {1} {2}", channel, status, name);
			}
		}

		public void Quit(string args)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("QUIT :{0}", args);
			}
		}

		public void Ping(string ping)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("PING :{0}", ping);
			}
		}

		public void Pong(string pong)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("PONG :{0}", pong);
			}
		}

		public void NickServ(string pass)
		{
			lock(WriteLock)
			{
				sSendMessage.SendCMPrivmsg("NickServ", "IDENTIFY {0}", pass);
			}
		}

		public void NickServStatus(string status)
		{
			lock(WriteLock)
			{
				sSendMessage.SendCMPrivmsg("NickServ", "STATUS {0}", status);
			}
		}

		public void NickServInfo(string info)
		{
			lock(WriteLock)
			{
				sSendMessage.SendCMPrivmsg("NickServ", "INFO {0}", info);
			}
		}

		public void HostServ(string Mode)
		{
			lock(WriteLock)
			{
				sSendMessage.SendCMPrivmsg("HostServ", "{0}", Mode);
			}
		}

		public void NickServGhost(string ghost, string pass)
		{
			lock(WriteLock)
			{
				sSendMessage.SendCMPrivmsg("NickServ", "GHOST {0} {1}", ghost, pass);
			}
		}

		public void Whois(string name)
		{
			lock(WriteLock)
			{
				sSendMessage.WriteLine("WHOIS {0}", name);
			}
		}
	}
}