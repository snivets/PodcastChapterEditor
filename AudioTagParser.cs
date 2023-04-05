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

		public List<string> GetFriendlyTimestamps() {
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
	}
}
