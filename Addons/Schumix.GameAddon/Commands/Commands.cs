﻿/*
 * This file is part of Schumix.
 * 
 * Copyright (C) 2010-2011 Megax <http://www.megaxx.info/>
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
using System.Data;
using Schumix.API;
using Schumix.Irc;
using Schumix.Irc.Commands;
using Schumix.Framework;
using Schumix.Framework.Extensions;
using Schumix.Framework.Localization;
using Schumix.GameAddon.MaffiaGames;

namespace Schumix.GameAddon.Commands
{
	public class GameCommand : CommandInfo
	{
		private readonly LocalizationManager sLManager = Singleton<LocalizationManager>.Instance;
		private readonly SendMessage sSendMessage = Singleton<SendMessage>.Instance;
		private readonly ChannelInfo sChannelInfo = Singleton<ChannelInfo>.Instance;
		private readonly Utilities sUtilities = Singleton<Utilities>.Instance;
		private readonly Sender sSender = Singleton<Sender>.Instance;

		protected void HandleGame(IRCMessage sIRCMessage)
		{
			CNick(sIRCMessage);

			if(sIRCMessage.Info.Length < 5)
			{
				sSendMessage.SendCMPrivmsg(sIRCMessage.Channel, sLManager.GetWarningText("NoValue", sIRCMessage.Channel));
				return;
			}

			if(sIRCMessage.Info[4].ToLower() == "start")
			{
				if(sIRCMessage.Info.Length < 6)
				{
					sSendMessage.SendCMPrivmsg(sIRCMessage.Channel, "Nincs megadva a játék neve!");
					return;
				}

				if(sIRCMessage.Info[5].ToLower() == "maffiagame")
				{
					foreach(var maffia in GameAddon.MaffiaList)
					{
						if(sIRCMessage.Channel.ToLower() != maffia.Key)
						{
							foreach(var player in maffia.Value.GetPlayerList())
							{
								if(player.Value == sIRCMessage.Nick)
								{
									sSendMessage.SendCMPrivmsg(sIRCMessage.Channel, "{0}: Te már játékban vagy itt: {1}", sIRCMessage.Nick, maffia.Key);
									return;
								}
							}
						}
					}

					var db = SchumixBase.DManager.QueryFirstRow("SELECT Functions FROM channel WHERE Channel = '{0}'", sIRCMessage.Channel);
					if(!db.IsNull())
					{
						if(!GameAddon.GameChannelFunction.ContainsKey(sIRCMessage.Channel.ToLower()))
							GameAddon.GameChannelFunction.Add(sIRCMessage.Channel.ToLower(), db["Functions"].ToString());
					}

					SchumixBase.DManager.QueryFirstRow("UPDATE channel SET Functions = '{0}' WHERE Channel = '{1}'", sUtilities.GetFunctionUpdate(), sIRCMessage.Channel);
					sChannelInfo.ChannelFunctionReload();
					SchumixBase.DManager.QueryFirstRow("UPDATE channel SET Functions = '{0}' WHERE Channel = '{1}'", sChannelInfo.ChannelFunctions("commands", "off", sIRCMessage.Channel), sIRCMessage.Channel);
					sChannelInfo.ChannelFunctionReload();
					SchumixBase.DManager.QueryFirstRow("UPDATE channel SET Functions = '{0}' WHERE Channel = '{1}'", sChannelInfo.ChannelFunctions("gamecommands", "on", sIRCMessage.Channel), sIRCMessage.Channel);
					sChannelInfo.ChannelFunctionReload();
					sSender.Mode(sIRCMessage.Channel, "+v", sIRCMessage.Nick);
					if(!GameAddon.MaffiaList.ContainsKey(sIRCMessage.Channel.ToLower()))
						GameAddon.MaffiaList.Add(sIRCMessage.Channel.ToLower(), new MaffiaGame(sIRCMessage.Nick, sIRCMessage.Channel));
					else
						GameAddon.MaffiaList[sIRCMessage.Channel.ToLower()].NewGame(sIRCMessage.Nick, sIRCMessage.Channel);
				}
				else
					sSendMessage.SendCMPrivmsg(sIRCMessage.Channel, "Nincs ilyen játék!");
			}
		}

		protected void HandleNewNick(IRCMessage sIRCMessage)
		{
			foreach(var maffia in GameAddon.MaffiaList)
			{
				if(!maffia.Value.Running)
					continue;

				foreach(var player in maffia.Value.GetPlayerList())
				{
					if(player.Value == sIRCMessage.Nick)
					{
						maffia.Value.NewNick(player.Key, sIRCMessage.Nick, sIRCMessage.Info[2].Remove(0, 1, ":"));
						break;
					}
				}
			}
		}

		protected void HandleQuit(IRCMessage sIRCMessage)
		{
			foreach(var maffia in GameAddon.MaffiaList)
			{
				if(!maffia.Value.Running)
					continue;

				foreach(var player in maffia.Value.GetPlayerList())
				{
					if(player.Value == sIRCMessage.Nick)
					{
						maffia.Value.Leave(sIRCMessage.Nick);
						break;
					}
				}
			}
		}
	}
}