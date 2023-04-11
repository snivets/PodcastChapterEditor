﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ATL;
using Windows.Storage;

namespace ChapEdit
{
	public class AudioTagParser
	{
		private readonly StorageFile audioFile;
		private Track audioTrack;

		public AudioTagParser(StorageFile audioFile) {
			this.audioFile = audioFile;
			this.audioTrack = new Track(this.audioFile.Path);
		}

		public IList<ChapterInfo> GetChapters() {
			return audioTrack.Chapters;
		}

		public void UpdateChapters(ChapterInfo[] newChapters) {
			audioTrack.Chapters = newChapters;
			audioTrack.SaveAsync();
		}

		public byte[] GetAlbumArt() {
			if (audioTrack.EmbeddedPictures.Any(p => p.PicType == PictureInfo.PIC_TYPE.Front))
				return audioTrack.EmbeddedPictures.First(p => p.PicType == PictureInfo.PIC_TYPE.Front).PictureData;

			return null;
		}

		public string GetFileInfo() {
			var additionalFieldsStr = string.Empty;
			audioTrack.AdditionalFields.ToList().ForEach(af => additionalFieldsStr += af.Key + ": '" + af.Value + "', ");
			var metadataFormats = string.Empty;
			audioTrack.MetadataFormats.ToList().ForEach(mf => metadataFormats += mf.Name + ", ");
			if (metadataFormats.Length > 0)
				metadataFormats = metadataFormats.Trim().TrimEnd(',');
			if (additionalFieldsStr.Length > 0)
				additionalFieldsStr = additionalFieldsStr.Trim().TrimEnd(',');
			var info = $"Metadata format(s): {metadataFormats}";
			if (audioTrack.ChaptersTableDescription.Length > 0)
				info += $" / Chapters table description: {audioTrack.ChaptersTableDescription}";
			if (audioTrack.AdditionalFields.Count > 0)
				info += $" / Additional fields: {additionalFieldsStr}";

			return info;
		}

		public void UpdateTimestampsFromFormattedStrings(FormattedAudioChapter[] fChapters) {
			var chapters = new List<ChapterInfo>();
			foreach(FormattedAudioChapter c in fChapters) {
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				var parsedTimestamp = TimeSpan.Parse(c.Timestamp);
				var timeInMillis = parsedTimestamp.TotalMilliseconds;
				ChapterInfo thisChapter = new ChapterInfo(title: c.Title, startTime: (UInt32)timeInMillis);
				chapters.Add(thisChapter);
			}
			this.audioTrack.Chapters = chapters.ToArray();
		}

		public static string FormatChapterTime(UInt32 millis) {
			var chapterStart = TimeSpan.FromMilliseconds(millis);
			return $"{chapterStart.Hours:00}:{chapterStart.Minutes:00}:{chapterStart.Seconds:00}.{chapterStart.Milliseconds:00}";
		}

		public static UInt32 GetMillisFromFriendlyString(string timeStr) {
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			var timespan = TimeSpan.Parse(timeStr);
			var millis = timespan.TotalMilliseconds;
			return (uint)millis;
		}
	}

	public class FormattedAudioChapter {
		public string Title { get; set; }
		public string Timestamp { get; set; }

		public FormattedAudioChapter() {
			this.Title = string.Empty;
			this.Timestamp = "00:00:00.00";
		}

		public FormattedAudioChapter(string title, string timestamp) {
			this.Title = title;
			this.Timestamp = timestamp;
		}

		/// <summary>
		/// Returns this FOrmattedAudioChapter object as ATL's ChapterInfo format.
		/// </summary>
		public ChapterInfo GetChapterInfo() {
			return new ChapterInfo(title: this.Title, startTime: AudioTagParser.GetMillisFromFriendlyString(this.Timestamp));
		}
	}
}
