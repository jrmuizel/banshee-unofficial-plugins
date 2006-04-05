
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
using Mono.Unix;

using Banshee.Widgets;

namespace Banshee
{
	public sealed class QueryFilterOperation
	{
        private string name, format;
        public string Name {
            get { return name; }
        }

        private static Hashtable filters = new Hashtable();
        public static QueryFilterOperation GetByName (string name)
        {
            return filters[name] as QueryFilterOperation;
        }

        private static QueryFilterOperation NewOperation (string name, string format)
        {
            QueryFilterOperation op = new QueryFilterOperation(name, format);
            filters[name] = op;
            return op;
        }

        private QueryFilterOperation (string name, string format)
        {
            this.name = name;
            this.format = format;
        }

        public string FilterSql (bool text, string column, string value1, string value2)
        {
            if (text)
                return String.Format (format, "'", column, value1, value2);
            else
                return String.Format (format, "", column, value1, value2);
        }


		public static QueryFilterOperation Is = NewOperation (
            Catalog.GetString ("is"),
            "{1} = {0}{2}{0}"
        );

		public static QueryFilterOperation IsNot = NewOperation (
            Catalog.GetString ("is not"),
            "{1} != {0}{2}{0}"
        );

		public static QueryFilterOperation IsLessThan = NewOperation (
            Catalog.GetString ("is less than"),
            "{1} < {0}{2}{0}"
        );

		public static QueryFilterOperation IsGreaterThan = NewOperation (
            Catalog.GetString ("is greater than"),
            "{1} > {0}{2}{0}"
        );

		public static QueryFilterOperation Contains = NewOperation (
            Catalog.GetString ("contains"),
            "{1} LIKE '%{2}%'"
        );

		public static QueryFilterOperation DoesNotContain = NewOperation (
            Catalog.GetString ("does not contain"),
            "{1} NOT LIKE '%{2}%'"
        );

		public static QueryFilterOperation StartsWith = NewOperation (
            Catalog.GetString ("starts with"),
            "{1} LIKE '{2}%'"
        );

		public static QueryFilterOperation EndsWith = NewOperation (
            Catalog.GetString ("ends with"),
            "{1} LIKE '%{2}'"
        );

		public static QueryFilterOperation IsBefore = NewOperation (
            Catalog.GetString ("is before"),
            "{1} < {0}{2}{0}"
        );

		public static QueryFilterOperation IsAfter = NewOperation (
            Catalog.GetString ("is after"),
            "{1} > {0}{2}{0}"
        );

		public static QueryFilterOperation IsInTheRange = NewOperation (
            Catalog.GetString ("is in the range"),
            "({1} >= {0}{2}{0} AND {1} <= {0}{3}{0})"
        );
	}
	
	public sealed class QuerySelectedByCriteria
	{
		public static string Random = Catalog.GetString("Random");
		public static string Album = Catalog.GetString("Album");
		public static string Artist = Catalog.GetString("Artist");
		public static string Genre = Catalog.GetString("Genre");
		public static string SongName = Catalog.GetString("Song Name");
		public static string HighestRating = Catalog.GetString("Highest Rating");
		public static string LowestRating = Catalog.GetString("Lowest Rating");
		public static string LeastOftenPlayed = Catalog.GetString("Least Often Played");
		public static string MostOftenPlayed = Catalog.GetString("Most Often Played");
		public static string MostRecentlyAdded = Catalog.GetString("Most Recently Added");
		public static string LeastRecentlyAdded = Catalog.GetString("Least Recently Added");
	}
	
	public sealed class QueryLimitCriteria
	{
		public static string Songs = Catalog.GetString("songs");
		public static string Minutes = Catalog.GetString("minutes");
		public static string Hours = Catalog.GetString("hours");
	}

	// --- Query Match String --- 
	
	public class QueryMatchString : QueryMatch
	{
		private Entry dispEntry;

		public override string FilterValues()
		{
			UpdateValues();
			
			string val1 = Statement.EscapeQuotes(Value1.ToLower());
            string col = String.Format("lower({0})", Column);
			
            QueryFilterOperation op = QueryFilterOperation.GetByName (Filter);
            if (op == null)
                return null;
            else
                return op.FilterSql (true, col, val1, null);
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
		
		public override QueryFilterOperation [] ValidOperations
		{
			get {	
				QueryFilterOperation [] validOperations = {
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

            QueryFilterOperation op = QueryFilterOperation.GetByName (Filter);
            if (op == null)
                return null;
            else
                return op.FilterSql (false, Column, Value1, Value2);
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
				
				if(QueryFilterOperation.GetByName(Filter) != QueryFilterOperation.IsInTheRange) {
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
		
		public override QueryFilterOperation [] ValidOperations
		{
			get {	
				QueryFilterOperation [] validOperations = {
					QueryFilterOperation.Is,
					QueryFilterOperation.IsNot,
					QueryFilterOperation.IsLessThan,
					QueryFilterOperation.IsGreaterThan,
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
		
			string pv = Statement.EscapeQuotes(Value1), pv2 = null;
			if(Value2 != null)
				pv2 = Statement.EscapeQuotes(Value2);

            QueryFilterOperation op = QueryFilterOperation.GetByName (Filter);
            if (op == null)
                return null;
            else
                return op.FilterSql (true, Column, pv, pv2);
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
				
				if(QueryFilterOperation.GetByName(Filter) != QueryFilterOperation.IsInTheRange) {
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
		
		public override QueryFilterOperation [] ValidOperations
		{
			get {	
				QueryFilterOperation [] validOperations = {
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
					QueryLimitCriteria.Songs
					//QueryLimitCriteria.Minutes,
					//QueryLimitCriteria.Hours
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
			
			if(builder.Limit && builder.LimitNumber != "0")
				query += " LIMIT " + builder.LimitNumber;
			
			Console.WriteLine(query);
		}
	}
}
