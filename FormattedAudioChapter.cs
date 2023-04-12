using ATL;

namespace ChapEdit
{
	public class FormattedAudioChapter
	{
		public string Title { get; set; }
		public string Timestamp { get; set; }

		public FormattedAudioChapter() {
			this.Title = string.Empty;
			this.Timestamp = "00:00:00.000";
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
