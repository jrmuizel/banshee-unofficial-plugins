
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Unix;
namespace Banshee.Plugins.Wikipedia
{
	
	public abstract class LyricsParser : Parser
	{
		
		public LyricsParser(Stream s) : base(s)
		{
		}
		/*public abstract Page GetPage();
		protected abstract void Parse();*/
	}
	public sealed class RegexLyricsParser : LyricsParser {
		private string body;
		//private bool suggest_list; 
		public RegexLyricsParser(Stream s) : base(s) {
			//this.suggest_list = false;
		}
		
		public override Page GetPage() {
			this.Parse();
			//if ( this.suggest_list ) {
			//	return new ErrorPage("asd",body);
			//} else {
				return new PageBody(body);
			//}
		}
		
		protected override void Parse() {
			Encoding enc = Encoding.GetEncoding("ISO-8859-1");
			string src;
			using ( StreamReader s = new StreamReader(req,enc) ) {
				src = s.ReadToEnd();
			}
			src = Encoding.UTF8.GetString(
			                            Encoding.Convert(enc,Encoding.UTF8,enc.GetBytes(src))); 
			
			Regex artist_regex = new Regex("(?<=<span class=\"TEXTmagenta\">)(\\w|\\s)+(?=</span>)");			
			Match m_artist = artist_regex.Match(src);
			//this.suggest_list = !m_artist.Success;
			if ( m_artist.Success ) {
				Regex lyrics_regex = new Regex("(?<=<td align=\"center\" class=\"TEXT\">)([^<]|<br( /)?>|<a )+");
				Regex song_regex   = new Regex("(?<=<span  class=\"TEXTabc\">)(\\w|\\s)+(?=</span>)");
				Match m_song = song_regex.Match(src);
				this.body += "<h1>"+m_artist.Value;
				if ( m_song.Success ) {
					this.body += ": "+m_song.Value;
				}
				this.body += "</h1>";
				Match m_lyrics = lyrics_regex.Match(src);
				this.body += m_lyrics.Value;
			} else {
				Regex regex        = new Regex("(?<=<td align=\"center\" class=\"TEXT\">)([^<]|<(/)?(br|a|font)[^>]*>)+");
				this.body += "<h1>"+Catalog.GetString("Requested lyric was not found")+"</h1>";
				Match m = regex.Match(src);
				string res = m.Value;
				res = res.Replace(" Suggestions :","<h2>"+Catalog.GetString("Here are some suggestions")+"</h2>");
				res = Regex.Replace(res, "<br><br> If none is your song <br><br><a [^>]*><font >Add a lyric</font></a><br><br>Correct : <br>","");
				this.body += res;
				
			}
			
			
			
		}
		
	}
	
	
}
