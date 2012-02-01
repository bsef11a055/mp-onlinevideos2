﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OnlineVideos
{
    /// <summary>
    /// These will be loaded with the language files content
    /// if the selected lang file is not found, it will first try to load en(us).xml as a backup
    /// if that also fails it will use the hardcoded strings as a last resort.
    /// </summary>
	public class Translation : CrossDomanSingletonBase<Translation>
    {
		private Translation() {}

		Dictionary<string, string> _translations;
		/// <summary>
		/// Gets the translated strings collection in the active language
		/// </summary>
		public Dictionary<string, string> Strings
		{
			get
			{
				if (_translations == null)
				{
					_translations = new Dictionary<string, string>();
					Type transType = typeof(Translation);
					FieldInfo[] fields = transType.GetFields(BindingFlags.Public | BindingFlags.Instance);
					foreach (FieldInfo field in fields)
					{
						_translations.Add(field.Name, field.GetValue(this).ToString());
					}
				}
				return _translations;
			}
		}

		public string GetByName(string name)
		{
			string result = name;
			Strings.TryGetValue(name, out result);
			return result;
		}

		public string GetByName(string name, params object[] args)
		{
			return String.Format(GetByName(name), args);
		}

		/// <summary>
		/// Takes an input string and replaces all ${named} variables with the proper translation if available
		/// </summary>
		/// <param name="input">a string containing ${named} variables that represent the translation keys</param>
		/// <returns>translated input string</returns>
		public string ParseString(string input)
		{
			Regex replacements = new Regex(@"\$\{([^\}]+)\}");
			MatchCollection matches = replacements.Matches(input);
			foreach (Match match in matches)
			{
				input = input.Replace(match.Value, GetByName(match.Groups[1].Value));
			}
			return input;
		}

        // A
        public string AddingToFavorites = "adding to favorites";
        public string Airdate = "Aired";
        public string AlreadyDownloading = "Already downloading this file.";
        public string All = "All";
        public string Actions = "Actions";
        public string Actors = "Actors";
        public string AddToMySites = "Add to my sites";
        public string AddToFavourites = "Add to favourites";
        public string AutomaticUpdate = "Automatic Update";
        public string AutomaticUpdateDisabled = "Automatic Update disabled.";

        // B
        public string Broken = "Broken";
        public string Buffered = "buffered";

        // C
        public string Cancel = "Cancel";
        public string Category = "Category";
        public string Categories = "Categories";
        public string Concurrent = "concurrent";
        public string Creator = "Creator";
        public string CheckingForPluginUpdate = "checking for updated plugin version";

        // D
        public string Date = "Date";
        public string DateOfRelease = "Date of Release";
        public string Directors = "Directors";
        public string Download = "Download";
        public string DownloadUserdefined = "Download to User Directory";
        public string Downloading = "Downloading";
        public string DownloadingSubtitle = "DownloadingSubtitle";
        public string DownloadingDescription = "Shows a list of downloads currently running.";
        public string DownloadFailed = "Download failed: {0}";
        public string DownloadCancelled = "Download Cancelled";
        public string DownloadComplete = "Download Complete";
        public string DownloadStarted = "Download Started";
        public string DownloadedVideos = "Downloaded Videos";
		public string DownloadsWillBeAborted = "Downloads will be aborted.";
        public string Delete = "Delete";
        public string DeleteAll = "Delete All";
        public string Default = "Default";
        public string Done = "Done";
        public string DeletingOldThumbs = "Deleting old thumbnails";

        // E
        public string Error = "Error";
        public string EnterPin = "Enter Pin";

        // F
        public string Favourites = "Favorites";
        public string Filter = "Filter";
        public string FullUpdate = "Full Update";

        // G
        public string Genre = "Genre";
        public string GettingVideoDetails = "getting video details";
        public string GettingCategoryVideos = "getting category videos";
        public string GettingFavoriteVideos = "getting favorite videos";
        public string GettingSearchResults = "getting search results";
        public string GettingRelatedVideos = "getting related videos";
        public string GettingFilteredVideos = "getting filtered videos";
        public string GettingNextPageVideos = "getting next page videos";
        public string GettingPreviousPageVideos = "getting previous page videos";
        public string GettingPlaybackUrlsForVideo = "getting playback urls for video";
        public string GettingDynamicCategories = "getting dynamic categories";
        public string GettingReports = "getting reports from webservice";
        public string GettingSiteXml = "getting site xml from webservice";
        public string GettingIcon = "getting site icon from webservice";
        public string GettingBanner = "getting site banner from webservice";
        public string GettingDll = "getting dll from webservice";
        public string Groups = "Groups";

        // H

        // I

        // L
        public string LatestVersionRequired = "Latest version ({0}) required!";
        public string Language = "Language";
        public string LayoutList = "Layout: List";
        public string LayoutIcons = "Layout: Icons";
        public string LayoutBigIcons = "Layout: Big Icons";

        // M
        public string ManageSites = "Manage Sites";
        public string MaxResults = "Max Results";

        // N
        public string Name = "Name";
        public string NextPage = "Next page";
        public string NewVideos = "New Videos";
        public string None = "None";
        public string NoVideoFound = "No video found";
        public string NoReportsForSite = "No reports for this site available.";
        public string NewDllDownloaded = "New dll downloaded!";
        public string NewSearch = "New search";

        // O
		public string OnlyLocal = "Only Local";
		public string OnlyServer = "Only Server";
		public string Others = "Others";

        // P
        public string PlayAll = "Play all";
		public string PlayAllRandom = "Play all (random order)";
        public string PlayWith = "Play with ...";
		public string Playlists = "Playlists";
		public string PlaybackWillBeStopped = "Playback will be stopped.";
        public string PleaseEnterDescription = "Please give a short description.";
        public string PlotOutline = "Plot outline";
        public string PreviousPage = "Previous page";
        public string PerformAutomaticUpdate = "Perform automatic update?";

        //Q
        public string Queued = "queued";

        // R
		public string Recommendations = "Recommendations";
        public string RemoveFromFavorites = "Remove from favorites";
        public string RemovingFromFavorites = "removing from favorites";
        public string Refresh = "Refresh";
        public string RelatedVideos = "Related Videos";
        public string ReportBroken = "Report Broken";
        public string Reported = "Reported";
        public string Reports = "Reports";
        public string Runtime = "Runtime";
        public string RemoveFromMySites = "Remove from my sites";
		public string RemoveAllFromMySites = "Remove all from my sites";
        public string RestartMediaPortal = "Restart MediaPortal!";
        public string RetrievingRemoteDlls = "Retrieving remote dlls ...";
        public string RetrievingRemoteSites = "Retrieving remote sites ...";

        // S
        public string SearchResults = "Search results";
        public string SelectSource = "Select source";
        public string Sites = "Sites";
        public string Size = "Size";
        public string SortOptions = "Sort options";
        public string Search = "Search";
        public string SetDownloadFolderInConfig = "Please set a download folder in Configuration!";
        public string State = "State";
        public string SavingLocalSiteList = "Saving local site list ...";
        public string ShowReports = "Show creator's and user's reports";
        public string StartingPlayback = "starting playback";
        public string Subscriptions = "Subscriptions";
        public string SearchHistory = "Search history";
        public string Success = "Success";

        // T
        public string Timeframe = "Timeframe";
        public string Timeout = "Timeout";
        public string Tags = "Tags";

        // U
        public string Updated = "Updated";
        public string Updatable = "Updatable";
        public string UpdateAll = "Update all";
        public string UpdateAllSkipCategories = "Update all (skip categories)";
        public string UpdateMySite = "Update my site";
        public string UpdateMySiteSkipCategories = "Update my site (skip categories)";
        public string UpdateAllYourSites = "This will update all your current sites.";
        public string UnableToPlayVideo = "Unable to play the video. No URL.";
        public string UnableToDownloadVideo = "Unable to download the video. Invalid URL.";
        public string UploadsBy = "uploads by";

        // V
        public string Videos = "Videos";
		public string VideoQuality = "Video Quality";

        // W
        public string Working = "Working";
        public string Writers = "Writers";

        // Y

        // Z

		// Strings from the settings file
		public string Settings_Yes = "Yes";
		public string Settings_No = "No";
		public string Settings_SearchHistoryType_Off = "Off";
		public string Settings_SearchHistoryType_Simple = "Simple";
		public string Settings_SearchHistoryType_Extended = "Extended";
		public string Settings_BasicHomeName = "Basic Home Name";
		public string Settings_PluginEnabled = "Plugin Enabled";
		public string Settings_ListedInHome = "Listed in Home";
		public string Settings_ListedInPlugins = "Listed in My Plugins";
		public string Settings_AutoGroupByLang = "Automatically group all sites by their language";
		public string Settings_UseQuickSelect = "Use alphanumeric keys to quickly select items by first letter";
		public string Settings_SearchHistoryType = "Search history";
		public string Settings_ThumbAge = "Maximum age (days) for thumbnails";
		public string Settings_CacheTimeout = "Time (minutes) to cache data from the web";
		public string Settings_CategoryDiscoveryTimeout = "Time (minutes) after which categories are reloaded from the web";
		public string Settings_UtilTimeout = "Timeout (seconds) for webrequests";
		public string Settings_WmpBuffer = "Buffer (milliseconds) for Windows Media Player";
		public string Settings_PlayBuffer = "Percentage to buffer before starting playback";
		public string Settings_FavoritesFirst = "Put Favorites and Downloads sites first in the list instead of last";
		public string Settings_LatestVideosRandomize = "Randomize all latest videos before displaying";
		public string Settings_LatestVideosOnlineDataRefresh = "Refresh latest videos data from web every x minutes";
		public string Settings_LatestVideosGuiDataRefresh = "Refresh displayed latest videos every x seconds";
    }
}