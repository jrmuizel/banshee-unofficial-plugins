
/***************************************************************************
 *  QueryBuilderModel.cs
 *
 *  Copyright (C) 2005 Novell
 *  Written by Aaron Bockover (aaron@aaronbock.net)
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */
 
using System;
using GLib;
using Gtk;
using Sql;
using System.Collections;

using Banshee.Widgets;

namespace Banshee
{
	public sealed class QueryFilterOperation
	{
		public const string Is = "is";
		public const string IsNot = "is not";
		public const string IsLessThan = "is less than";
		public const string IsGreaterThan = "is greater than";
		public const string Contains = "contains";
		public const string DoesNotContain = "does not contain";
		public const string StartsWith = "starts with";
		public const string EndsWith = "ends with";
		public const string IsBefore = "is before";
		public const string IsAfter = "is after";
		public const string IsInTheRange = "is in the range";
	}
	
	public sealed class QuerySelectedByCriteria
	{
		public const string Random = "Random";
		public const string Album = "Album";
		public const string Artist = "Artist";
		public const string Genre = "Genre";
		public const string SongName = "Song Name";
		public const string HighestRating = "Highest Rating";
		public const string LowestRating = "Lowest Rating";
		public const string LeastOftenPlayed = "Least Often Played";
		public const string MostOftenPlayed = "Most Often Played";
		public const string MostRecentlyAdded = "Most Recently Added";
		public const string LeastRecentlyAdded = "Least Recently Added";
	}
	
	public sealed class QueryLimitCriteria
	{
		public const string Songs = "songs";
		public const string Minutes = "minutes";
		public const string Hours = "hours";
	}
	
	// --- Query Match String --- 
	
	public class QueryMatchString : QueryMatch
	{
		private Entry dispEntry;

		public override string FilterValues()
		{
			UpdateValues();
			
			string pv = Statement.EscapeQuotes(Value1);
			
			switch(Filter) {
				case QueryFilterOperation.Is:
					return Column + " = '" + pv + "'";
				case QueryFilterOperation.IsNot:
					return Column + " != '" + pv + "'";
				case QueryFilterOperation.Contains:
					return Column + " LIKE '%" + pv + "%'";
				case QueryFilterOperation.DoesNotContain:
					return Column + " NOT LIKE '%" + pv + "%'";
				case QueryFilterOperation.StartsWith:
					return Column + " LIKE '" + pv + "%'";
				case QueryFilterOperation.EndsWith:
					return Column + " LIKE '%" + pv + "'";
			}
			
			return null;
		}
		
		public override void UpdateValues()
		{
			if(dispEntry == null)
				throw new Exception("Display Widget was never Set");
				
			Value1 = dispEntry.Text;
		}
		
		public override Widget DisplayWidget
		{
			get {
				if(dispEntry == null) {
					dispEntry = new Entry();
					dispEntry.Show();
				}
					
				return dispEntry;
			}
		}
		
		public override string [] ValidOperations
		{
			get {	
				string [] validOperations = {
					QueryFilterOperation.Is,
					QueryFilterOperation.IsNot,
					QueryFilterOperation.Contains,
					QueryFilterOperation.DoesNotContain,
					QueryFilterOperation.StartsWith,
					QueryFilterOperation.EndsWith
				};

				return validOperations;
			}
		}
	}

	// --- Query Match Integers --- 

	public class QueryMatchInteger : QueryMatch
	{
		private SpinButton spinButton1, spinButton2;
		private HBox rangeBox;

		public override string FilterValues()
		{
			UpdateValues();
		
			switch(Filter) {
				case QueryFilterOperation.Is:
					return Column + " = '" + Value1 + "'";
				case QueryFilterOperation.IsNot:
					return Column + " != '" + Value1 + "'";
				case QueryFilterOperation.IsLessThan:
					return Column + " > '" + Value1 + "'";
				case QueryFilterOperation.IsGreaterThan:
					return Column + " < '" + Value1 + "'";
				case QueryFilterOperation.IsInTheRange:
					return String.Format ("({0} >= {1} AND {0} <= {2})", Column, Value1, Value2);
			}
			
			return null;
		}
		
		public override void UpdateValues()
		{
			if(spinButton1 == null)
				throw new Exception("Display Widget was never Set");
				
			Value1 = spinButton1.ValueAsInt.ToString ();
			
			if(spinButton2 != null)
			    Value2 = spinButton2.ValueAsInt.ToString ();
		}
		
		public override Widget DisplayWidget
		{
			get {
				if(spinButton1 == null) {
					spinButton1 = new SpinButton(Int32.MinValue, Int32.MaxValue, 1.0);
                    spinButton1.Value = 1.0;
                    spinButton1.Digits = 0;
                    spinButton1.WidthChars = 2;
					spinButton1.Show();
				}
				
				if(Filter != QueryFilterOperation.IsInTheRange) {
					if(rangeBox != null && spinButton2 != null) {
						rangeBox.Remove(spinButton1);
						rangeBox.Remove(spinButton2);
						
						spinButton2.Destroy();
						spinButton2 = null;
						rangeBox.Destroy();
						rangeBox = null;
					}
				
					return spinButton1;
				}
				
				if(spinButton2 == null) {
					spinButton2 = new SpinButton(Int32.MinValue, Int32.MaxValue, 1.0);
                    spinButton2.Value = 1.0;
                    spinButton2.Digits = 0;
                    spinButton2.WidthChars = 2;
					spinButton2.Show();
				}
				
				rangeBox = BuildRangeBox(spinButton1, spinButton2);
				return rangeBox;
			}
		}
		
		public override string [] ValidOperations
		{
			get {	
				string [] validOperations = {
					QueryFilterOperation.Is,
					QueryFilterOperation.IsNot,
					QueryFilterOperation.IsBefore,
					QueryFilterOperation.IsAfter,
					QueryFilterOperation.IsInTheRange
				};

				return validOperations;
			}
		}
	}
	
	// --- Query Match Date --- 
		
	public class QueryMatchDate : QueryMatch
	{
		private DateButton dateButton1;
		private DateButton dateButton2;
		private HBox rangeBox;

		public override string FilterValues()
		{
			UpdateValues();
		
			string pv = Statement.EscapeQuotes(Value1), pv2 = Value2;
			if(pv2 != null)
				pv2 = Statement.EscapeQuotes(Value1);
			
			switch(Filter) {
				case QueryFilterOperation.Is:
					return Column + " = '" + pv + "'";
				case QueryFilterOperation.IsNot:
					return Column + " != '" + pv + "'";
				case QueryFilterOperation.IsBefore:
					return Column + " < '" + pv + "'";
				case QueryFilterOperation.IsAfter:
					return Column + " > '" + pv + "'";
				case QueryFilterOperation.IsInTheRange:
					return "(" + Column + " >= '" + pv + "' AND " 
						+ Column + " <= '" + pv2 + "')";
			}
			
			return null;
		}
		
		public override void UpdateValues()
		{
			if(dateButton1 == null)
				throw new Exception("Display Widget was never Set");
				
			Value1 = dateButton1.Date.ToString("yyyy-MM-dd");
			
			if(dateButton2 != null)
				Value2 = dateButton2.Date.ToString("yyyy-MM-dd");
		}
		
		public override Widget DisplayWidget
		{
			get {
				if(dateButton1 == null) {
					dateButton1 = new DateButton("<Select Date>");
					dateButton1.Show();
				}
				
				if(Filter != QueryFilterOperation.IsInTheRange) {
					if(rangeBox != null && dateButton2 != null) {
						rangeBox.Remove(dateButton1);
						rangeBox.Remove(dateButton2);
						
						dateButton2.Destroy();
						dateButton2 = null;
						rangeBox.Destroy();
						rangeBox = null;
					}
				
					return dateButton1;
				}
				
				if(dateButton2 == null) {
					dateButton2 = new DateButton("<Select Date>");
					dateButton2.Show();
				}
				
				rangeBox = BuildRangeBox(dateButton1, dateButton2);
				return rangeBox;
			}
		}
		
		public override string [] ValidOperations
		{
			get {	
				string [] validOperations = {
					QueryFilterOperation.Is,
					QueryFilterOperation.IsNot,
					QueryFilterOperation.IsBefore,
					QueryFilterOperation.IsAfter,
					QueryFilterOperation.IsInTheRange
				};

				return validOperations;
			}
		}
	}
	
	public class TracksQueryModel : QueryBuilderModel
	{
		private Hashtable fields;
		
		public TracksQueryModel() : base()
		{
			AddField("Title", "Title", typeof(QueryMatchString));
			AddField("Artist", "Artist", typeof(QueryMatchString));
			AddField("Album", "Album", typeof(QueryMatchString));
			AddField("Song Name", "Title", typeof(QueryMatchString));
			AddField("Genre", "Genre", typeof(QueryMatchString));
			AddField("Date Added", "DateAddedStamp", typeof(QueryMatchDate));
			AddField("Last Played", "LastPlayedStamp", typeof(QueryMatchDate));
			AddField("Number of Plays", "NumberOfPlays", typeof(QueryMatchInteger));
			AddField("Rating", "Rating", typeof(QueryMatchInteger));
			//AddField("Year", "Year", typeof(QueryMatchInteger));
			
			AddOrder(QuerySelectedByCriteria.Random, "RAND()");
			AddOrder(QuerySelectedByCriteria.Album, "Album");
			AddOrder(QuerySelectedByCriteria.Artist, "Artist");
			AddOrder(QuerySelectedByCriteria.Genre, "Genre");
			AddOrder(QuerySelectedByCriteria.SongName, "Title");
			AddOrder(QuerySelectedByCriteria.HighestRating, "Rating DESC");
			AddOrder(QuerySelectedByCriteria.LowestRating, "Rating ASC");
			AddOrder(QuerySelectedByCriteria.LeastOftenPlayed, "NumberOfPlays ASC");
			AddOrder(QuerySelectedByCriteria.MostOftenPlayed, "NumberOfPlays DESC");
			AddOrder(QuerySelectedByCriteria.MostRecentlyAdded, "DateAddedStamp DESC");
			AddOrder(QuerySelectedByCriteria.LeastRecentlyAdded, "DateAddedStamp ASC");
		}

		public override string [] LimitCriteria 
		{
			get {
				string [] criteria = {
					QueryLimitCriteria.Songs,
					QueryLimitCriteria.Minutes,
					QueryLimitCriteria.Hours
				};
				
				return criteria;
			}
		}
	}

	public class SqlBuilderUI
	{
		private QueryBuilder builder;
		private TracksQueryModel model;
	
		public SqlBuilderUI()
		{
			Window win = new Window("SQL Builder");
			win.Show();
			win.BorderWidth = 10;
			win.Resizable = false;
			
			VBox box = new VBox();
			box.Show();
			win.Add(box);
			box.Spacing = 10;
			
			model = new TracksQueryModel();
			builder = new QueryBuilder(model);
			builder.Show();
			builder.Spacing = 4;
			
			box.PackStart(builder, true, true, 0);
			
			Button btn = new Button("Generate Query");
			btn.Show();
			box.PackStart(btn, false, false, 0);
			btn.Clicked += OnButtonClicked;	
		}
		
		private void OnButtonClicked(object o, EventArgs args)
		{
			string query = "SELECT * FROM Tracks";
			
			query += builder.MatchesEnabled ?
				" WHERE" + builder.MatchQuery : " ";
			
			query += "ORDER BY " + builder.OrderBy;
			
			if(builder.Limit && builder.LimitNumber > 0)
				query += " LIMIT " + builder.LimitNumber;
			
			Console.WriteLine(query);
		}
	}
}
