/*
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
using System.IO;

namespace Schumix.Config.CopyTo
{
	public sealed class Copy
	{
		public Copy(string Version)
		{
			if(Directory.Exists("Addons"))
				Directory.Delete("Addons", true);

			Directory.Move(Version + "/Run/Release/Addons", "Addons");
			var dir = new DirectoryInfo(Version + "/Run/Release/");

			foreach(var file in dir.GetFiles())
			{
				File.Delete(file.Name);
				File.Move(Version + "/Run/Release/" + file.Name, file.Name);
			}

			dir = new DirectoryInfo("Configs");

			foreach(var fi in dir.GetFiles())
			{
				Console.WriteLine(fi.Name);
				File.Move("Configs/" + fi.Name, "Configs/_" + fi.Name);
			}
		}
	}
}