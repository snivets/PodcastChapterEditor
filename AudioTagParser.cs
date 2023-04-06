using System;
using System.Collections.Generic;
using System.Linq;
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

		/// <summary>
		/// Deprecated, but could be useful for debug at some point.
		/// </summary>
		/// <returns>List of strings representing paragraph objects for a text box</returns>
		public List<string> GetFriendlyChapterSummary() {
			var timeList = new List<string>();
			timeList.Add(audioTrack.ChaptersTableDescription);
			audioTrack.AdditionalFields.ToList().ForEach(af => timeList.Add(af.Key + ":" + af.Value));
			timeList.Add("Codec Family: " + audioTrack.CodecFamily.ToString());
			audioTrack.Chapters.ToList().ForEach(ch => {
				var ts = TimeSpan.FromMilliseconds(ch.StartTime);
				timeList.Add(ts.Hours.ToString().PadLeft(2, '0') + ":"
					+ ts.Minutes.ToString().PadLeft(2, '0') + ":"
					+ ts.Seconds.ToString().PadLeft(2, '0') + "."
					+ ts.Milliseconds.ToString().PadLeft(3, '0')
					+ "   " + ch.Title);
			});
			return timeList;
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
	}
}
