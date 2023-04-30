using ATL;

namespace ChapEdit
{
	public class FormattedAudioChapter
	{
		public string Title { get; set; }
		public string Timestamp { get; set; }
		public int Index { get; set; }

		public FormattedAudioChapter() {
			this.Title = string.Empty;
			this.Timestamp = "00:00:00.000";
			this.Index = 0;
		}

		public FormattedAudioChapter(string title, string timestamp) {
			this.Title = title;
			this.Timestamp = timestamp;
			this.Index = 0;
		}

		public FormattedAudioChapter(string title, string timestamp, int index) : this(title, timestamp) {
			this.Index = index;
		}

		/// <summary>
		/// Returns this FormattedAudioChapter object as ATL's ChapterInfo format.
		/// </summary>
		public ChapterInfo GetChapterInfo() {
			return new ChapterInfo(title: this.Title, startTime: AudioTagParser.GetMillisFromFriendlyString(this.Timestamp));
		}
	}
}
