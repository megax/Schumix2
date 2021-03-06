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
using System.Threading;
using System.Runtime.InteropServices;
using Schumix.Framework;
using Schumix.Framework.Client;

namespace Schumix.Server
{
	class Windows
	{
		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
		private delegate bool EventHandler(CtrlType sig);
		private EventHandler _handler;
		private Windows() {}

		public void Init()
		{
			_handler += new EventHandler(Handler);
			SetConsoleCtrlHandler(_handler, true);
		}
	
		private bool Handler(CtrlType sig)
		{
			var packet = new SchumixPacket();
			packet.Write<int>((int)Opcode.SMSG_CLOSE_CONNECTION);
			packet.Write<int>((int)0);

			switch(sig)
			{
				case CtrlType.CTRL_C_EVENT:
				case CtrlType.CTRL_BREAK_EVENT:
				case CtrlType.CTRL_CLOSE_EVENT:
					//Log.Notice("Windows", "Daemon killed.");
					MainClass.Shutdown();
					break;
				case CtrlType.CTRL_LOGOFF_EVENT:
				case CtrlType.CTRL_SHUTDOWN_EVENT:
					//Log.Notice("Windows", "User is logging off.");
					MainClass.Shutdown();
					break;
				default:
					break;
			}

			return true;
		}
	}
}