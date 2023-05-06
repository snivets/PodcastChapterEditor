using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ATL;
using Windows.Storage;

namespace ChapEdit
{
	public class ChapterEditorAudioLayer {
		private readonly StorageFile audioFile;
		private Track audioTrack;

		public int Duration => audioTrack.Duration;

		public ChapterEditorAudioLayer(StorageFile audioFile) {
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

		public void UpdateAlbumArt(byte[] newImage) {
			PictureInfo newPic = PictureInfo.fromBinaryData(newImage, PictureInfo.PIC_TYPE.Front);
			audioTrack.EmbeddedPictures.Insert(0, newPic);
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
			var info = $"Duration: {TimeSpan.FromSeconds(Duration).ToString():hh:mm:ss} / Metadata format(s): {metadataFormats}";
			if (audioTrack.AdditionalFields.Count > 0)
				info += $" / Additional fields: {additionalFieldsStr}";
			if (audioTrack.ChaptersTableDescription.Length > 0)
				info += $" / Chapters table description: {audioTrack.ChaptersTableDescription}";

			return info;
		}

		public void UpdateTimestampsFromFormattedStrings(FormattedAudioChapter[] fChapters) {
			var chapters = new List<ChapterInfo>();
			foreach (FormattedAudioChapter c in fChapters) {
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				var parsedTimestamp = TimeSpan.Parse(c.Timestamp);
				var timeInMillis = parsedTimestamp.TotalMilliseconds;
				ChapterInfo thisChapter = new ChapterInfo(title: c.Title, startTime: (UInt32)timeInMillis);
				chapters.Add(thisChapter);
			}
			this.audioTrack.Chapters = chapters.ToArray();
		}

		public static string FormatChapterTimeFromMillis(UInt32 millis) {
			var chapterStart = TimeSpan.FromMilliseconds(millis);
			return $"{chapterStart.Hours:00}:{chapterStart.Minutes:00}:{chapterStart.Seconds:00}.{chapterStart.Milliseconds:000}";
		}

		public static UInt32 GetMillisFromFriendlyString(string timeStr) {
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			var timespan = TimeSpan.Parse(timeStr);
			var millis = timespan.TotalMilliseconds;
			return (uint)millis;
		}

		/// <summary>
		/// Takes any ole' timestamp formatted string and desperately tries to convert it to the podcast standard.
		/// </summary>
		/// <returns></returns>
		public static TimestampFormatResult GetTimestampString(string timeStr) {
			TimeSpan timespan;
			string ReturnTimestampString = timeStr;
			bool IsSuccessful = false;

			if (TimeSpan.TryParseExact(timeStr, @"h\:mm\:ss\.fff", CultureInfo.InvariantCulture, out timespan) ||
				TimeSpan.TryParseExact(timeStr, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture, out timespan) ||
				TimeSpan.TryParseExact(timeStr, @"mm\:ss\.fff", CultureInfo.InvariantCulture, out timespan) ||
				TimeSpan.TryParseExact(timeStr, @"hhmmss\.fff", CultureInfo.InvariantCulture, out timespan) ||
				TimeSpan.TryParseExact(timeStr, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out timespan) ||
				TimeSpan.TryParseExact(timeStr, @"mm\:ss", CultureInfo.InvariantCulture, out timespan) ||
				TimeSpan.TryParseExact(timeStr, @"m\:ss", CultureInfo.InvariantCulture, out timespan) ||
				TimeSpan.TryParseExact(timeStr, @"mmss", CultureInfo.InvariantCulture, out timespan) ||
				TimeSpan.TryParseExact(timeStr, @"hmmss", CultureInfo.InvariantCulture, out timespan) ||
				TimeSpan.TryParseExact(timeStr, @"hhmmss", CultureInfo.InvariantCulture, out timespan)) {
				IsSuccessful = true;
				ReturnTimestampString = $"{timespan:hh\\:mm\\:ss\\.fff}";
			}

			return new TimestampFormatResult() {
				TimestampResult = ReturnTimestampString,
				SuccessfullyParsed = IsSuccessful
			};
		}
	}

	/// <summary>
	/// A return object that contains a formatted timestamp and an indicator of if it was successful.
	/// </summary>
	public class TimestampFormatResult
	{
		public string TimestampResult { get; set; }
		public bool SuccessfullyParsed { get; set; }
	}
}
