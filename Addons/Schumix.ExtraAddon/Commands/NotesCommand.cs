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
using System.Data;
using Schumix.API;
using Schumix.Irc;
using Schumix.Irc.Commands;
using Schumix.Framework;
using Schumix.Framework.Config;
using Schumix.Framework.Extensions;
using Schumix.Framework.Localization;

namespace Schumix.ExtraAddon.Commands
{
	sealed class Notes : CommandInfo
	{
		private readonly LocalizationConsole sLConsole = Singleton<LocalizationConsole>.Instance;
		private readonly LocalizationManager sLManager = Singleton<LocalizationManager>.Instance;
		private readonly SendMessage sSendMessage = Singleton<SendMessage>.Instance;
		private readonly Utilities sUtilities = Singleton<Utilities>.Instance;
		private Notes() {}

		public void HandleNotes(IRCMessage sIRCMessage)
		{
			if(sIRCMessage.Info.Length >= 6 && sIRCMessage.Info[4].ToLower() == "user" && sIRCMessage.Info[5].ToLower() == "register")
			{
			}
			else
			{
				if(!Warning(sIRCMessage))
					return;
			}

			if(sIRCMessage.Info.Length < 5)
			{
				sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoValue", sIRCMessage.Channel));
				return;
			}

			if(sIRCMessage.Info[4].ToLower() == "info")
			{
				if(!IsUser(sIRCMessage.Nick, sIRCMessage.Host))
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoDataNoCommand", sIRCMessage.Channel));
					return;
				}

				var db = SchumixBase.DManager.Query("SELECT Code FROM notes WHERE Name = '{0}'", sIRCMessage.Nick.ToLower());
				if(!db.IsNull())
				{
					string codes = string.Empty;

					foreach(DataRow row in db.Rows)
					{
						string code = row["Code"].ToString();
						codes += ", " + code;
					}

					if(codes == string.Empty)
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetCommandText("notes/info", sIRCMessage.Channel), sLConsole.Other("Nothing"));
					else
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetCommandText("notes/info", sIRCMessage.Channel), codes.Remove(0, 2, ", "));
				}
				else
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("FaultyQuery", sIRCMessage.Channel));
			}
			else if(sIRCMessage.Info[4].ToLower() == "user")
			{
				if(sIRCMessage.Info.Length < 6)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoValue", sIRCMessage.Channel));
					return;
				}

				if(sIRCMessage.Info[5].ToLower() == "access")
				{
					var text = sLManager.GetCommandTexts("notes/user/access", sIRCMessage.Channel);
					if(text.Length < 2)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel)));
						return;
					}

					if(sIRCMessage.Info.Length < 7)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoPassword", sIRCMessage.Channel));
						return;
					}

					string name = sIRCMessage.Nick;
					var db = SchumixBase.DManager.QueryFirstRow("SELECT Password FROM notes_users WHERE Name = '{0}'", name.ToLower());
					if(!db.IsNull())
					{
						if(db["Password"].ToString() == sUtilities.Sha1(sIRCMessage.Info[6]))
						{
							SchumixBase.DManager.Update("notes_users", string.Format("Vhost = '{0}'", sIRCMessage.Host), string.Format("Name = '{0}'", name.ToLower()));
							sSendMessage.SendChatMessage(sIRCMessage, text[0]);
						}
						else
							sSendMessage.SendChatMessage(sIRCMessage, text[1]);
					}
				}
				else if(sIRCMessage.Info[5].ToLower() == "newpassword")
				{
					var text = sLManager.GetCommandTexts("notes/user/newpassword", sIRCMessage.Channel);
					if(text.Length < 2)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel)));
						return;
					}

					if(sIRCMessage.Info.Length < 7)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoOldPassword", sIRCMessage.Channel));
						return;
					}

					if(sIRCMessage.Info.Length < 8)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoNewPassword", sIRCMessage.Channel));
						return;
					}

					string name = sIRCMessage.Nick;
					var db = SchumixBase.DManager.QueryFirstRow("SELECT Password FROM notes_users WHERE Name = '{0}'", name.ToLower());
					if(!db.IsNull())
					{
						if(db["Password"].ToString() == sUtilities.Sha1(sIRCMessage.Info[6]))
						{
							SchumixBase.DManager.Update("notes_users", string.Format("Password = '{0}'", sUtilities.Sha1(sIRCMessage.Info[7])), string.Format("Name = '{0}'", name.ToLower()));
							sSendMessage.SendChatMessage(sIRCMessage, text[0], sIRCMessage.Info[7]);
						}
						else
							sSendMessage.SendChatMessage(sIRCMessage, text[1]);
					}
				}
				else if(sIRCMessage.Info[5].ToLower() == "register")
				{
					var text = sLManager.GetCommandTexts("notes/user/register", sIRCMessage.Channel);
					if(text.Length < 2)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel)));
						return;
					}

					if(sIRCMessage.Info.Length < 7)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoPassword", sIRCMessage.Channel));
						return;
					}

					string name = sIRCMessage.Nick;
					var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM notes_users WHERE Name = '{0}'", name.ToLower());
					if(!db.IsNull())
					{
						sSendMessage.SendChatMessage(sIRCMessage, text[0]);
						return;
					}

					string pass = sIRCMessage.Info[6];
					SchumixBase.DManager.Insert("`notes_users`(Name, Password, Vhost)", name.ToLower(), sUtilities.Sha1(pass), sIRCMessage.Host);
					sSendMessage.SendChatMessage(sIRCMessage, text[1]);
				}
				else if(sIRCMessage.Info[5].ToLower() == "remove")
				{
					var text = sLManager.GetCommandTexts("notes/user/remove", sIRCMessage.Channel);
					if(text.Length < 5)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel)));
						return;
					}

					if(sIRCMessage.Info.Length < 7)
					{
						sSendMessage.SendChatMessage(sIRCMessage, text[0]);
						return;
					}

					string name = sIRCMessage.Nick;
					var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM notes_users WHERE Name = '{0}'", name.ToLower());
					if(db.IsNull())
					{
						sSendMessage.SendChatMessage(sIRCMessage, text[1]);
						return;
					}

					db = SchumixBase.DManager.QueryFirstRow("SELECT Password FROM notes_users WHERE Name = '{0}'", name.ToLower());
					if(!db.IsNull())
					{
						if(db["Password"].ToString() != sUtilities.Sha1(sIRCMessage.Info[6]))
						{
							sSendMessage.SendChatMessage(sIRCMessage, text[2]);
							sSendMessage.SendChatMessage(sIRCMessage, text[3]);
							return;
						}
					}

					SchumixBase.DManager.Delete("notes_users", string.Format("Name = '{0}'", name.ToLower()));
					SchumixBase.DManager.Delete("notes", string.Format("Name = '{0}'", name.ToLower()));
					sSendMessage.SendChatMessage(sIRCMessage, text[4]);
				}
			}
			else if(sIRCMessage.Info[4].ToLower() == "code")
			{
				if(!IsUser(sIRCMessage.Nick, sIRCMessage.Host))
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoDataNoCommand", sIRCMessage.Channel));
					return;
				}

				if(sIRCMessage.Info.Length < 6)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoCode", sIRCMessage.Channel));
					return;
				}

				if(sIRCMessage.Info[5].ToLower() == "remove")
				{
					var text = sLManager.GetCommandTexts("notes/code/remove", sIRCMessage.Channel);
					if(text.Length < 2)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel)));
						return;
					}

					if(sIRCMessage.Info.Length < 7)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoCode", sIRCMessage.Channel));
						return;
					}

					var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM notes WHERE Code = '{0}' AND Name = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[6].ToLower()), sIRCMessage.Nick.ToLower());
					if(db.IsNull())
					{
						sSendMessage.SendChatMessage(sIRCMessage, text[0]);
						return;
					}

					SchumixBase.DManager.Delete("notes", string.Format("Code = '{0}' AND Name = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[6].ToLower()), sIRCMessage.Nick.ToLower()));
					sSendMessage.SendChatMessage(sIRCMessage, text[1]);
				}
				else
				{
					var db = SchumixBase.DManager.QueryFirstRow("SELECT Note FROM notes WHERE Code = '{0}' AND Name = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[5].ToLower()), sIRCMessage.Nick.ToLower());
					if(!db.IsNull())
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetCommandText("notes/code", sIRCMessage.Channel), db["Note"].ToString());
					else
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("FaultyQuery", sIRCMessage.Channel));
				}
			}
			else
			{
				var text = sLManager.GetCommandTexts("notes", sIRCMessage.Channel);
				if(text.Length < 3)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel)));
					return;
				}

				if(!IsUser(sIRCMessage.Nick, sIRCMessage.Host))
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoDataNoCommand", sIRCMessage.Channel));
					return;
				}

				if(sIRCMessage.Info.Length < 6)
				{
					sSendMessage.SendChatMessage(sIRCMessage, text[0]);
					return;
				}

				string code = sIRCMessage.Info[4];
				var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM notes WHERE Code = '{0}' AND Name = '{1}'", sUtilities.SqlEscape(code.ToLower()), sIRCMessage.Nick.ToLower());
				if(!db.IsNull())
				{
					sSendMessage.SendChatMessage(sIRCMessage, text[1]);
					return;
				}

				SchumixBase.DManager.Insert("`notes`(Code, Name, Note)", sUtilities.SqlEscape(code.ToLower()), sIRCMessage.Nick.ToLower(), sUtilities.SqlEscape(sIRCMessage.Info.SplitToString(5, SchumixBase.Space)));
				sSendMessage.SendChatMessage(sIRCMessage, text[2], code);
			}
		}

		private bool IsUser(string Name)
		{
			var db = SchumixBase.DManager.QueryFirstRow("SELECT * FROM notes_users WHERE Name = '{0}'", Name.ToLower());
			return !db.IsNull();
		}

		private bool IsUser(string Name, string Vhost)
		{
			var db = SchumixBase.DManager.QueryFirstRow("SELECT Vhost FROM notes_users WHERE Name = '{0}'", Name.ToLower());
			if(!db.IsNull())
			{
				string vhost = db["Vhost"].ToString();

				if(Vhost != vhost)
					return false;

				return true;
			}

			return false;
		}

		private bool Warning(IRCMessage sIRCMessage)
		{
			if(!IsUser(sIRCMessage.Nick))
			{
				var text = sLManager.GetCommandTexts("notes/warning", sIRCMessage.Channel);
				if(text.Length < 4)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel)));
					return false;
				}

				sSendMessage.SendChatMessage(sIRCMessage, text[0]);
				sSendMessage.SendChatMessage(sIRCMessage, text[1]);
				sSendMessage.SendChatMessage(sIRCMessage, text[2], IRCConfig.CommandPrefix);
				sSendMessage.SendChatMessage(sIRCMessage, text[3], IRCConfig.CommandPrefix);
				return false;
			}

			return true;
		}
	}
}