using System;

namespace Banshee.Plugins.Wikipedia
{
	
	public abstract class PageGtkHeader : PageHeader
	{
		
		public PageGtkHeader(Page p) : base(p)
		{
		}
		public PageGtkHeader()
		{
			Gtk.Style s 		= Gtk.Rc.GetStyle(new Gtk.Button());
			Gdk.Color bg 		= s.Base(Gtk.StateType.Normal);
			Gdk.Color e_bg 		= s.Background(Gtk.StateType.Normal);
			Gdk.Color light 	= s.Base(Gtk.StateType.Selected);
			Gdk.Color text  	= s.Text(Gtk.StateType.Normal);
			Gdk.Color dark  	= s.Dark(Gtk.StateType.Selected);
			Gdk.Color dark_text	= s.Text(Gtk.StateType.Selected);
			
			//Background image for error div
			// crashes
//			string bg_image;
//			try {
//				string name       = Gtk.Stock.DialogError;
//				Gtk.IconInfo info = Gtk.IconTheme.Default.LookupIcon(name,256,Gtk.IconLookupFlags.ForceSvg);
//				bg_image  = info.Filename;
//			} catch (Exception e) {
//				Console.WriteLine(e.Message);
//				bg_image= "";
//			}
			
			this.content = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">"+
			"<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\" dir=\"ltr\">"+
			"<head>"+
			"<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />"+
//			"<style type=\"text/css\" media=\"screen,projection\">/*<![CDATA[*/ @import \"/skins-1.5/monobook/main.css?7\"; /*]]>*/</style>"+
//			"<link rel=\"stylesheet\" type=\"text/css\" media=\"print\" href=\"/skins-1.5/common/commonPrint.css\" />"+
//			"<meta http-equiv=\"imagetoolbar\" content=\"no\" /><![endif]-->"+
//			"<script type=\"text/javascript\">var skin = 'monobook';var stylepath = '/skins-1.5';</script>"+
//			"<script type=\"text/javascript\" src=\"/skins-1.5/common/wikibits.js?1\"><!-- wikibits js --></script>"+
//			"<script type=\"text/javascript\" src=\"/w/index.php?title=-&amp;action=raw&amp;gen=js\"><!-- site js --></script>"+
//			"<style type=\"text/css\">/*<![CDATA[*/"+
//			"@import \"/w/index.php?title=MediaWiki:Common.css&action=raw&ctype=text/css&smaxage=2678400\";"+
//			"@import \"/w/index.php?title=MediaWiki:Monobook.css&action=raw&ctype=text/css&smaxage=2678400\";"+
//			"@import \"/w/index.php?title=-&action=raw&gen=css&maxage=2678400\";"+
//			"/*]]>*/</style>"+
			"<style>"+
			"body { "+
			"margin:10px;"+
			"border:0px;"+
			"background-color: rgb("+(bg.Red>>8)+","+(bg.Green>>8)+","+(bg.Blue>>8)+");"+
			"color: rgb("+(text.Red>>8)+","+(text.Green>>8)+","+(text.Blue>>8)+");"+
			"font-size:x-small;"+
			"font:Verdana,Sans-serif;"+
			"}"+
			"h1 { "+
			"	padding:5px;"+
			"	background-color: rgb("+(light.Red>>8)+","+(light.Green>>8)+","+(light.Blue>>8)+"); "+
			"	-moz-border-radius:5px;"+
			"	}"+
			"#title {"+
			"background-color: rgb("+(dark.Red>>8)+","+(dark.Green>>8)+","+(dark.Blue>>8)+");"+
			"color: rgb("+(dark_text.Red>>8)+","+(dark_text.Green>>8)+","+(dark_text.Blue>>8)+"); }"+
			"img a { boder:none;}"+
			"a {color: rgb("+(dark.Red>>8)+","+(dark.Green>>8)+","+(dark.Blue>>8)+"); }"+
			"#error { background-color:rgb(255,0,0); color(255,255,255);}"+
			".e_msg {"+
			"	border:1px solid red;font-size:medium; width:60%;margin:auto;"+
			"	background-color: rgb("+(e_bg.Red>>8)+","+(e_bg.Green>>8)+","+(e_bg.Blue>>8)+");"+
			"	padding:40px;"+
			"	-moz-border-radius:15px;"+
//			"	background-image: url(\"file://"+bg_image+"\"); "+
//			"	background-repeat: no-repeat; "+
//			" 	background-position: top left;"+
			"}"+
			"</style></head><body>";
//			"<img src=\""+bg_image+"\" /><br />"+
//			"<img src=\"file://"+bg_image+"\" />";
//			Array st = Enum.GetValues(typeof(Gtk.StateType));
//			foreach ( Gtk.StateType t in st ) {
//				bg = s.Background(t);
//				Gdk.Color ba = s.Base(t);
//				dark = s.Dark(t);
//				Gdk.Color fg = s.Foreground(t);
//				light = s.Light(t);
//				Gdk.Color mid = s.Mid(t);
//				text = s.Text(t);
//				this.content += string.Format("<h1>{0}</h1>",t);
//				this.content += string.Format("<div style=\"background-color: rgb("+(bg.Red>>8)+","+(bg.Green>>8)+","+(bg.Blue>>8)+");\">{0}</div><br />","Backgroung");
//				this.content += string.Format("<div style=\"background-color: rgb("+(ba.Red>>8)+","+(ba.Green>>8)+","+(ba.Blue>>8)+");\">{0}</div><br />","Base");
//				this.content += string.Format("<div style=\"background-color: rgb("+(dark.Red>>8)+","+(dark.Green>>8)+","+(dark.Blue>>8)+");\">{0}</div><br />","Dark");
//				this.content += string.Format("<div style=\"background-color: rgb("+(fg.Red>>8)+","+(fg.Green>>8)+","+(fg.Blue>>8)+");\">{0}</div><br />","Foreground");
//				this.content += string.Format("<div style=\"background-color: rgb("+(light.Red>>8)+","+(light.Green>>8)+","+(light.Blue>>8)+");\">{0}</div><br />","Light");
//				this.content += string.Format("<div style=\"background-color: rgb("+(mid.Red>>8)+","+(mid.Green>>8)+","+(mid.Blue>>8)+");\">{0}</div><br />","Mid");
//				this.content += string.Format("<div style=\"background-color: rgb("+(text.Red>>8)+","+(text.Green>>8)+","+(text.Blue>>8)+");\">{0}</div><br />","Text");
//				this.content += string.Format("<br /><br />");
//			}
		}
	}
	
}
