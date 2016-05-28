﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SrcSrv.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace GitLink.Pdb
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Catel;

	public static class SrcSrv
	{
		private static string CreateTarget(string rawUrl, string revision)
		{
			rawUrl = rawUrl.Replace("https", "http");
			return string.Format(rawUrl, revision);
		}

		public static byte[] Create(string rawUrl, string revision, IEnumerable<Tuple<string, string>> paths, bool downloadWithPowershell)
		{
			Argument.IsNotNullOrWhitespace(() => rawUrl);
			Argument.IsNotNullOrWhitespace(() => revision);

			using (var ms = new MemoryStream())
			{
				using (var sw = new StreamWriter(ms))
				{
					var scheme = new Uri(rawUrl).Scheme;

					sw.WriteLine("SRCSRV: ini ------------------------------------------------");
					sw.WriteLine("VERSION=2");
					sw.WriteLine("SRCSRV: variables ------------------------------------------");
					sw.WriteLine("RAWURL={0}", CreateTarget(rawUrl, revision));
					if (downloadWithPowershell)
					{
						sw.WriteLine("TRGFILE=%fnbksl%(%targ%%var2%)");
						sw.WriteLine("SRCSRVTRG=%TRGFILE%");
						//sw.WriteLine("SRCSRVCMD=powershell -NoProfile -Command \"(New-Object System.Net.WebClient).DownloadFile('%RAWURL%', '%TRGFILE%')\"");
						sw.Write("SRCSRVCMD=powershell invoke-command -scriptblock {param($url='%RAWURL%', $output='%TRGFILE%'); (New-Object System.Net.WebClient).DownloadFile($url, $output)}");
					}
					else
					{
						sw.WriteLine("SRCSRVVERCTRL={0}", scheme.Replace("https", "http"));
						sw.WriteLine("SRCSRVTRG=%RAWURL%");
					}
					sw.WriteLine("SRCSRV: source files ---------------------------------------");

					foreach (var tuple in paths)
					{
						var instrumentedFile = InstrumentFilePath(tuple.Item2);
						sw.WriteLine("{0}*{1}", tuple.Item1, instrumentedFile);
					}

					sw.WriteLine("SRCSRV: end ------------------------------------------------");

					sw.Flush();

					return ms.ToArray();
				}
			}
		}


		private static object InstrumentFilePath(string filePath)
		{
			return filePath.Replace("/", "__slash__");
		}
	}
}